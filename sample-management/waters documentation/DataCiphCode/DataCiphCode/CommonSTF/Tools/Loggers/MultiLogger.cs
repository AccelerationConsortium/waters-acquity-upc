#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF.Tools.Loggers
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    MultiLogger.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CommonSTF.Tools.Loggers
{

    /// <summary>
    /// Handling multi channel logging. It can consist from one or many Log2Net, File logger, Event Logger and potential Database Logger.
    /// </summary>
    public class MultiLogger:ILogger
    {
        private  List<ILogger> _loggers;
         

        public MultiLogger(List<ILogger> loggers)
        {
            _loggers = loggers;
        }
  
        public Log4NetLogger ReturnLog4NetLogger()
        {
            foreach (var l in _loggers)
            {
                if (l is Log4NetLogger)
                    return (Log4NetLogger)l;
            }
            return null;
        }

        public void LogError(string msg,
                bool logCallerInformation = false,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            foreach (var l in _loggers)
            {
                l.LogError(msg);
            }
        }

        public void LogError(string msg, System.Exception ex,
               bool logCallerInformation = false,
               [CallerMemberName] string memberName = "",
               [CallerFilePath] string sourceFilePath = "",
               [CallerLineNumber] int sourceLineNumber = 0)
        {
            foreach (var l in _loggers)
            {
                l.LogError(msg, ex, logCallerInformation, memberName, sourceFilePath, sourceLineNumber);
            }
        }

        public void LogInfo(string msg,
                bool logCallerInformation = false,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            foreach (var l in _loggers)
            {
                l.LogInfo(msg);
            }
        }

        public void LogWarning(string msg,
                bool logCallerInformation = false,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            foreach (var l in _loggers)
            {
                l.LogWarning(msg);
            }
        }
    }
}
