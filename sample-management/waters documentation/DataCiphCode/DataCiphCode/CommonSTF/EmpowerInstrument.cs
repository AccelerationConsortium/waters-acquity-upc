#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    EmpowerInstrument.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using CommonSTF.Tools.Loggers;
using MillenniumToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonSTF
{
    /// <summary>
    /// Wrapper class for toolkit Instrument object. It support logging and error handling.
    /// </summary>
    public class EmpowerInstrument
    {

        #region Fields
        #region 
        public const int sleepingPeriodForInstrumentConnection = 200;//in ms
        #endregion
        #region _instrument
        /// <summary>
        /// Must be initialized after login with project (after project is selected), otherwise exception is trown
        /// Creating an instance of the COM component with CLSID {DCF06512-1FBC-11D1-873F-0020AFEE2C2A} 
        /// from the IClassFactory failed due to the following error: 80040205 An unexpected exception was raised (Exception from HRESULT: 0x80040205).
        /// </summary>
        Instrument _instrument = new Instrument();
        #endregion
        private ILogger _logger;
        #endregion

        #region Constructor
        public EmpowerInstrument(ILogger logger)
        {
            this._logger = logger;
        }
        #endregion

        #region Properties 
        #region EmpowerInstrumentStatusData
        /// <summary>
        /// Instrument status data collection. You must be connected to instrument before reading this properties.
        /// </summary>
        public EmpowerInstrumentStatusData InstrumentStatusData
        {
            get
            {
                try
                {
                    EmpowerInstrumentStatusData isd = new EmpowerInstrumentStatusData();
                     
                    InstrumentStatus instrumentStatus = (InstrumentStatus)_instrument.Status; 

                    isd.SampleSetLineNumber = instrumentStatus.SampleSetLineNumber;
                    isd.SystemStateDescription = instrumentStatus.SystemStateDescription;
                    return isd;

                }
                catch (Exception exc)
                {
                    string errorMessage = exc.Message;
                    if (exc is System.Runtime.InteropServices.COMException)
                    {
                        errorMessage = ToolkitUtils.GetErrorMessagefromToolkitException((System.Runtime.InteropServices.COMException)exc);
                    }
                    _logger.LogError("InstrumentStatusData : " + errorMessage);
                    return null;
                }
            }
        }
        #endregion
         
        #region ConnectionStatusDone
        /// <summary>
        /// Try to get done status of instrument connection.
        /// </summary>
        public bool ConnectionStatusDone
        {
            get
            {
                try
                {
                    return _instrument.ConnectionStatus.Done;
                }
                catch (Exception exc)
                {
                    string errorMessage = exc.Message;
                    if (exc is System.Runtime.InteropServices.COMException)
                    {
                        errorMessage = ToolkitUtils.GetErrorMessagefromToolkitException((System.Runtime.InteropServices.COMException)exc);
                    }
                    _logger.LogError("Connection status done : " + errorMessage);
                    return false;
                }
            }
        }
        #endregion

        #region ConnectionStatusSucceeded
        /// <summary>
        /// If "Successfully connected to instrument server" OR an empty string
        /// Then call connection succeeded, otherwise error number is returned
        /// </summary>
        public bool ConnectionStatusSucceeded
        {
            get
            {
                try
                {
                    if (_instrument.ConnectionStatus.Text.Equals("Successfully connected to instrument server") || _instrument.ConnectionStatus.Text.Length == 0)
                    {
                        return _instrument.ConnectionStatus.Done;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (System.Runtime.InteropServices.COMException exc)
                {
                    string errorMessage = ToolkitUtils.GetErrorMessagefromToolkitException(exc);
                    _logger.LogError("ConnectionStatusSuccedded : " + errorMessage);
                    return false;
                }
            }
        }
        #endregion
 
        #region SampleSetQueueEntryData
        /// <summary>
        /// Get all Sample Sets from current instrument queue (also current running if there is any).
        /// </summary>
        public List<SampleSetQueueEntryData> SampleSetQueueEntries
        {
            get
            {
                List<SampleSetQueueEntryData> entries = new List<SampleSetQueueEntryData>();
                try
                {
                    SampleSetQueueEntries ssqe = (SampleSetQueueEntries)_instrument.SampleSetQueue; 
                    if (ssqe.Count > 0)
                    {

                        for (int i = 0; i < ssqe.Count; ++i)
                        {
                            SampleSetQueueEntry entry = (SampleSetQueueEntry)ssqe.Item(i);
                            entries.Add(new SampleSetQueueEntryData(entry));
                        }
                    }
                    return entries;
                }
                catch (Exception exc)
                {
                    string errorMessage = exc.Message;
                    if (exc is System.Runtime.InteropServices.COMException)
                    {
                        errorMessage = ToolkitUtils.GetErrorMessagefromToolkitException((System.Runtime.InteropServices.COMException)exc);
                    }
                    _logger.LogError("SampleSetQueueEntries : " + errorMessage);
                    return entries;
                }
            }
        }

        internal ToolkitHelpers.ErrorCodes ErrorCodes
        {
            get => default(ToolkitHelpers.ErrorCodes);
            set
            {
            }
        }

        public CredentialsEntry CredentialsEntry
        {
            get => default(CredentialsEntry);
            set
            {
            }
        }

        public LoginData LoginData
        {
            get => default(LoginData);
            set
            {
            }
        }

        public SampleSetMethodLineData SampleSetMethodLineData
        {
            get => default(SampleSetMethodLineData);
            set
            {
            }
        }
        #endregion

        #endregion

        #region Functions
        #region Connect
        /// <summary>
        /// You connect to System and a Node combination, You need to know a valid combination, The toolkit does not know!
        /// There is no link between systems and nodes, Connection is asynchronous, You used the ConnectionStatus object.
        /// </summary>
        /// <param name="acqServerNode">Node on which system is installed</param>
        /// <param name="instrument">System to connect</param>
        public void Connect(string acqServerNode, string instrument)
        {
            Disconnect();//try to disconnect first
            try
            {
                _instrument.Connect(acqServerNode, instrument);
            }
            catch (Exception exc)
            {
                string errorMessage = exc.Message;
                if (exc is System.Runtime.InteropServices.COMException)
                {
                    errorMessage = ToolkitUtils.GetErrorMessagefromToolkitException((System.Runtime.InteropServices.COMException)exc);
                }
                _logger.LogError("Connect : " + errorMessage); 
            }
        }
        #endregion         

        #region ConnectToInstrumentAndWaitForConnection
        /// <summary>
        /// Connect to instrument and wait to get connection status.
        /// </summary>
        /// <param name="acqServerNode">Node on which system is installed.</param>
        /// <param name="instrumenName">System to connect.</param>
        /// <param name="timeoutSeconds">How much time will we wait for connection to succeed.</param>
        /// <returns>true if successfully connected, false otherwise</returns>
        public bool ConnectToInstrumentAndWaitForConnection(string acqServerNode, string instrumenName,
             int timeoutSeconds,  out string errorMessage)
        {
            errorMessage = "";
            
            _logger.LogInfo($"CommonSTF.ConnectToInstrumentAndWaitForConnection(acqServerNode={acqServerNode}, instrumenName={instrumenName}, timeoutSeconds={timeoutSeconds})... step in...");
            try
            {
                Connect(acqServerNode, instrumenName);  

                int loops = 0;
                if (timeoutSeconds <= 0)
                {
                    timeoutSeconds = 4;
                }
                double sleep = ((double)sleepingPeriodForInstrumentConnection / 1000);
                double maxLoops = timeoutSeconds / sleep;

                _logger.LogInfo($"CommonSTF.ConnectToInstrumentAndWaitForConnection()... loops before timeout: {maxLoops}");
                bool timeout = false;
                while (ConnectionStatusDone == false && !timeout)
                {
                    // Sleep 
                    System.Threading.Thread.Sleep(sleepingPeriodForInstrumentConnection);                    
                    if (++loops >= maxLoops)
                    {
                        timeout = true;
                    }
                }

                if (timeout)
                {
                    errorMessage = $"{instrumenName}@{acqServerNode}: Instrument connection timeout.";
                    _logger.LogError(errorMessage);
                    return false;
                }

                if (ConnectionStatusSucceeded)
                {
                    _logger.LogInfo($"CommonSTF.ConnectToInstrumentAndWaitForConnection()... Connection Succeeded");                    
                    errorMessage = $"{instrumenName}@{acqServerNode}: connection Succeeded.";
                    return true;
                }
                else
                {
                    errorMessage = $"{instrumenName}@{acqServerNode}: connection not succeeded.";
                    _logger.LogError(errorMessage);
                    return false;
                }
            }
            catch (Exception exc)
            {
                errorMessage = exc.Message;
                if (exc is System.Runtime.InteropServices.COMException)
                {
                    errorMessage = ToolkitUtils.GetErrorMessagefromToolkitException((System.Runtime.InteropServices.COMException)exc);
                }
                errorMessage = $"Instrument connection exception. Error: {errorMessage}";
                _logger.LogError(errorMessage, exc);
                return false;
            }
        }
        #endregion
         

        #region Disconnect
        /// <summary>
        /// Disconnect from instrument if connection is active.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _instrument.Disconnect();
            }
            catch (Exception exc)
            {
                string errorMessage = exc.Message;
                if (exc is System.Runtime.InteropServices.COMException)
                {
                    errorMessage = ToolkitUtils.GetErrorMessagefromToolkitException((System.Runtime.InteropServices.COMException)exc);
                }
                _logger.LogError("Disconnect() : " + errorMessage);
            }
        }
        #endregion

        #region Run
        /// <summary>
        /// Run sample set method on connected instrument.
        /// </summary>
        /// <param name="sampleSetMethodName">Sample Set Method to run</param>
        /// <param name="sampleSetName">Sample set name to run (mostly same as sampleSetMethodName)</param>
        /// <param name="errorMessage">Out parameter with error message if there is any error</param>
        /// <param name="runMode">Can be Run Only,  Run And Process and Run And Report.</param>
        /// <returns>True if job is added to queue successfully, otherwise false.</returns>
        public bool Run(string sampleSetMethodName, string sampleSetName, 
            out string errorMessage, int runMode = (int)mtkRunMode.mtkRunOnly )
        {
            errorMessage = "";
            try
            {  
                _instrument.RunMode = (mtkRunMode)Enum.Parse(typeof(mtkRunMode), runMode.ToString());

                _instrument.Run(sampleSetMethodName, sampleSetName);
                return true;
            }
            catch (Exception exc)
            {
                errorMessage = exc.Message;
                if (exc is System.Runtime.InteropServices.COMException)
                {
                    errorMessage = ToolkitUtils.GetErrorMessagefromToolkitException((System.Runtime.InteropServices.COMException)exc);
                }
                _logger.LogError("Run : " + errorMessage, exc);
                return false;
            }
        }
        #endregion

        #region Pause
        /// <summary>
        /// Pause current processing job on connected instrument.
        /// </summary>
        /// <param name="seconds">Seconds to pause</param>
        /// <param name="errorMessage">Error message as out parameter if there is any error.</param>
        /// <returns>If pausing is successful return true, false otherwise.</returns>
        public bool Pause(float seconds, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                _instrument.Pause(seconds); 
                return true;
            }
            catch (Exception exc)
            {
                errorMessage = exc.Message;
                if (exc is System.Runtime.InteropServices.COMException)
                {
                    errorMessage = ToolkitUtils.GetErrorMessagefromToolkitException((System.Runtime.InteropServices.COMException)exc);
                }
                _logger.LogError(errorMessage, exc);
                return false;
            }
        }
        #endregion

        #region Replace
        /// <summary>
        /// The results are stored in the existing sample set created with the original sample set method. 
        /// The replaced sample set method automatically becomes the currently running method; no Resume call is necessary. 
        /// This is used when the current sample set method needs to be modified for lines that have not yet been run. 
        /// Replacing the sample set method with a totally different method can have unpredictable results. 
        /// The new sample set method runs using the original sample set method’s database, project, sample set, and result set and starts at the next line in the method.
        /// This function is also used to change the running method’s RunMode and SuitabilityMode.Replace generates an error 
        /// if the current sample set method is being run with SelectedLines.
        /// </summary>
        /// <param name="sampleSetMethodName">Name of new SSM</param>
        /// <param name="errorMessage">Error message if error occurred.</param>
        /// <returns>True if action completed successful, false if error occurred.</returns>
        public bool Replace(string sampleSetMethodName, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                _instrument.Replace(sampleSetMethodName);
                return true;
            }
            catch (Exception exc)
            {
                errorMessage = exc.Message;
                if (exc is System.Runtime.InteropServices.COMException)
                {
                    errorMessage = ToolkitUtils.GetErrorMessagefromToolkitException((System.Runtime.InteropServices.COMException)exc);
                }
                _logger.LogError(errorMessage, exc);
                return false;
            }
        }
        #endregion

        #region GetCurrentlyRunningJob
        ///// <summary>
        ///// Get min and max vials intervals from every single queued (or running) job (test) from connected instrument
        ///// The first job is actually the running job (if instrument is online and not paused or ....)
        ///// </summary>
        ///// <param name="currentlyRunningVial">Vial label of current processing vial position.</param>
        ///// <param name="showDirtyVialsBeforeCurrent">If true also finished vials from current job are in first interval</param>
        ///// <param name="dirtyVialsAdded">If finished vials are added in first interval</param>
        ///// <param name="currentSSMRunningAdded">If current running Vial is added to return</param>
        ///// <param name="currentLogin">Credential of current logged in Project</param>
        ///// <param name="currentProject">Full path of current Project</param>
        ///// <param name="credentials">Credential for logging if SSM in queue belong to different project than above one</param>
        ///// <param name="ignoreDifferentProjects">Ignore fetching queue and SSMs from other project than above current project</param>
        ///// <returns>True if action completed successful, false if error occurred.</returns>
        public bool GetCurrentlyRunningJob(
            EmpowerSampleSetMethod sampleSetMethod, List<SampleSetQueueEntryData> entries,
            out string currentlyRunningVial, out bool currentSSMRunningAdded,
            LoginData currentLogin, EmpowerProject currentProject, out string firstVial, out string lastVial,
            bool ignoreDifferentProjects, CredentialsEntry credentials,
            bool credentialsPasswordEncrypted, out List<SampleSetMethodLineData> lines,
            out int currentSSMLineNumber)
        { 
            lines = null;
            currentSSMLineNumber = int.MinValue;
            if (ignoreDifferentProjects == false && credentials == null)
            {
                throw new NullReferenceException("If ignoreDifferentProject is false, credentials can not be null!");
            }

            currentSSMRunningAdded = false;
            currentlyRunningVial = null;
            firstVial = "";
            lastVial = "";
            try
            {
                if (entries.Count > 0)
                {
                    entries = entries.OrderBy(u => u.JobId).ToList<SampleSetQueueEntryData>();
                    string error;
                    string currentProjectPath = currentLogin.Project;
                    string originalProjectPath = currentLogin.Project;
                    string originalUsername = currentLogin.Username;
                    string originalPassword = currentLogin.Password;
                    string originalUserType = currentLogin.UserType;

                    var entry = entries[0];
                    if (currentProjectPath != entry.Project)
                    {
                        _logger.LogInfo($"Need to switch project... relogin from \"{currentProjectPath}\" to \"{entry.Project}\"", true);

                        if (ignoreDifferentProjects)
                        {
                            return false;
                        }
                        _logger.LogInfo($"Need to switch project... relogin from \"{currentProjectPath}\" to \"{entry.Project}\"", true);
                        currentProjectPath = entry.Project;
                        currentLogin.Project = currentProjectPath;
                        currentLogin.Username = credentials.Username;
                        currentLogin.Password = credentialsPasswordEncrypted ? credentials.GetDecodedPassword() : credentials.EncodedPassword;
                        currentLogin.UserType = string.Empty;//default user type
                        if (!currentProject.Login(currentLogin, out string errorMessage))
                        {
                            _logger.LogError($"Switching project error: {errorMessage} ", true);
                            return false;
                        }
                        else
                        {
                            _logger.LogInfo($"Successfully switched project", true);
                        }
                    }
                    GenericMethodData gmd = sampleSetMethod.GetSampleSetMethodDetails(entry.SampleSetName);
                     lines = sampleSetMethod.GetSampleSetLines(gmd.ID, out error, out string[] nonExistingCustomColumns);
                    _logger.LogInfo($"Getting SSM id from  SSM name: {entry.SampleSetName}={gmd.ID}; in project {entry.Project} ", true);

                    if (!string.IsNullOrEmpty(error))
                    {
                        _logger.LogError(error, true);
                        return false; 
                    }
                    if (lines.Count > 0)
                    {
                        // Vial can be empty for cleaning/calibration/washing or something like that, skip with empty volume.
                        List<SampleSetMethodLineData> linesWithInjections = lines.Where(u => (u.InjVol.HasValue && u.InjVol >= 0)).ToList();
                        if (linesWithInjections.Count > 0)
                        {                          

                            int? currentSSMLineNumberRunningInInstrument = InstrumentStatusData.SampleSetLineNumber;
                            if (currentSSMLineNumberRunningInInstrument.HasValue)//something is running
                            {
                                currentSSMLineNumber = (int)currentSSMLineNumberRunningInInstrument;
                                if (lines[(int)currentSSMLineNumberRunningInInstrument].InjVol >= 0 && !string.IsNullOrEmpty(
                                    lines[(int)currentSSMLineNumberRunningInInstrument].Vial))
                                {
                                    currentlyRunningVial = lines[(int)currentSSMLineNumberRunningInInstrument].Vial;
                                    currentSSMRunningAdded = true;
                                }
                                firstVial = linesWithInjections[0].Vial;
                                lastVial = linesWithInjections[linesWithInjections.Count - 1].Vial;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    if (currentProjectPath != originalProjectPath)
                    {
                        _logger.LogInfo($"Need to switch project to original one from \"{currentProjectPath}\" to \"{originalProjectPath}\"", true);
                        currentProjectPath = originalProjectPath;
                        currentLogin.Project = currentProjectPath;
                        currentLogin.Username = originalUsername;
                        currentLogin.Password = originalPassword;
                        currentLogin.UserType = originalUserType;
                        if (!currentProject.Login(currentLogin, out string errorMessage))
                        {
                            _logger.LogError($"Switching project to original one error: {errorMessage} ", true);
                        }
                        else
                        {
                            _logger.LogInfo($"Successfully switched to original project", true);
                        }
                    }
                    return true;
                }
                return false;
            }
            catch (Exception exc)
            {
                string errorMessage = exc.Message;
                if (exc is System.Runtime.InteropServices.COMException)
                {
                    errorMessage = ToolkitUtils.GetErrorMessagefromToolkitException((System.Runtime.InteropServices.COMException)exc);
                }
                _logger.LogError(errorMessage, true);
                return false;
            }
        }
        #endregion

        #region RemoveSSMFromQueue
        /// <summary>
        /// Remove job from instrument Queue.
        /// </summary>
        /// <param name="JobId">Id of job to be removed.</param>
        /// <param name="errorMessage">Error message if action fails.</param>
        /// <returns>True if action completed successful, false if error occurred.</returns>
        public bool RemoveSSMFromQueue(int JobId, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                _instrument.RemoveEntry(JobId);
                return true;
            }
            catch (Exception exc)
            {
                errorMessage = exc.Message;
                if (exc is System.Runtime.InteropServices.COMException)
                {
                    errorMessage = ToolkitUtils.GetErrorMessagefromToolkitException((System.Runtime.InteropServices.COMException)exc);
                }
                _logger.LogError($"Error while removing SSM from queue: {errorMessage}.", exc);
                return false;
            }
        }
        #endregion
         
        #endregion
    }
}
