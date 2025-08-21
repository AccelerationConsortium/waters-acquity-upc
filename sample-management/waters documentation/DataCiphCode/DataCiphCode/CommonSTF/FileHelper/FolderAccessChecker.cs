#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF.FileHelper
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    FolderAccessChecker.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using CommonSTF.Tools.Configuration;
using CommonSTF.Tools.Loggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonSTF.FileHelper
{
    /// <summary>
    /// The class that take care if process has right to read/write/delete files to desired path.
    /// </summary>
    public class FolderAccessChecker
    { 
        private ILogger _logger;

        public FolderAccessChecker(ILogger multiLogger)
        {
            _logger = multiLogger;
        }

        /// <summary>
        /// Check if the user can write to the desired folder.
        /// </summary>
        /// <param name="path">Full folder path (from root folder to current folder)</param>
        /// <returns>True if access to folder is granted, False otherwise.</returns>
        public bool CheckFolderAccessRights(string path)
        { 
            if (!HasWriteAccessToFolder(path))
            {
                _logger.LogError($"No access to {path}!");
                return false;
            }
            _logger.LogInfo($"Full access to {path}!");
            return true;
        }

        /// <summary>
        /// Checks the ability to create and write to a file in the supplied directory.
        /// </summary>
        /// <param name="folderPath">string representing the folder path to check.</param>
        /// <returns>
        ///     True if successful; otherwise false.
        /// </returns>
        private static bool HasWriteAccessToFolder(string folderPath)
        {
            var retval = false;
            var fullPath = Path.Combine(folderPath, $"{Guid.NewGuid().ToString()}.temp");

            if (!Directory.Exists(folderPath)) return false;
            try
            {
                using (var fs = new FileStream(fullPath, FileMode.CreateNew,
                    FileAccess.Write))
                {
                    fs.WriteByte(0xff);
                }

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    retval = true;
                }
            }
            catch
            {
                retval = false;
            }
            return retval;
        }
    }
}
