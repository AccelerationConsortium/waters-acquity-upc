#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    STFServiceHandler.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using CommonSTF;
using CommonSTF.Tools; 
using CommonSTF.Tools.Configuration;
using CommonSTF.Tools.Enums;
using CommonSTF.Tools.Loggers; 
using CommonSTF.FileHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CommonSTF
{
    /// <summary>
    ///  Main class of the project, representing the windows service which runs continuously process jobs from JSON files.
    ///  Upon  starting it will read setting from configuration file  and will do the preliminary check for folder access (read/write) rights.
    /// </summary>
    public class STFServiceHandler
    {
        #region Privates
        private Timer _timer;

        private int _timeCheckIntervalMls;
        private int _instrumentConnectionTimeOut = 4;
        private string _STF_JsonDirectoryPath = "";
        private string _STF_JsonDirectoryUserName = "";
        private string _STF_JsonDirectoryPassword = "";
        private string _STF_JsonDirectoryDomain = "";
        private string _serviceId;
        private bool _empowerJSONpwEncrypted;

        private const string FAILED_TEXT = "Failed";
        private const string COMPLETED_TEXT = "Completed";         

        private NetworkFolderLogonWrappedImpersonationContext _impersonationContext = null;

        private IConfig _configReader;
        private ILogger _logger;
        private int _runMode;
        private FolderAccessChecker _folderAccessChecker;

        private string _serviceName; 

        private bool _busy = false;  

        private FileUtils _fileUtils;
        private JsonUtils _jsonUtils;
        private JsonValidator _jsonValidator;
        private string _defaultComment;
        #endregion

        #region Constructors
        public STFServiceHandler(string serviceName, IConfig configReader, ILogger multiLogger)
        {
            _configReader = configReader;
            _logger = multiLogger;
            _serviceName = serviceName;
            _folderAccessChecker = new FolderAccessChecker(_logger);
            _jsonUtils = new JsonUtils(_logger);
            _jsonValidator = new JsonValidator(_logger);
            _fileUtils = new FileUtils(_logger);
        } 
        #endregion

        #region Public methods
        /// <summary>
        /// Start service, read config values and check folder/file permissions.
        /// </summary>
        public void StartService()
        {
            try
            {               
                _logger.LogInfo($"StartService(): Reading {EnumUtil.GetName(ConfigKeys.TimerInterval)} ...");

                ReadConfiguration();

                CheckAccessToFolder();                 

                _timer = new Timer { Interval = _timeCheckIntervalMls };
                _timer.Elapsed += Timer_Elapsed;
                _timer.Enabled = true;

                var msg = $"Service started: {_serviceName}, serviceId: {_serviceId}, with timer interval: {_timer.Interval / 60000}min";
                _logger.LogInfo(msg);

                _timer.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError($"The Elapsed event exception: {ex}");
            }
        }

        public void StopService()
        {
            _timer.Enabled = false;
            _timer.Dispose();
            _timer = null;

            if (_impersonationContext != null)
            {
                _impersonationContext.Leave();
            }

            _logger.LogInfo($"Service stopped: {_serviceName}, serviceId: {_serviceId}");
        }
        #endregion

        #region Private events        
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _logger.LogInfo($"Timer_Elapsed: entering... previous busy flag was set to {_busy.ToString()}...");
            if (_busy)//wait for the previous active check if there is any
            {
                _logger.LogInfo($"Timer_Elapsed: service is busy, previous iteration not finished yet... skipping this iteration...");
                return;
            }
            _logger.LogInfo($"Timer_Elapsed: service is not busy... going straight to STF file process..");
             
            _busy = true;
            _logger.LogInfo($"Timer_Elapsed: starting... setting iteration busy flag to {_busy.ToString()}...");
            try
            {
                JsonObject[] newFiles = _fileUtils.GetNewFilesForProcessingInTopFolderSortedAscByDate(_STF_JsonDirectoryPath);

                foreach (JsonObject file in newFiles)
                {
                    JsonObject ob = null;


                    try
                    {

                        _fileUtils.RenameFileAsLocked(file);
                        ob = _jsonUtils.Deserialize(file.CurrentFileName, _empowerJSONpwEncrypted, out string error);

                        // If deserialization process finished successfully.
                        if (ob != null)
                        {
                            _defaultComment = $"STF service [{_serviceId}] automatic action following Integrator [{ob.IntegratorID}] command";
                            if (_jsonValidator.ValidateJSONObject(ob, out error))
                            {
                                // Try to login.
                                EmpowerProject ep = new EmpowerProject(_logger);
                                LoginData loginData = new LoginData(
                                    ob.HeaderFields.EmpowerUn, ob.HeaderFields.EmpowerPw,
                                    ob.HeaderFields.EmpowerDatabase, ob.HeaderFields.EmpowerProject);

                                if (ep.Login(loginData, out string errorMsgLogin))
                                {
                                    ep.DefaultAuditTrailComment = _defaultComment;
                                    //ep.CreateAuditTrailEntry(_defaultComment, out string _auditTrailError);
                                    int processed = 0;
                                    int succedded = 0;

                                    EmpowerInstrument instrument = new EmpowerInstrument(_logger);
                                    if (instrument.ConnectToInstrumentAndWaitForConnection(
                                            ob.HeaderFields.Node, ob.HeaderFields.System,
                                            _instrumentConnectionTimeOut, out string instrumentConnectionError))
                                    {
                                        List<SampleSetQueueEntryData> ssQueue = instrument.SampleSetQueueEntries;
                                        foreach (SampleSetDetailsData ssdDetails in ob.SampleSetDetails)
                                        {
                                            ++processed;
                                            if (ProcessSSM(instrument, loginData, ssQueue, ep, ssdDetails, out string sampleSetMethodRunReport))
                                            {
                                                ssdDetails.Status = COMPLETED_TEXT;
                                                ++succedded;
                                            }
                                            else
                                            {
                                                ssdDetails.Status = FAILED_TEXT;
                                            }

                                            ssdDetails.SampleSetMethodRunReport = sampleSetMethodRunReport;
                                        }

                                        ob.TrailerReport.FileVerified = true;
                                        ob.TrailerReport.FileProcessed = true;
                                        ob.TrailerReport.FileStatus = COMPLETED_TEXT;
                                        ob.TrailerReport.FileProcessReport = $"File completed '{processed}' ssm processed, {succedded} succeeded, {(processed - succedded)} failed.";

                                        _fileUtils.RenameFileAsProcessed(file);
                                        _fileUtils.SaveFile(file.CurrentFileName, _jsonUtils.Serialize(ob, out error));

                                        instrument.Disconnect();
                                    }
                                    else
                                    {
                                        ob.TrailerReport.FileVerified = true;
                                        ob.TrailerReport.FileProcessed = true;
                                        ob.TrailerReport.FileStatus = FAILED_TEXT;
                                        ob.TrailerReport.FileProcessReport = $"File not processed - Cannot connect to system. Error: {instrumentConnectionError}";

                                        _fileUtils.RenameFileAsError(file);
                                        _fileUtils.SaveFile(file.CurrentFileName, _jsonUtils.Serialize(ob, out error));
                                    }

                                    if (!ep.Logoff(out string errorLogOff))
                                    {
                                        _logger.LogError($"Cannot logoff project {loginData.Project}, error: {errorLogOff}");
                                    }
                                }
                                else
                                {
                                    ob.TrailerReport.FileVerified = true;
                                    ob.TrailerReport.FileProcessed = true;
                                    ob.TrailerReport.FileStatus = FAILED_TEXT;
                                    ob.TrailerReport.FileProcessReport = $"File not processed - login failure: {errorMsgLogin}";

                                    _fileUtils.RenameFileAsError(file);
                                    _fileUtils.SaveFile(file.CurrentFileName, _jsonUtils.Serialize(ob, out error));
                                }
                            }
                            else
                            {
                                _fileUtils.RenameFileAsError(file);
                                _logger.LogError($"JSON file Validation error: {error}");
                                if (ob.TrailerReport != null)
                                {
                                    ob.TrailerReport.FileVerified = true;
                                    ob.TrailerReport.FileProcessed = true;
                                    ob.TrailerReport.FileStatus = FAILED_TEXT;
                                    ob.TrailerReport.FileProcessReport = $"File not processed - validation failure: {error}";
                                    _fileUtils.SaveFile(file.CurrentFileName, _jsonUtils.Serialize(ob, out error));
                                }
                            }

                        }
                        else
                        {
                            _fileUtils.RenameFileAsErrorDeserialization(file);
                            _logger.LogError($"File not processed - deserialzation failure: {error}, {file.CurrentFileName}", true);
                        }
                    }
                    catch (Exception exInner)
                    {
                        if (ob != null)
                        {
                            _fileUtils.RenameFileAsError(file);
                            _logger.LogError($"Error while processing single JSON file {file.CurrentFileName}", exInner);
                        }
                        else
                        {
                            _logger.LogError($"Error while processing single JSON file", exInner);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Timer exception: {ex}");
                _busy = false;
                _logger.LogInfo($"Timer_Elapsed: exc occurred... setting busy flag to {_busy.ToString()}");
            }
            finally
            {
                _busy = false;
                _logger.LogInfo($"Timer_Elapsed: finally... setting busy flag to {_busy.ToString()}");
            }
            _busy = false;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Process single SampleSet from JSON file.
        /// </summary>
        /// <param name="instrument">System to connect to</param>
        /// <param name="currentLogin">Current login information</param>
        /// <param name="ssQueue">Instrument queue</param>
        /// <param name="ep">Empower Project we are logged in</param>
        /// <param name="sampleSetDetailsFromJson">Single sample set from JSON</param>
        /// <param name="sampleSetMethodRunReport">Return running type of job (Run, RunAndProcess, RunAndReport)</param>
        /// <returns></returns>
        private bool ProcessSSM(EmpowerInstrument instrument, LoginData currentLogin, List<SampleSetQueueEntryData> ssQueue, 
            EmpowerProject ep,  SampleSetDetailsData sampleSetDetailsFromJson, out string sampleSetMethodRunReport)
        {
            var essm = new EmpowerSampleSetMethod(_logger);

            // IIf this is new job treat as new one.
            if (sampleSetDetailsFromJson.New)
            {
                return NewSSMRunOrReplace(instrument, essm, sampleSetDetailsFromJson,
                   sampleSetDetailsFromJson.BaseSampleSetMethodName, sampleSetDetailsFromJson.SampleSetName, out sampleSetMethodRunReport, false);
            }
            else // If new one flag is false, then treat us update.
            {
                #region check queue 
                // First check if it is Queue and if there is still Inject Sample lines to run - compared with current running.
                    ssQueue = ssQueue.OrderBy(u => u.JobId).ToList<SampleSetQueueEntryData>();

                    for (int i = 0; i < ssQueue.Count; ++i)
                    {
                        SampleSetQueueEntryData sampleSetInQueue = ssQueue[i];
                    if ((sampleSetInQueue.SampleSetName == sampleSetDetailsFromJson.SampleSetName) && // If same SSM already exist in queue or running, then replace currently running or queue.
                        (sampleSetInQueue.Project == currentLogin.Project))// Ignore if SampleSetMethod with same name exist in queue, but is from different project.
                    {
                        if (i == 0)
                        { 
                            bool currentJobIsSameAsInJson = instrument.GetCurrentlyRunningJob(
                                essm, ssQueue, out string currentlyRunningVial,
                                out bool currentSSMRunningAdded, currentLogin, ep,
                                out string firstVial, out string lastVial,
                                true, null, false, out List<SampleSetMethodLineData> lines,
                                out int currentSSMLineNumber);// If is currently running.

                         

                            if (currentJobIsSameAsInJson)
                            {
                                // Pause current job, create changes and resume job.
                                return UpdateCurrentRunningSSM(instrument, essm,  
                                    sampleSetDetailsFromJson,
                                     lines, currentSSMLineNumber,
                                    out sampleSetMethodRunReport);
                            } 
                        }
                        else// If not running but is in queue, remove complete Sample Set and save and run updated one.
                        {
                            return UpdateSsmInQueue(instrument, essm, sampleSetInQueue, sampleSetDetailsFromJson, out sampleSetMethodRunReport);
                        }
                    }
                }
                #endregion Check queue

                #region Not in queue
                // We checked all, but this SSM is not in queue but has been already done.
                return UpdateSsm(instrument, essm, sampleSetDetailsFromJson,
                        sampleSetDetailsFromJson.BaseSampleSetMethodName, sampleSetDetailsFromJson.SampleSetName, out sampleSetMethodRunReport);
                    #endregion Not in queue
               
            }
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="instrument"></param>
       /// <param name="essm"></param>
       /// <param name="sampleSetInQueue"></param>
       /// <param name="sampleSetDetailsFromJson"></param>
       /// <param name="sampleSetMethodRunReport"></param>
       /// <returns></returns>
        private bool UpdateSsmInQueue(EmpowerInstrument instrument, EmpowerSampleSetMethod essm,
            SampleSetQueueEntryData sampleSetInQueue, SampleSetDetailsData sampleSetDetailsFromJson, out string sampleSetMethodRunReport)
        {
            if (instrument.RemoveSSMFromQueue(sampleSetInQueue.JobId, out string errorRemove))
            {
                _logger.LogInfo($"Removing SSM:{sampleSetInQueue.SampleSetName} with JobId:{sampleSetInQueue.JobId} was successful.");

                //  sampleSetDetailsFromJson.SampleSetName == sampleSetInQueue.SampleSetName must be equal.
                return NewSSMRunOrReplace(instrument, essm, sampleSetDetailsFromJson, sampleSetInQueue.SampleSetName, sampleSetInQueue.SampleSetName, out sampleSetMethodRunReport, false);
            }
            else
            {
                sampleSetMethodRunReport = $"Removing SSM:{sampleSetInQueue.SampleSetName} with JobId:{sampleSetInQueue.JobId} was NOT successful. Error:{errorRemove}";
                _logger.LogError(sampleSetMethodRunReport);
                return false;
            }
        }

        private bool UpdateCurrentRunningSSM(EmpowerInstrument instrument, EmpowerSampleSetMethod essm,
            SampleSetDetailsData sampleSetDetailsFromJson,
            List<SampleSetMethodLineData> lines, int currentSSMLineNumber, out string sampleSetMethodRunReport)
        {
            // If current running line < last line of Inject Samples in existing ssm then pause ssm, treat as New_SSM, resume. If not, treat as UPDATE.
            int lastLineOfInjectSample = 0;
            for (int i = 0; i < lines.Count; ++i)
            {
                if (lines[i].FunctionName == ToolkitUtils.GetInjectSampleName())
                {
                    lastLineOfInjectSample = i;
                }
            }
            if (currentSSMLineNumber != int.MinValue && currentSSMLineNumber < lastLineOfInjectSample)
            {
                // Need to pause for enough time to process job update and resume.
                instrument.Pause(60, out string error);
                return NewSSMRunOrReplace(instrument, essm, sampleSetDetailsFromJson,
                   sampleSetDetailsFromJson.BaseSampleSetMethodName,
                   sampleSetDetailsFromJson.SampleSetName, out sampleSetMethodRunReport, true);
            }
            else
            {
                return UpdateSsm(instrument, essm, sampleSetDetailsFromJson,  sampleSetDetailsFromJson.BaseSampleSetMethodName, 
                    sampleSetDetailsFromJson.SampleSetName,  out sampleSetMethodRunReport);
            } 
        }         

        private bool UpdateSsm(EmpowerInstrument instrument, EmpowerSampleSetMethod essm,
                 SampleSetDetailsData sampleSetDetailsFromJson, string templateSSMName, 
                    string SsmToUpdateName, out string sampleSetMethodRunReport)
        { 
            // Check if exist in project.
            GenericMethodData ssmDetails = essm.GetSampleSetMethodDetails(SsmToUpdateName);
            if (ssmDetails == null)
            {
                sampleSetMethodRunReport = $"SSM to update with name {SsmToUpdateName} does not exist in the project.";
                _logger.LogError(sampleSetMethodRunReport);
                return false;
            }            

            List<SampleSetMethodLineData> linesFromEmpowerTemplate = essm.GetSampleSetLines(ssmDetails.ID, out string error, out string[] nonExistingCustomColumn, null, true);

            ssmDetails.Comments += _defaultComment;
            ssmDetails.Name = SsmToUpdateName;

            int numberOfInjectSamplesFromJson = sampleSetDetailsFromJson.Samples.Where(u => (u[SampleSetMethodLineField.FUNCTION].ToString() == ToolkitUtils.GetInjectSampleName())).ToList().Count();
            int numberOfInjectSamplesFromEmpower = linesFromEmpowerTemplate.Where(u => (u.FunctionName.ToString() == ToolkitUtils.GetInjectSampleName())).ToList().Count();
            int numberOfInjectLinesToUpdate = numberOfInjectSamplesFromJson - numberOfInjectSamplesFromEmpower;
            if (numberOfInjectLinesToUpdate > 0)
            {
                // Remove all previously processed inject sample lines.
                List<Dictionary<string, object>> ssmLinesFromJson =
                    sampleSetDetailsFromJson.Samples.OrderBy(u => (Convert.ToInt32(u[JsonUtils.LineNumberLabel].ToString()))).ToList();

                List<Dictionary<string, object>> ssmLinesToRemove = new List<Dictionary<string, object>>();
                foreach (Dictionary<string, object> tempLine in ssmLinesFromJson)
                {
                    tempLine.TryGetValue(SampleSetMethodLineField.FUNCTION, out object value);
                    string functionValue = value.ToString();
                    if (functionValue != ToolkitUtils.GetInjectSampleName()) // Remove All non inject sample lines when updating.
                    {
                        ssmLinesToRemove.Add(tempLine);
                    }
                    else if (numberOfInjectLinesToUpdate > 0) // Inject Sample line. Leave only new one for update.
                    {
                        --numberOfInjectLinesToUpdate;
                        ssmLinesToRemove.Add(tempLine);                        
                    }
                }
                foreach(var lineToRemove in ssmLinesToRemove)
                    ssmLinesFromJson.Remove(lineToRemove);

                sampleSetDetailsFromJson.Samples = ssmLinesFromJson;
                // Treat as new.
                return NewSSMRunOrReplace(instrument, essm, sampleSetDetailsFromJson, templateSSMName, SsmToUpdateName, out sampleSetMethodRunReport, false);
            }

            sampleSetMethodRunReport = $"SSM for update has no new inject lines: {SsmToUpdateName}.";
            _logger.LogWarning(sampleSetMethodRunReport, true);
            return false;
        }


        private bool  NewSSMRunOrReplace(EmpowerInstrument instrument, EmpowerSampleSetMethod essm,
         SampleSetDetailsData sampleSetDetailsFromJson, string templateSSMName, string newSSMName, out string sampleSetMethodRunReport, bool replaceCurrentJobWithNewSSM)
        {
            GenericMethodData ssmDetails = essm.GetSampleSetMethodDetails(templateSSMName);
            if (ssmDetails == null)
            {
                sampleSetMethodRunReport = $"BaseSampleSetMethodName '{templateSSMName}' does not exist in the project.";
                _logger.LogError(sampleSetMethodRunReport);
                return false;
            }

            List<SampleSetMethodLineData> linesFromEmpowerTemplate = essm.GetSampleSetLines(ssmDetails.ID, out string error, out string[] nonExistingCustomColumn, null, true);
           
            ssmDetails.Comments += _defaultComment;
            ssmDetails.Name = newSSMName;

            List<SampleSetMethodLineData> updatedLines = new List<SampleSetMethodLineData>();

            IList<Dictionary<string, object>> sampleSetDetailsLinesFromJsonOrdered = sampleSetDetailsFromJson.Samples.ToList().
                OrderBy(u => (Convert.ToInt32(u[JsonUtils.LineNumberLabel].ToString()))).ToList();
 
             
            SampleSetMethodLineData line; 

            int lastNonInjIndex = 0;
            lastNonInjIndex =    CopyNonInjectionLinesFromEmpower(linesFromEmpowerTemplate, updatedLines, lastNonInjIndex);

            // Process Json lines.
            foreach(Dictionary<string, object> keyValuePair in sampleSetDetailsLinesFromJsonOrdered)
            {
                // Get same (first) line with same function from empower to copy object.
                SampleSetMethodLineData ssmLinesWithSameFunctionFromEmpower = 
                    linesFromEmpowerTemplate.Where(u => (u.FunctionName) == 
                    keyValuePair[SampleSetMethodLineField.FUNCTION].ToString()).FirstOrDefault();
                if (ssmLinesWithSameFunctionFromEmpower == null)
                {
                    sampleSetMethodRunReport = $"SSM line with function name '{keyValuePair[SampleSetMethodLineField.FUNCTION].ToString()}' does not exist in empower for {templateSSMName}. At least one line from base SSM with equal function name is required.";
                    _logger.LogError(sampleSetMethodRunReport);
                    return false;
                }

                line = (SampleSetMethodLineData)ssmLinesWithSameFunctionFromEmpower.Clone(); 

                foreach (KeyValuePair<string, object> field in keyValuePair)
                {
                    // JsonUtils.LineNumberLabel is only for sorting.
                    if (field.Key != JsonUtils.LineNumberLabel)
                    {
                        var valuesFromJson = new KeyValuePair<string, string>(field.Key,
                            field.Value == null ? "" : field.Value.ToString());
                        line.nonStandardAdditionalColumns.Add(valuesFromJson);
                    }
                }

                updatedLines.Add(line);
            }

            // Copy last set of non inj lines.
            CopyNonInjectionLinesFromEmpower(linesFromEmpowerTemplate, updatedLines, lastNonInjIndex);
                  
            if (essm.SaveSampleSetMethodAndLines(ssmDetails, updatedLines, out string errorMessage))
            {
                _logger.LogInfo($"Saving successful:{newSSMName}. Adding update SSM to queue.", true);
                if (replaceCurrentJobWithNewSSM)
                {
                    if (instrument.Replace(newSSMName, out string errorReplace))
                    {
                        sampleSetMethodRunReport = $"SSM stored and replaced successfully to Empower: {newSSMName}.";
                        _logger.LogInfo(sampleSetMethodRunReport, true);
                        return true;
                    }
                    else
                    {
                        sampleSetMethodRunReport = $"Successfully stored SSM, but error occurred while replacing Empower current job:{newSSMName}. Error: {errorReplace}.";
                        _logger.LogError(sampleSetMethodRunReport, true);
                        return false;
                    }
                }
                else
                {
                    if (instrument.Run(newSSMName, newSSMName, out string errorRun, _runMode))
                    {
                        sampleSetMethodRunReport = $"SSM stored and submitted successfully to Empower: {newSSMName}.";
                        _logger.LogInfo(sampleSetMethodRunReport, true);
                        return true;
                    }
                    else
                    {
                        sampleSetMethodRunReport = $"Successfully stored SSM, but error occurred while adding to Empower queue:{newSSMName}. Error: {errorRun}.";
                        _logger.LogError(sampleSetMethodRunReport, true);
                        return false;
                    }
                }
            }
            else
            {
                sampleSetMethodRunReport = $"Cannot save SSM: {newSSMName}. Error: {errorMessage}.";
                _logger.LogError(sampleSetMethodRunReport, true);
                return false;
            }
        }

        private int CopyNonInjectionLinesFromEmpower(List<SampleSetMethodLineData> linesFromEmpowerTemplate, List<SampleSetMethodLineData> updatedLines, int startIndex)
        {
            bool firstSet = false;
            if (startIndex == 0)
            {
                firstSet = true;
            }
            
            for (; startIndex < linesFromEmpowerTemplate.Count; ++startIndex)
            {
                SampleSetMethodLineData nonInj = linesFromEmpowerTemplate[startIndex];
                if (firstSet  && (nonInj.InjVol.HasValue && nonInj.InjVol >= 0))// Skip changing non injection lines (keep it same as in original one)
                {
                    // If first set of non Injection lines, break after you get first with inj.
                    break;
                }
                else if (!(nonInj.InjVol.HasValue && nonInj.InjVol >= 0))
                {
                    // Non - injection lines.
                    updatedLines.Add(nonInj);
                }
            }
            return startIndex;
        }

        private bool ReadConfiguration()
        {
            _timeCheckIntervalMls = _configReader.GetIntValue(ConfigKeys.TimerInterval, out bool valueExist);
            if (!valueExist)
            {                
                _logger.LogError(EnumUtil.GetName(ConfigKeys.TimerInterval) + " not exist in config!");
                throw new Exception(EnumUtil.GetName(ConfigKeys.TimerInterval) + " not exist in config!");
            }
            _logger.LogInfo($"StartService(): Reading {EnumUtil.GetName(ConfigKeys.TimerInterval)}={_timeCheckIntervalMls}");


            _runMode = _configReader.GetIntValue(ConfigKeys.RunMode, out valueExist);
            if (!valueExist)
            {
                _logger.LogError(EnumUtil.GetName(ConfigKeys.RunMode) + " not exist in config!");
                throw new Exception(EnumUtil.GetName(ConfigKeys.RunMode) + " not exist in config!");
            }
            _logger.LogInfo($"StartService(): Reading {EnumUtil.GetName(ConfigKeys.RunMode)}={_runMode}");


            _instrumentConnectionTimeOut = _configReader.GetIntValue(ConfigKeys.InstrumentConnectionTimeOut, out valueExist);
            if (!valueExist)
            {
                _logger.LogError(EnumUtil.GetName(ConfigKeys.InstrumentConnectionTimeOut) + " not exist in config!");
                throw new Exception(EnumUtil.GetName(ConfigKeys.InstrumentConnectionTimeOut) + " not exist in config!");
            }
            _logger.LogInfo($"StartService(): Reading {EnumUtil.GetName(ConfigKeys.InstrumentConnectionTimeOut)}={_instrumentConnectionTimeOut}");

            _STF_JsonDirectoryPath = _configReader.GetStringValue(ConfigKeys.STF_JsonDirectoryPath, out valueExist);
            if (!valueExist)
            {
                _logger.LogError(EnumUtil.GetName(ConfigKeys.STF_JsonDirectoryPath) + " not exist in config!");
                throw new Exception(EnumUtil.GetName(ConfigKeys.STF_JsonDirectoryPath) + " not exist in config!");
            }
            _logger.LogInfo($"StartService(): Reading {EnumUtil.GetName(ConfigKeys.STF_JsonDirectoryPath)}={_STF_JsonDirectoryPath}");


            _STF_JsonDirectoryUserName = _configReader.GetStringValue(ConfigKeys.STF_JsonDirectoryUserName, out valueExist);
            if (!valueExist)
            {
                _logger.LogWarning(EnumUtil.GetName(ConfigKeys.STF_JsonDirectoryUserName) + " not exist in config!"); 
            }
            _logger.LogInfo($"StartService(): Reading {EnumUtil.GetName(ConfigKeys.STF_JsonDirectoryUserName)}={_STF_JsonDirectoryUserName}");


            _STF_JsonDirectoryDomain = _configReader.GetStringValue(ConfigKeys.STF_JsonDirectoryDomain, out valueExist);
            if (!valueExist)
            {
                _logger.LogInfo(EnumUtil.GetName(ConfigKeys.STF_JsonDirectoryDomain) + " not exist in config!"); 
            }
            _logger.LogInfo($"StartService(): Reading {EnumUtil.GetName(ConfigKeys.STF_JsonDirectoryDomain)}={_STF_JsonDirectoryDomain}");

            _STF_JsonDirectoryPassword = _configReader.GetStringValue(ConfigKeys.STF_JsonDirectoryPassword, out valueExist);
            if (!valueExist)
            {
                _logger.LogWarning(EnumUtil.GetName(ConfigKeys.STF_JsonDirectoryPassword) + " not exist in config!");
            }
            _logger.LogInfo($"StartService(): Reading {EnumUtil.GetName(ConfigKeys.STF_JsonDirectoryPassword)}={_STF_JsonDirectoryPassword}");
            _logger.LogInfo("StartService(): Reading encoded " +
                  EnumUtil.GetName(ConfigKeys.STF_JsonDirectoryPassword) + "=" + _STF_JsonDirectoryPassword);
            _STF_JsonDirectoryPassword = Crypto.DecryptStringAES(_STF_JsonDirectoryPassword, Crypto.SharedSecret);


            _serviceId = _configReader.GetStringValue(ConfigKeys.ServiceId, out valueExist);
            if (!valueExist)
            {
                _logger.LogError(EnumUtil.GetName(ConfigKeys.ServiceId) + " not exist in config!");
                throw new Exception(EnumUtil.GetName(ConfigKeys.ServiceId) + " not exist in config!");
            }
            _logger.LogInfo($"StartService(): Reading {EnumUtil.GetName(ConfigKeys.ServiceId)}={_serviceId}");


            _empowerJSONpwEncrypted = _configReader.GetBoolValue(ConfigKeys.EmpowerJSONpwEncrypted, out valueExist);
            if (!valueExist)
            {
                _logger.LogError(EnumUtil.GetName(ConfigKeys.EmpowerJSONpwEncrypted) + " not exist in config!");
                throw new Exception(EnumUtil.GetName(ConfigKeys.EmpowerJSONpwEncrypted) + " not exist in config!");
            }
            _logger.LogInfo($"StartService(): Reading {EnumUtil.GetName(ConfigKeys.EmpowerJSONpwEncrypted)}={_empowerJSONpwEncrypted}");

            // Everything passed good.
            return true;
        }

        private void CheckAccessToFolder()
        {
            if (_STF_JsonDirectoryPath.StartsWith(@"\\"))
            {
                if (string.IsNullOrEmpty(_STF_JsonDirectoryUserName) || string.IsNullOrEmpty(_STF_JsonDirectoryPassword))
                {
                    _logger.LogError($"Network shared user/pws cannot be empty!");
                    throw new Exception($"Network shared user/pws cannot be empty!");
                }
                // Logon to network folder.
                // Notice: For FTP or over internet read write, a suitable risk assessment is needed for security
                // and data integrity - and it will need different libraries for example FTP access.
                _impersonationContext = new NetworkFolderLogonWrappedImpersonationContext(
                    _STF_JsonDirectoryDomain, _STF_JsonDirectoryUserName, _STF_JsonDirectoryPassword);

                // Invoke _impersonationContext.Leave() on service stop.
                _impersonationContext.Enter();
            }

            // Check access to folder.
            if (!_folderAccessChecker.CheckFolderAccessRights(_STF_JsonDirectoryPath))
            {
                _logger.LogError($"No access to folder {_STF_JsonDirectoryPath}!");
                throw new Exception($"No access to folder {_STF_JsonDirectoryPath}!");
            }
        }
        #endregion
    }
}
