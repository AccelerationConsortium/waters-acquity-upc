#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    STFTest
//        NS:    STFTest
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    Globals.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:59 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using CommonSTF.Tools.Configuration;
using CommonSTF.Tools.Loggers; 
using System.Collections.Generic; 

namespace STFTest
{
    public static class Globals
    { 
        private static IConfig _configReader;
        private static ILogger _fileLogger;
        private static ILogger _log4NetLogger;
        private static ILogger _eventLogger; 
        private static ILogger _multichannelLogger;
         

        public static IConfig GetConfigReader()
        {
            if (_configReader == null)
            {
                _configReader = new ConfigReader();
            }
            return _configReader;
        }

        public static ILogger GetFileLogger()
        {
            if (_fileLogger == null)
            {
                _fileLogger = new FileLogger(GetConfigReader().GetStringValue(ConfigKeys.LogFileNamePath));
            }
            return _fileLogger;
        }

        public static ILogger GetLog4NetLogger()
        {
            if (_log4NetLogger == null)
            {
                _log4NetLogger = new Log4NetLogger(GetConfigReader().GetStringValue(ConfigKeys.LogFileNamePath));
            }
            return _log4NetLogger;
        }
 

        public static ILogger GetEventLogger()
        {
            if (_eventLogger == null)
            {
                string logName = GetConfigReader().GetStringValue(ConfigKeys.EventLogName);
                string eventSourceName = GetConfigReader().GetStringValue(ConfigKeys.EventLogSourceName);
                eventSourceName = string.IsNullOrEmpty(eventSourceName) ? "test" : eventSourceName;
                if (!string.IsNullOrEmpty(logName))
                {
                    _eventLogger = new EventLogger(logName, eventSourceName);
                }
                else
                {
                    _eventLogger = new EventLogger(eventSourceName);
                }

            }
            return _eventLogger;
        }

        public static ILogger GetMultichannelLogger()
        {
            if (_multichannelLogger == null)
            {
                _multichannelLogger = new MultiLogger(new List<ILogger>() { GetEventLogger(), GetLog4NetLogger()});
            }
            return _multichannelLogger;
        }
    }
}

