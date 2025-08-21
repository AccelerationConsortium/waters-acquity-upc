#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF.FileHelper
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    JsonUtils.cs                                                          
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
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommonSTF.FileHelper
{
    /// <summary>
    /// Ensures serialization and deserialization of JSON  files. If JSON files are not valid, an error will be written to log.
    /// </summary>
    public class JsonUtils
    {
        public const string LineNumberLabel = "LineNumber";
        #region Privates
        private ILogger _logger;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            // Write formated JSON file with CR (new lines), correct indent...
            WriteIndented = true,
        };
        #endregion Privates


        #region Constructors
        public JsonUtils(ILogger logger)
        {
            _logger = logger;
        }
        #endregion


        #region Public methods
        public JsonObject Deserialize(string fileName, bool empowerJSONpwEncrypted, out string error)
        {
            _logger.LogInfo($"Starting file deserialization {fileName}.");
            error = "";
            try
            {
                string jsonString = File.ReadAllText(fileName).Replace("\\\\", "\\").Replace("\\", "\\\\");
                var JSONobject = JsonSerializer.Deserialize<JsonObject>(jsonString); 
                JSONobject.FileName = fileName;
                if (!string.IsNullOrEmpty(JSONobject.HeaderFields.EmpowerPw))
                {
                    if (empowerJSONpwEncrypted)
                    {
                        JSONobject.HeaderFields.EmpowerPw =
                            CommonSTF.Tools.Crypto.DecryptStringAES(JSONobject.HeaderFields.EmpowerPw,
                            CommonSTF.Tools.Crypto.SharedSecret);
                    }
                }

                _logger.LogInfo($"File deserialization {fileName} succeeded.");
                return JSONobject;
            }
            catch (Exception ex)
            {
                error = "Error occurred while Deserializing file: ";
                _logger.LogError(error, ex);
                error = error + ex.Message;
                return null;
            }
        }

        public string Serialize(JsonObject jSONobject, out string error)
        {
            _logger.LogInfo($"Starting file object Serialization for {jSONobject.FileName}.");

            error = "";
            try
            {
                var serializedJSON =  JsonSerializer.Serialize<JsonObject>(jSONobject, _jsonSerializerOptions);
                _logger.LogInfo($"Object serialization succeeded for {jSONobject.FileName}.");
                // Unescape single quotes.
                return System.Text.RegularExpressions.Regex.Unescape(serializedJSON); 
            }
            catch (Exception ex)
            {
                error = $"Error occurred while Serializing object for {jSONobject.FileName}. ";
                _logger.LogError(error, ex);
                error = error + ex.Message;
                return null;
            }
        }   
        #endregion Public methods 
    }
}
