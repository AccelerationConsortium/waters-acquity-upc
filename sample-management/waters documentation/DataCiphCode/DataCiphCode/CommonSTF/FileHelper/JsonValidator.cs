#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF.FileHelper
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    JsonValidator.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using CommonSTF;
using CommonSTF.Tools.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonSTF.FileHelper
{
    /// <summary>
    /// The class that take care about proper/expected JSON file format, according to current rules.
    /// </summary>
    public class JsonValidator
    {
        private ILogger _logger;
        public JsonValidator(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validating object from deserialized JSON file. According to specs, number of Sample Sets in JSON has to be equal to header control value.
        /// Number of Sample Sets cannot be 0. In every JSON file, there must be empower user name and password.
        /// Number of samples (lines) in every Sample Set must be equal to control number.
        /// Line number (internally used for sorting), Vial label and Function are mandatory fields in every Sample Set.
        /// </summary>
        /// <param name="jSONobject">Object to validate</param>
        /// <param name="errorMessage">Error (exception) message if return value fail.</param>
        /// <returns></returns>
        public bool ValidateJSONObject(JsonObject jSONobject, out string errorMessage)
        {
            _logger.LogInfo($"Validating file: {jSONobject.FileName}");
            errorMessage = "";
            if (jSONobject.HeaderFields.SampleSets != jSONobject.SampleSetDetails.Count)
            {
                errorMessage = $"The number of SSM details are not equal to number described in the file header. SSMs in header={jSONobject.HeaderFields.SampleSets}, SampleSetDetails.Count={jSONobject.SampleSetDetails.Count}.";
                _logger.LogError(errorMessage, true);
                return false;
            }

            if (jSONobject.SampleSetDetails.Count == 0)
            {
                errorMessage = "The number of SSM details cannot be 0.";
                _logger.LogError(errorMessage, true);
                return false;
            }

            if (string.IsNullOrEmpty(jSONobject.HeaderFields.EmpowerUn) ||
                string.IsNullOrEmpty(jSONobject.HeaderFields.EmpowerPw))
            {
                errorMessage = "Empower UserName or Password are empty in JSON file!";
                _logger.LogError(errorMessage, true);
                return false;
            }

            if (jSONobject.TrailerReport == null)
            { 
                errorMessage = "TrailerReport not exists in JSON file!";
                _logger.LogError(errorMessage, true);
                return false;
            }

            foreach (SampleSetDetailsData sampleSet in jSONobject.SampleSetDetails)
            {
                if (sampleSet.NumberOfSamples != sampleSet.Samples.Count)
                {
                    errorMessage = $"NumberOfSamples={sampleSet.NumberOfSamples} in SampleSetDetailsData is not equal to jSONobject.SampleSetDetails.Samples.Count={sampleSet.Samples.Count}";
                    _logger.LogError(errorMessage, true);
                    return false;
                }
                foreach (Dictionary<string, object> line in sampleSet.Samples)
                {
                    // Check mandatory fields.
                    if (!int.TryParse(line[JsonUtils.LineNumberLabel].ToString(), out int result1))
                    {
                        errorMessage = $"LineNumber  in {jSONobject.FileName} is not number! Value={line[JsonUtils.LineNumberLabel].ToString()}";
                        _logger.LogError(errorMessage, true);
                        return false;
                    }
                    if (string.IsNullOrEmpty(line[SampleSetMethodLineField.FUNCTION].ToString()))
                    {
                        errorMessage = $"Function in {jSONobject.FileName}  cannot be null or empty! Value={line[SampleSetMethodLineField.FUNCTION].ToString()}";
                        _logger.LogError(errorMessage, true);
                        return false;
                    }
                    if (string.IsNullOrEmpty(line[SampleSetMethodLineField.VIAL].ToString()))
                    {
                        errorMessage = $"Vial in {jSONobject.FileName} cannot be null or empty! Value={line[SampleSetMethodLineField.VIAL].ToString()}";
                        _logger.LogError(errorMessage, true);
                        return false;
                    }
                }
            }

            _logger.LogInfo($"File verification passed: {jSONobject.FileName}!");
            return true;
        }
    }
}
