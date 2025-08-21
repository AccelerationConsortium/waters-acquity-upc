#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF.Tools.Loggers
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    EventLogger.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using log4net;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CommonSTF.Tools.Loggers
{
    /// <summary>
    /// Logger that take care of logging to Windows event database (viewable through Event Viewer).
    /// </summary>
    public class EventLogger : ILogger
    { 
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private EventLog eventLog;

        public EventLogger(string eventLogSourceName) : this("Application", eventLogSourceName)
        {
        }

        public EventLogger(string eventLogName, string eventLogSourceName)
        {
            if (!EventLog.SourceExists(eventLogSourceName))
            {
                EventLog.CreateEventSource(eventLogSourceName, eventLogName);
            }
            eventLog = new EventLog
            {
                Source = eventLogSourceName,
                Log = eventLogName
            };

        }

        private void WriteEventLog(string msg, EventLogEntryType entryType)
        {
            try
            {
                if (eventLog == null)
                    return;

                eventLog.WriteEntry(msg, entryType);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception while writing to EventLog message: {0}{1}Exception: {3}", msg, Environment.NewLine, ex.ToString());
            }
        }

        public void LogInfo(string msg,
                bool logCallerInformation = false,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        { 
            WriteEventLog(LoggerHelper.getMessageWithCaller(msg, logCallerInformation, sourceFilePath, sourceLineNumber, memberName), EventLogEntryType.Information);
            log.Info(msg);
        }

        public void LogWarning(string msg,
                bool logCallerInformation = false,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            WriteEventLog(LoggerHelper.getMessageWithCaller(msg, logCallerInformation, sourceFilePath, sourceLineNumber, memberName), EventLogEntryType.Warning);
            log.Warn(msg);
        }

        public void LogError(string msg,
                bool logCallerInformation = false,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            WriteEventLog(LoggerHelper.getMessageWithCaller(msg, logCallerInformation, sourceFilePath, sourceLineNumber, memberName), EventLogEntryType.Error);
            log.Error(msg);
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

            WriteEventLog(LoggerHelper.getMessageWithCaller(msg, logCallerInformation, sourceFilePath, sourceLineNumber, memberName), EventLogEntryType.Error);
            log.Error(msg);
        }
    }
}
