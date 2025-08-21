#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF.Tools.Loggers
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    LoggerHelper.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonSTF.Tools.Loggers
{
    /// <summary>
    /// Helper class contains utilities for easier logging.
    /// </summary>
    public class LoggerHelper
    {
        /// <summary>
        /// When handling logging from outer classes, this method is returning formatted message with parent class an method class, also known as source of invoke.
        /// </summary>
        /// <param name="msg">Log message to format</param>
        /// <param name="logCallerInformation">True if we want to include source of invoke.</param>
        /// <param name="sourceFilePath">Invoker class file path and name.</param>
        /// <param name="sourceLineNumber">Line number of invoker class where call/exc was initialized.</param>
        /// <param name="methodName">Method name of invoker.</param>
        /// <returns></returns>
        public static string getMessageWithCaller(string msg, bool logCallerInformation, string sourceFilePath, int sourceLineNumber, string methodName)
        {
            if (logCallerInformation)
            {
                int index = sourceFilePath.LastIndexOf("\\") + 1;
                return (sourceFilePath.Substring(index, sourceFilePath.Length - index)
                    + "(" + sourceLineNumber + ")" + "." + methodName + ": " + msg);
            }
            else
            {
                return msg;
            }
        }
    }
}
