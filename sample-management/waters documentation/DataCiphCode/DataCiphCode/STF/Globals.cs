#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    STFService
//        NS:    STFService
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    Globals.cs                                                          
//    Author:    Vedran Jašarević | Egeo d.o.o.                                        
//   Created:    1.10.2020 8:27 AM
//   Updated:    10.11.2020 6:45 AM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@gmail.com                                            
//==================================================================================

#endregion
using CommonSTF.Tools;
using CommonSTF.Tools.Configuration;
using CommonSTF.Tools.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STFService
{
    public static class Globals
    { 
        private static IConfig _configReader;
        private static ILogger _fileLogger;
        private static ILogger _log4NetLogger;
        private static ILogger _eventLogger; 
        private static ILogger _multichannelLogger ; 

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
                try
                {
                    string path = GetConfigReader().GetStringValue(ConfigKeys.LogFileNamePath);
                    string drive = System.IO.Path.GetPathRoot(path);   // e.g. K:\

                    if (System.IO.Directory.Exists(drive))
                    {
                        _fileLogger = new FileLogger(path);
                    }
                    else
                    {
                        GetEventLogger().LogError("Path for log file does not exist: " + path + ", switching to c: ");
                        path = GetConfigReader().GetStringValue(ConfigKeys.LogFileNamePath);
                        path = "C:" + path.Substring(2, path.Length - 2);

                        GetEventLogger().LogError("New path: " + path);
                        _fileLogger = new FileLogger(path);
                    }
                }
                catch (System.Exception ee)
                {
                    GetEventLogger().LogError("Error getting file logger: " + ee.Message);
                }
            }
            return _fileLogger;
        }

        public static ILogger GetLog4NetLogger()
        {
            if (_log4NetLogger == null)
            {
                string path = GetConfigReader().GetStringValue(ConfigKeys.LogFileNamePath);

                string drive = System.IO.Path.GetPathRoot(path);   // e.g. K:\

                if (System.IO.Directory.Exists(drive))
                {
                    _log4NetLogger = new Log4NetLogger(GetConfigReader().GetStringValue(ConfigKeys.LogFileNamePath));
                }
                else
                {
                    GetEventLogger().LogError("Log4NetLogger path (drive) does not exists: " + path + ", switching to c: ");

                    path = "C:" + path.Substring(2, path.Length - 2);
                    GetEventLogger().LogError("Log4NetLogger new path: " + path);
                    _log4NetLogger = new Log4NetLogger(path);

                }
            }
            return _log4NetLogger;
        }

        public static ILogger GetMultichannelLogger()
        {
            if (_multichannelLogger == null)
            {
                _multichannelLogger = new MultiLogger(new List<ILogger>() { GetEventLogger(), GetLog4NetLogger() });
            }
            return _multichannelLogger;
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
    }
}
