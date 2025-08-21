#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF.Tools.Configuration
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    IConfig.cs                                                          
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

namespace CommonSTF.Tools.Configuration
{

    public enum ConfigKeys
    {
        LogFileNamePath,
        EventLogName,
        EventLogSourceName,  
        TimerInterval,  
        RunMode, 
        InstrumentConnectionTimeOut,  
        STF_JsonDirectoryPath,
        STF_JsonDirectoryUserName,
        STF_JsonDirectoryPassword,
        STF_JsonDirectoryDomain,
        ServiceId,
        EmpowerUn,
        EmpowerPw,
        EmpowerJSONpwEncrypted,
    }

    /// <summary>
    /// An interface for configuration reader.  
    /// </summary>
    public interface IConfig
    { 

        T GetValue<T>(ConfigKeys forKey);

        string GetStringValue(ConfigKeys forKey);

        int GetIntValue(ConfigKeys forKey);

        bool GetBoolValue(ConfigKeys forKey);

        T GetValue<T>(ConfigKeys forKey, out bool valueExists);

        string GetStringValue(ConfigKeys forKey, out bool valueExists);

        int GetIntValue(ConfigKeys forKey, out bool valueExists);

        bool GetBoolValue(ConfigKeys forKey, out bool valueExists);
    }
}
