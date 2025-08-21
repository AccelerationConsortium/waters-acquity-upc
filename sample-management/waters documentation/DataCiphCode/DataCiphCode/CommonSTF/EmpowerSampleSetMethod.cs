#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    EmpowerSampleSetMethod.cs                                                          
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonSTF
{
    /// <summary>
    /// Wrapper for toolkit Method object. Handling fetching and storing different type of Methods
    /// and Sample Set Method Lines.
    /// </summary>
    public class EmpowerSampleSetMethod  
    {
        #region Fields
        private ILogger _logger;
        #endregion

        public EmpowerSampleSetMethod(ILogger logger)
        {
            _logger = logger;
        }

        public GenericMethodData GenericMethodData
        {
            get => default(GenericMethodData);
            set
            {
            }
        }

        internal ToolkitHelpers.ErrorCodes ErrorCodes
        {
            get => default(ToolkitHelpers.ErrorCodes);
            set
            {
            }
        }

        public ToolkitUtils ToolkitUtils
        {
            get => default(ToolkitUtils);
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

        #region Functions
        #region GetSampletSetMethodFromName
        /// <summary>
        /// Fetch sample set method by method name. It will return last version of SSM with equal name.
        /// </summary>
        /// <param name="methodName">Sample Set Method name to fetch.</param>
        /// <returns>Toolkit Method object</returns>
        internal Method GetSampletSetMethodFromName(string methodName)
        {
            try
            {
                foreach (string name in new Method().Names((mtkMethodType.mtkSampleSetMethod)) as string[])
                {
                    if (name == methodName)
                    {
                        Method method = new Method();
                        method.Name = name;
                        method.Fetch(mtkMethodType.mtkSampleSetMethod);
                        return method;
                    }
                }
            }
            catch (Exception exc)
            {
                string errorMessage = exc.Message;
                if (exc is System.Runtime.InteropServices.COMException)
                {
                    errorMessage = ToolkitUtils.GetErrorMessagefromToolkitException((System.Runtime.InteropServices.COMException)exc);
                }
                _logger.LogError("EmpowerSampleSetMethod GetSampleSetMethodFromName: " + errorMessage);
                return null;
            }
            return null;
        }
        #endregion
        
        #region GetSampleSetMethodDetails
        /// <summary>
        /// Creates wrapper object for Method toolkit object with generic data.
        /// </summary>
        /// <param name="methodName">Method name to fetch.</param>
        /// <returns>Object containing generic method data (ID, Name, Date, Modified by, Comments).</returns>
        public GenericMethodData GetSampleSetMethodDetails(string methodName)
        {
            Method method = GetSampletSetMethodFromName(methodName);

            if (method == null)
            {
                _logger.LogError($"SampleSetMethod '{methodName}' not exist!");
                return null;
            }
            else
                return new GenericMethodData(method.ID, method.Name, method.Date, method.ModifiedBy, method.Comments);
        }
        #endregion

        #region SaveSampleSetMethodLines
        /// <summary>
        /// Saves new or edited lines (or deleted lines) and save comment and method name.
        /// </summary>
        /// <param name="method">Method to save (with Method ID included)</param>
        /// <param name="lines">Sample Set Lines to save to desired method</param>
        /// <param name="errorMessage">Returning possible error message.</param>
        /// <returns>True if action completed successful, false if error occurred.</returns>
        public bool SaveSampleSetMethodAndLines(GenericMethodData method, List<SampleSetMethodLineData> lines, out string errorMessage)
        {
            try
            {
                SampleSetMethod sampleSetMethod = new SampleSetMethod();
                sampleSetMethod.FetchById(method.ID);
                 

                sampleSetMethod.Comment = method.Comments;
                sampleSetMethod.Name = method.Name;                 

                // Remove all old.
                while (sampleSetMethod.SampleSetLines.Count > 0)
                {
                    sampleSetMethod.SampleSetLines.Remove(0);
                }

                for (int i = 0; i < lines.Count; i++)
                {
                    SampleSetLine ssLine = lines[i].originalToolkitLine == null ? new SampleSetLine() : lines[i].originalToolkitLine;

                    if (!string.IsNullOrEmpty(lines[i].Vial))
                        ssLine.Set(SampleSetMethodLineField.VIAL, lines[i].Vial);

                    if (lines[i].InjVol.HasValue)
                        ssLine.Set(SampleSetMethodLineField.INJ_VOL, lines[i].InjVol);

                    if (lines[i].NumOfInjs.HasValue)
                        ssLine.Set(SampleSetMethodLineField.NUM_OF_INJS, lines[i].NumOfInjs);

                    if (!string.IsNullOrEmpty(lines[i].Label))
                        ssLine.Set(SampleSetMethodLineField.LABEL, lines[i].Label);

                    if (!string.IsNullOrEmpty(lines[i].SampleName))
                        ssLine.Set(SampleSetMethodLineField.SAMPLE_NAME, lines[i].SampleName);

                    if (!string.IsNullOrEmpty(lines[i].Level))
                        ssLine.Set(SampleSetMethodLineField.LEVEL, lines[i].Level);

                    if (lines[i].Function.HasValue)
                        ssLine.Set(SampleSetMethodLineField.FUNCTION, lines[i].Function);

                    if (!string.IsNullOrEmpty(lines[i].MethodSetOrReportMethod))
                        ssLine.Set(SampleSetMethodLineField.METHOD_SET_OR_REPORT_METHOD, lines[i].MethodSetOrReportMethod);

                    if (lines[i].InjVol.HasValue)
                        ssLine.Set(SampleSetMethodLineField.LABEL_REFERENCE, lines[i].LabelReference);

                    if (lines[i].Processing.HasValue)
                        ssLine.Set(SampleSetMethodLineField.PROCESSING, lines[i].Processing);

                    if (lines[i].RunTime.HasValue)
                        ssLine.Set(SampleSetMethodLineField.RUN_TIME, lines[i].RunTime);

                    if (lines[i].DataStart.HasValue)
                        ssLine.Set(SampleSetMethodLineField.DATA_START, lines[i].DataStart);

                    if (lines[i].NextInjDelay.HasValue)
                        ssLine.Set(SampleSetMethodLineField.NEXT_INJ_DELAY, lines[i].NextInjDelay);


                    if (lines[i].nonStandardAdditionalColumns != null && lines[i].nonStandardAdditionalColumns.Count > 0)
                    {
                        List<KeyValuePair<string, string>> additionalColumns = lines[i].nonStandardAdditionalColumns;
                        foreach (KeyValuePair<string, string> addColumn in additionalColumns)
                        {
                            if (lines[i].originalToolkitLine != null)
                            {
                                var fieldValue = lines[i].originalToolkitLine.Get(addColumn.Key);
                                if (fieldValue is int && addColumn.Value != null)
                                {
                                    if (addColumn.Key == SampleSetMethodLineField.FUNCTION && addColumn.Value is string)
                                    {
                                        ssLine.Set(addColumn.Key, ToolkitUtils.GetFunctionValue(addColumn.Value));
                                    }
                                    else
                                    {
                                        ssLine.Set(addColumn.Key, Convert.ToInt32(addColumn.Value));
                                    }
                                }
                                else if (fieldValue is double && addColumn.Value != null)
                                {
                                    ssLine.Set(addColumn.Key, Convert.ToDouble(addColumn.Value));
                                }
                                else if (fieldValue is decimal && addColumn.Value != null)
                                {
                                    ssLine.Set(addColumn.Key, Convert.ToDecimal(addColumn.Value));
                                }
                                else if (fieldValue is DateTime && addColumn.Value != null)
                                {
                                    ssLine.Set(addColumn.Key, Convert.ToDateTime(addColumn.Value));
                                }
                                else if (fieldValue is bool && addColumn.Value != null)
                                {
                                    ssLine.Set(addColumn.Key, Convert.ToBoolean(addColumn.Value));
                                }
                                else if (fieldValue is string && addColumn.Value != null)
                                {
                                    ssLine.Set(addColumn.Key, Convert.ToString(addColumn.Value));
                                }
                                else
                                {
                                    ssLine.Set(addColumn.Key, addColumn.Value == null ? null : addColumn.Value);
                                }
                            }
                            else
                            {
                                ssLine.Set(addColumn.Key, addColumn.Value == null ? "" : addColumn.Value);
                            }
                        }
                    }
                    sampleSetMethod.SampleSetLines.Add(ssLine);
                }              
                
                sampleSetMethod.Store(); // After working with a method please they can left in the 'in use by' state, call the unlock method to remove the in use by lock.

                errorMessage = "";
                sampleSetMethod.UnLock(); // Clears the 'Being Edited By' lock for a ValidationProtocolMethod.
                return true;

            }
            catch (Exception exc)
            {
                errorMessage = exc.Message;
                if (exc is System.Runtime.InteropServices.COMException)
                {
                    errorMessage = ToolkitUtils.GetErrorMessagefromToolkitException((System.Runtime.InteropServices.COMException)exc);
                }
                _logger.LogError("EmpowerSampleSetMethod SaveSampleSetMethodLines: " + errorMessage);
                return false;
            }
        }
        #endregion
       
        #region GetSampleSetLines
        /// <summary>
        /// Get Sample Set Line for specific Sample Set Method
        /// </summary>
        /// <param name="methodId">Sample Set Method ID</param>
        /// <param name="nonStandardAdditionalColumns">Optional: if additional columns are inserted in Empower</param>
        /// <returns></returns>
        public List<SampleSetMethodLineData> GetSampleSetLines(int methodId, out string errorMessage,
            out string[] nonStandardAdditionalColumnsNonExistent, List<string> nonStandardAdditionalColumns = null, bool includeOriginalEmpowerToolkitObject = false)
        {
            nonStandardAdditionalColumnsNonExistent = null;
            errorMessage = "";
            List<SampleSetMethodLineData> lines = new List<SampleSetMethodLineData>();
            try
            {

                SampleSetMethod sampleSetMethod = new SampleSetMethod();
                sampleSetMethod.FetchById(methodId);

                List<string> nonExistentELNColumns = new List<string>();

                foreach (SampleSetLine sampleSetLine in sampleSetMethod.SampleSetLines)
                { 
                    List<KeyValuePair<string, string>> additionalColumns = new List<KeyValuePair<string, string>>();

                    if (nonStandardAdditionalColumns != null && nonStandardAdditionalColumns.Count > 0)
                    {
                        foreach (string column in nonStandardAdditionalColumns)
                        {
                            try
                            {
                                string value = string.IsNullOrEmpty(Convert.ToString(sampleSetLine.Get(column))) ?
                                    null : Convert.ToString(sampleSetLine.Get(column));

                                if (!string.IsNullOrEmpty(value))
                                {
                                    KeyValuePair<string, string> additionalColumn =
                                         new KeyValuePair<string, string>(column, value);

                                    additionalColumns.Add(additionalColumn);
                                }
                            }
                            catch (System.Runtime.InteropServices.COMException exc)
                            {
                                //Custom Fields from config files are not included in some projects, 
                                //but are included in others, so ignore them
                                //Only way to do this is to catch toolkit exc with code 203 - Non existent fields
                                if (ToolkitUtils.ToolkitErrorCodeExtract(exc.ErrorCode) == "203")
                                {
                                    string error = "Invalid value. Fields, when an added field name is invalid.Occurs when setting a variant value that cannot be converted or is in the wrong format.";
                                    _logger.LogError($"EmpowerSampleSetMethod GetSampleSetLine custom column missing, columnName:{column}, error:{error}");

                                    if (!nonExistentELNColumns.Contains(column))//save column name so such columns can be removed from DataGridView
                                        nonExistentELNColumns.Add(column);
                                }
                                else
                                {
                                    throw exc;
                                }
                            }
                        }
                    }

                    SampleSetMethodLineData ssld =
                    new SampleSetMethodLineData(string.IsNullOrEmpty(Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.VIAL))) ?
                            null : Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.VIAL)),
                        string.IsNullOrEmpty(Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.INJ_VOL))) ?
                            null : Convert.ToDouble(sampleSetLine.Get(SampleSetMethodLineField.INJ_VOL)),
                        string.IsNullOrEmpty(Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.NUM_OF_INJS))) ?
                            null : Convert.ToInt32(sampleSetLine.Get(SampleSetMethodLineField.NUM_OF_INJS)),
                        string.IsNullOrEmpty(Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.LABEL))) ?
                            null : Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.LABEL)),
                        string.IsNullOrEmpty(Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.SAMPLE_NAME))) ?
                            null : Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.SAMPLE_NAME)),
                        string.IsNullOrEmpty(Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.LEVEL))) ?
                            null : Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.LEVEL)),
                        string.IsNullOrEmpty(Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.FUNCTION))) ?
                            null : Convert.ToInt32(sampleSetLine.Get(SampleSetMethodLineField.FUNCTION)),
                        string.IsNullOrEmpty(Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.METHOD_SET_OR_REPORT_METHOD))) ?
                            null : Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.METHOD_SET_OR_REPORT_METHOD)),
                        string.IsNullOrEmpty(Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.LABEL_REFERENCE))) ?
                            null : Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.LABEL_REFERENCE)),
                        string.IsNullOrEmpty(Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.PROCESSING))) ?
                            null : Convert.ToInt32(sampleSetLine.Get(SampleSetMethodLineField.PROCESSING)),
                        string.IsNullOrEmpty(Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.RUN_TIME))) ?
                            null : Convert.ToDouble(sampleSetLine.Get(SampleSetMethodLineField.RUN_TIME)),
                        string.IsNullOrEmpty(Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.DATA_START))) ?
                            null : Convert.ToDouble(sampleSetLine.Get(SampleSetMethodLineField.DATA_START)),
                        string.IsNullOrEmpty(Convert.ToString(sampleSetLine.Get(SampleSetMethodLineField.NEXT_INJ_DELAY))) ?
                            null : Convert.ToDouble(sampleSetLine.Get(SampleSetMethodLineField.NEXT_INJ_DELAY)),
                        
                        additionalColumns);
                    if (includeOriginalEmpowerToolkitObject)
                    {
                        ssld.originalToolkitLine = sampleSetLine;
                    }
                    lines.Add(ssld);

                }
                if (nonExistentELNColumns.Count > 0)
                {
                    nonStandardAdditionalColumnsNonExistent = nonExistentELNColumns.ToArray();
                }
                return lines;
            }
            catch (Exception exc)
            {
                errorMessage = exc.Message;
                if (exc is System.Runtime.InteropServices.COMException)
                {
                    errorMessage = ToolkitUtils.GetErrorMessagefromToolkitException((System.Runtime.InteropServices.COMException)exc);
                }
                _logger.LogError("EmpowerSampleSetMethod GetSampleSetLine: " + errorMessage);
                return lines;
            }
        }
        #endregion
        #endregion Functions
    }
}
