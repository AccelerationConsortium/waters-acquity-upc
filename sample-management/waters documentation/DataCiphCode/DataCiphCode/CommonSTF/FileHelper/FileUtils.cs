#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF.FileHelper
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    FileUtils.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using CommonSTF.Tools.Loggers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonSTF.FileHelper
{
    /// <summary>
    /// Class takes care of JSON files reading, renaming, locking, sorting, saving....
    /// </summary>
    public class FileUtils
    {
        #region Privates
        private ILogger _logger;
        #endregion

        #region const fields
        /// <summary>
        /// Files that needs to be processed must contains "new" keyword in file name
        /// </summary>
        private const string keywordNew = ".new.json";

        /// <summary>
        /// When file is processing by service, file will be temporary renamed, and "new" keyword in file name will be replaced with "lck" - as locked
        /// </summary>
        private const string keywordLock = ".lck.json";

        /// <summary>
        /// When service is finished with file processing, file will be renamed. The "lck" keyword will be replaced with "prc" - as processed.
        /// </summary>
        private const string keywordProcessed = ".prc.json";


        /// <summary>
        /// When verification process of file validation fail, file will be renamed. The "lck" keyword will be replaced with "error" - an error occurred.
        /// </summary>
        private const string keywordError= ".error.json";
       
        /// <summary>
        /// When deserialization process of file fail, file will be renamed. The "lck" keyword will be replaced with "error-deserialization" - an error occurred.
        /// </summary>
        private const string keywordErrorDeserialization = ".error-deserialization.json";
        #endregion const fields
        

        #region Constructors
        public FileUtils(ILogger logger)
        { 
            _logger = logger;
        }

        internal JSONobjectComparer JSONobjectComparer
        {
            get => default(JSONobjectComparer);
            set
            {
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Return all Files in top folder that contains "new" keyword in file name.
        /// </summary>
        /// <param name="folderPath">The path to JSON files directory</param>
        public JsonObject[] GetNewFilesForProcessingInTopFolderSortedAscByDate(string folderPath)
        {
            // The search string to match against the names of files in path.This parameter can 
            // contain a combination of valid literal path and wildcard(*and ?) characters, 
            // but it doesn't support regular expressions.
            // * (asterisk)	Zero or more characters in that position.
            // ? (question mark)	Zero or one character in that position.
            // example: searchPattern = "c*" - Only get files that begin with the letter "c".
            string[] fileList =  Directory.GetFiles(folderPath, $"*{keywordNew}*", SearchOption.TopDirectoryOnly);

            JsonObject[] jSONobjects = new JsonObject[fileList.Length];
            for (int i= 0; i <  fileList.Length; ++i)
            {
                JsonObject jo = new JsonObject();
                //jo.SetFileName(fileList[i]);
                jo.FileName = fileList[i];
                jSONobjects[i] = jo;
            }

            Array.Sort(jSONobjects, new JSONobjectComparer());

            return jSONobjects;
        }

        /// <summary>
        /// Rename file and change "new" keyword "to "lock" keyword.
        /// </summary>
        /// <param name="jo">JSON file mapped object to rename</param> 
        /// <returns>Locked File name</returns>
        public string RenameFileAsLocked(JsonObject jo)
        {
            string newFileNamePath =jo.CurrentFileName.Replace(keywordNew, keywordLock);
            File.Move(jo.CurrentFileName, newFileNamePath);
            jo.CurrentFileName = newFileNamePath;
            return newFileNamePath;
        }

        /// <summary>
        /// Rename file and change "lock" keyword "to "processed" keyword.
        /// </summary>
        /// <param name="jo">JSON file mapped object to rename</param> 
        /// <returns>Processed File name</returns>
        public string RenameFileAsProcessed(JsonObject jo)
        {
            string newFileNamePath = jo.CurrentFileName.Replace(keywordLock, keywordProcessed);
            File.Move(jo.CurrentFileName, newFileNamePath);
            jo.CurrentFileName = newFileNamePath;
            return newFileNamePath;
        }

        /// <summary>
        /// Rename file and change "lock" keyword "to "error" keyword.
        /// </summary>
        /// <param name="jo">JSON file mapped object to rename</param> 
        /// <returns>Error File name</returns>        
        public string RenameFileAsError(JsonObject jo)
        {
            string newFileNamePath = jo.CurrentFileName.Replace(keywordLock, keywordError);
            File.Move(jo.CurrentFileName, newFileNamePath);
            jo.CurrentFileName = newFileNamePath;
            return newFileNamePath;
        }

        /// <summary>
        /// Rename file and change "lock" keyword "to "error-deserialization" keyword.
        /// </summary>
        /// <param name="jo">JSON file mapped object to rename</param> 
        /// <returns>Error-deserialization File name</returns>       
        public string RenameFileAsErrorDeserialization(JsonObject jo)
        {
            string newFileNamePath = jo.CurrentFileName.Replace(keywordLock, keywordErrorDeserialization);
            File.Move(jo.CurrentFileName, newFileNamePath);
            jo.CurrentFileName = newFileNamePath;
            return newFileNamePath;
        }

        /// <summary>
        /// Write changes back to file. If file exists, file will be deleted first.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool SaveFile(string path, string text)
        {
            try
            {
                File.Delete(path);
                File.WriteAllText(path, text);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Save file error: path={path}; text={text}", ex);
                return false;
            }
        }
        #endregion Public methods
    }

    /// <summary>
    /// Internal JSON file list comparer.
    /// </summary>
    internal  class JSONobjectComparer : IComparer<JsonObject>
    {
        // Call CaseInsensitiveComparer.Compare with the parameters reversed.
        public int Compare(JsonObject x, JsonObject y)
        {
            return DateTime.Compare(x.Date, y.Date);
        }
    }
}
