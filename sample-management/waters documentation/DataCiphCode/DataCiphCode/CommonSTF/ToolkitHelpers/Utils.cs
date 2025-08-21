#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    Utils.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using CommonSTF.Tools;
using CommonSTF.Tools.Enums;
using MillenniumToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CommonSTF
{
    /// <summary>
    /// Toolkit enumeration and error codes handler.
    /// </summary>
    public class ToolkitUtils
    { 

        public static string GetFunctionName(int? value)
        {
            if (value.HasValue)
            { 
                return string.Join(" ",
                    SplitCamelCase(Enum.GetName(typeof(mtkFunction), value).Replace("mtk", "")));
            }
            return "";
        }

        public static string GetInjectSampleName()
        {
            return GetFunctionName((int)mtkFunction.mtkInjectSamples); 
        }

        public static int GetFunctionValue(string name)
        {
            name = "mtk" + name.Replace(" ", "");
            return EnumUtil.GetValue<mtkFunction> (name);
        } 
         
        public static string GetRunModeName(int value)
        {
            return string.Join(" ",
                SplitCamelCase(Enum.GetName(typeof(mtkRunMode), value).Replace("mtk", "")));
        }
          
        public static string[] SplitCamelCase(string source)
        {
            return System.Text.RegularExpressions.Regex.Split(source, @"(?<!^)(?=[A-Z])");
        }
       

        /// <summary>
        /// Roughly hewn function that is designed to convert the error code the interop returns to
        /// something matching the error codes in the Empower manuals.
        /// Although most errors are in Hexadecimal format some are not so a straight conversion often
        /// fails, for this reason we return the last three chars as a string rather than an int.
        /// </summary>
        /// <param name="errorNumber">The interop error number</param>
        /// <returns>A string that matches the codes within the Empower manual... and maybe some more.</returns>
        public static string ToolkitErrorCodeExtract(int errorNumber)
        {
            // Realistically we should deduct 80040000 but as not all results are integers when returned.
            // In hex we get some errors, so the best bet is to only return the last 3 chards.
             
            return errorNumber.ToString("X").Replace("80040", "");
        }


        public static string GetErrorMessagefromToolkitException(System.Runtime.InteropServices.COMException exc)
        { 
            string errorCode = (string)(ToolkitUtils.ToolkitErrorCodeExtract(exc.ErrorCode).ToString());
            string errorMessage = string.Format("Unknown error", errorCode, exc.ErrorCode.ToString(), exc.Message);
          
            string s = ToolkitHelpers.ErrorCodes.ResourceManager.GetString(errorCode.ToLower());
            if (!string.IsNullOrEmpty(s))
            {
                errorMessage = s;
            }
       
            return errorMessage;
        }
         
    }
}
