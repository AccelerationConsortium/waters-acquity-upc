#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    JsonObject.cs                                                          
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
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CommonSTF
{
    /// <summary>
    /// Class is mapped version of data from JSON file.
    /// It needs to be changed/updated if integrator JSON file format is changed.
    /// It also contains additional informations from file name: full FileName, IntegratorId, IntegratorJobID, Date.
    /// </summary>
    public class JsonObject
    {
        private string _fileName;
        private string _currentFileName;
        private string _integratorID;
        private string _integratorJobID;
        private DateTime _date;


        [JsonIgnore]
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                if (string.IsNullOrEmpty(_currentFileName))
                {
                    _currentFileName = value;
                }
                ParseFileName(this);
            }
        }

        [JsonIgnore]
        public string CurrentFileName
        {
            get { return _currentFileName; }
            set { _currentFileName = value; }
        }

        [JsonIgnore]
        public string IntegratorID { get { return _integratorID; } }

        [JsonIgnore]
        public string IntegratorJobID { get { return _integratorJobID; } }

        [JsonIgnore]
        public DateTime Date { get { return _date; } }

        public HeaderFieldsData HeaderFields { get; set; }
        public IList<SampleSetDetailsData> SampleSetDetails { get; set; }
        public TrailerReportData TrailerReport { get; set; }

        #region Public static methods 
        /// <summary>
        /// Parse integration ID, integrator JOB ID and date from JSON file name.
        /// </summary>
        /// <param name="jo"></param>
        public static void ParseFileName(JsonObject jo)
        {
            //IntegratorID_integratorjobID_yymmdd_hhmm.new.json
            if (!string.IsNullOrEmpty(jo.FileName))
            {
                //string fileNameWOPath = jo._fileName.Substring(jo._fileName.LastIndexOf("\\") + 1);//remove directory path
                string fileNameWOPath = System.IO.Path.GetFileName(jo.FileName);
                string[] temp = fileNameWOPath.Substring(0, fileNameWOPath.IndexOf(".")).Split(new char[] { '_' });
                if (temp.Length == 4)
                {
                    jo._integratorID = temp[0];
                    jo._integratorJobID = temp[1];
                    string d = temp[2] + " " + temp[3];
                    jo._date = DateTime.ParseExact(d, "yyMMdd HHmm", System.Globalization.CultureInfo.InvariantCulture);
                    System.Diagnostics.Debug.WriteLine(jo._date.ToLongDateString());
                }
                else
                {
                    throw new Exception($"File name format {jo.FileName} incorrect!");
                }
            }
            else
            {
                throw new Exception("File name cannot be empty!");
            }
        } 
        #endregion
    }

    /// <summary>
    /// Contains root object from JSON file.
    /// </summary>
    public class HeaderFieldsData
    {
        /// <summary>
        /// Full empower path need to be specified in JSON.
        /// </summary>
        public string EmpowerProject { get; set; }

        /// <summary>
        /// Empower DB name used for Project login.
        /// </summary>
        public string EmpowerDatabase { get; set; }

        /// <summary>
        /// Empower user name used for Project login.
        /// </summary>
        public string EmpowerUn { get; set; }

        /// <summary>
        /// Empower password used for Project login.
        /// It can be encrypted, then you need to specify encryption (yes/no) in configuration file.
        /// </summary>
        public string EmpowerPw { get; set; }

        /// <summary>
        /// Name of the instrument we will send jobs.
        /// </summary>
        public string System { get; set; }

        /// <summary>
        /// Name of the node where selected instrument is installed.
        /// </summary>
        public string Node { get; set; } 

        /// <summary>
        /// Number of JOBS in child array. Used for JSON validation.
        /// </summary>
        public int SampleSets { get; set; }
    }

    /// <summary>
    /// Contains collections of jobs from JSON file. Each job contains collection
    /// of Samples that need to added to queue. Collection of Samples is key-value list,
    /// where key is Sample Set Line field name with corresponded value.
    /// </summary>
    public class SampleSetDetailsData
    { 
        /// <summary>
        /// Sample Set Method name used as base method for cloning samples lines.
        /// </summary>
        public string BaseSampleSetMethodName { get; set; }

        /// <summary>
        /// Sample Set Method name where newly created SSM will be stored.
        /// </summary>
        /// 
        public string SampleSetName { get; set; }

        /// <summary>
        /// Experiment ID from integrator.
        /// </summary>
        public int ExperimentId { get; set; }
        /// <summary>
        /// Need to be equal to Samples count.
        /// </summary>
        public int NumberOfSamples { get; set; }

        /// <summary>
        /// If true, new integrator sent new SSM, if false then update of existing job is required.
        /// </summary>
        public bool New { get; set; }

        /// <summary>
        /// Key value pair for each sample set line.
        /// </summary>
        public IList<Dictionary<string, Object>> Samples { get; set; }

        /// <summary>
        /// Field to write back status of processed job.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Details message of processed job status.
        /// </summary>
        public string SampleSetMethodRunReport { get; set; }
    }

    /// <summary>
    /// Information of single JSON file status.
    /// </summary>
    public class TrailerReportData
    {
        /// <summary>
        /// Information if file verification passed of failed.
        /// </summary>
        public bool FileVerified { get; set; }

        /// <summary>
        /// Was file processed or not.
        /// </summary>
		public bool FileProcessed { get; set; }

        /// <summary>
        /// File status after processing: "Not started" or "Completed"...
        /// </summary>
        public string FileStatus { get; set; }

        /// <summary>
        /// Report for whole file: example: "File completed 2 ssm processed, 2 succeeded, 0 failed."
        /// </summary>
        public string FileProcessReport { get; set; } 
    }
}

