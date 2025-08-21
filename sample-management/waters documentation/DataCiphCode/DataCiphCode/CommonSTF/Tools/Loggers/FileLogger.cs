#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF.Tools.Loggers
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    FileLogger.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
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
    /// If there is no any option to write to log file or event track, try to write to simple txt file.
    /// This is in most cases before service is installed (while installation process is going on)  and log and config are read out.
    /// </summary>
    public class FileLogger : ILogger
    {
        private string _logPath;

        public FileLogger(string logFile)
        {
            _logPath = logFile;
        }

        

        private void Log(string severity,string msg)
        {
            using (StreamWriter file = new StreamWriter(_logPath, true))
            {
                file.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} {severity}: {msg}");
            }
        }

        public void LogError(string msg,
                bool logCallerInformation = false,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log("Error", LoggerHelper.getMessageWithCaller(msg, logCallerInformation, sourceFilePath, sourceLineNumber, memberName));
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
            msg = $"{msg}, ex={errorMessage}";

            Log("Error", LoggerHelper.getMessageWithCaller(msg, logCallerInformation, sourceFilePath, sourceLineNumber, memberName));
        }

        public void LogInfo(string msg,
                bool logCallerInformation = false,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log("Info", LoggerHelper.getMessageWithCaller(msg, logCallerInformation, sourceFilePath, sourceLineNumber, memberName));
        }

        public void LogWarning(string msg,
                bool logCallerInformation = false,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log("Warning", LoggerHelper.getMessageWithCaller(msg, logCallerInformation, sourceFilePath, sourceLineNumber, memberName));
        }
    }
}
