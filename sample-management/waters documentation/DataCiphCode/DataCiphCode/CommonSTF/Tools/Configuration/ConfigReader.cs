#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF.Tools.Configuration
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    ConfigReader.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using CommonSTF.Tools.Configuration;
using CommonSTF.Tools.Loggers;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonSTF.Tools.Configuration
{
    /// <summary>
    /// An implementation of configuration reader interface. Object is performing an exception less reading.
    /// If key is not in default configuration file, try to get one from common project.
    /// </summary>
    public class ConfigReader : IConfig
    {
        

        public int GetIntValue(ConfigKeys forKey)
        {
            return GetValue<int>(forKey);
        }

        public string GetStringValue(ConfigKeys forKey)
        {
            return GetValue<string>(forKey);
        }

        public bool GetBoolValue(ConfigKeys forKey)
        {
            return GetValue<bool>(forKey);
        }

        public int GetIntValue(ConfigKeys forKey, out bool valueExists)
        {
            return GetValue<int>(forKey, out valueExists);
        }

        public string GetStringValue(ConfigKeys forKey, out bool valueExists)
        {
            return GetValue<string>(forKey, out valueExists);
        }

        public bool GetBoolValue(ConfigKeys forKey, out bool valueExists)
        {
            return GetValue<bool>(forKey, out   valueExists);
        }

        public T GetValue<T>(ConfigKeys forKey)
        {
            return GetValue<T>(forKey, out bool valueExists);
        }

        public T GetValue<T>(ConfigKeys forKey, out bool valueExists)
        {
            valueExists = false;
            T retVal = default(T);

            try
            {
                string keyName = forKey.ToString();

                if (ConfigurationManager.AppSettings.AllKeys.ToList().Contains(keyName))//first try to read from calling app settings
                {
                    string cfgVal = ConfigurationManager.AppSettings.GetValues(keyName).FirstOrDefault();
                    retVal = (T)Convert.ChangeType(cfgVal, typeof(T));
                    valueExists = true;
                }
                else // Try if it is in (this)common.dll config.
                {
                    var commonDllConfig = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    if (commonDllConfig != null)
                    {
                        if (commonDllConfig.AppSettings.Settings.AllKeys.ToList().Contains(keyName))
                        {
                            string cfgVal = commonDllConfig.AppSettings.Settings[keyName].Value;
                            retVal = (T)Convert.ChangeType(cfgVal, typeof(T));
                            valueExists = true;
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                throw new Exception(ee.Message);
            }

            return retVal;
        }
    }
}
