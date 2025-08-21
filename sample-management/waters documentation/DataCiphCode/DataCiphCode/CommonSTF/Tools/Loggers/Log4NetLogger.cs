#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF.Tools.Loggers
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    Log4NetLogger.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonSTF.Tools.Loggers
{
    /// <summary>
    /// Implementation of Log4Net logging class, based on interface.
    /// </summary>
    public  class Log4NetLogger:ILogger
    { 
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
          
        public Log4NetLogger(string logFileNamePath)
        {  

            string logPath = logFileNamePath;
            if (string.IsNullOrEmpty(logPath))
            {
                logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppDomain.CurrentDomain.FriendlyName + ".log");
            }

            if (!File.Exists(logPath))
            {               
                string dir = Path.GetDirectoryName(logPath);
                if (!Directory.Exists(dir))
                {
                    try
                    {
                        Directory.CreateDirectory(dir);
                        using (var f = File.Create(logPath)) { }
                    }
                    catch (Exception e)
                    {
                        // Exception. Path, drive not exist or have not right to write.
                        EventLogger el = new EventLogger(
                            new Configuration.ConfigReader().GetStringValue(Configuration.ConfigKeys.EventLogSourceName));
                        el.LogError("Cannot create log file or path!", e);
                    }
                }                
            }

            GlobalContext.Properties["LogFilePath"] = logPath;
            log4net.Config.XmlConfigurator.Configure();

        }

        public ILogger ILogger
        {
            get => default(ILogger);
            set
            {
            }
        }

        public void LogError(string msg,
                bool logCallerInformation = false,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            string message = LoggerHelper.getMessageWithCaller(msg, logCallerInformation, sourceFilePath, sourceLineNumber, memberName);
            _log.Error(message); 
        }

        public void LogError(string msg, Exception ex,
              bool logCallerInformation = true,
              [CallerMemberName] string memberName = "",
              [CallerFilePath] string sourceFilePath = "",
              [CallerLineNumber] int sourceLineNumber = 0)
        {
            string errorMessage = ex.Message;
            var innerEx = ex.InnerException;
            while (innerEx != null)
            {
                errorMessage += $"; innerEx = {innerEx.Message}";
                innerEx = innerEx.InnerException;
            }
            string message = LoggerHelper.getMessageWithCaller($"{msg}, ex={errorMessage}", 
                logCallerInformation, sourceFilePath, sourceLineNumber, memberName);
            _log.Error(message); 
        }


        public void LogInfo(string msg,
                bool logCallerInformation = false,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            string message = LoggerHelper.getMessageWithCaller(msg, logCallerInformation, sourceFilePath, sourceLineNumber, memberName);
            _log.Info(message); 
        }

        public void LogWarning(string msg,
                bool logCallerInformation = false,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            string message = LoggerHelper.getMessageWithCaller(msg, logCallerInformation, sourceFilePath, sourceLineNumber, memberName);
            _log.Warn(message); 
        }
    }
}
