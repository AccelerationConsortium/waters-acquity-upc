#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    SampleSetQueueEntryData.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using MillenniumToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonSTF
{
    /// <summary>
    /// Wrapper for holding formatted Sample Sets information from Instrument queue.
    /// </summary>
    public class SampleSetQueueEntryData
    {
        public int JobId { get; set; }
        public string SampleSetName { get; set; }
        public int NumOfInjs { get; set; }
        public double Duration { get; set; }
        public Boolean WaitforUser { get; set; }
        public string RunMode { get; set; }
        public string InteractiveSysSuit { get; set; }
        public string SSMethodName { get; set; }
        public string Acquiredby { get; set; }
        public Boolean Dormant { get; set; }
        public string Project { get; set; }
        
        public SampleSetQueueEntryData(SampleSetQueueEntry entry)
        {
            this.JobId = entry.JobId;
            this.SampleSetName = entry.Get("SampleSetName");
            this.NumOfInjs = Convert.ToInt32(entry.Get("NumOfInjs").ToString());
            this.Duration = Convert.ToDouble(entry.Get("Duration"));
            this.WaitforUser = Convert.ToBoolean(entry.Get("WaitforUser"));
            this.RunMode = ToolkitUtils.GetRunModeName(Convert.ToInt32(entry.Get("RunMode").ToString()));
            this.InteractiveSysSuit = Convert.ToString(entry.Get("InteractiveSysSuit", true));
            this.SSMethodName = entry.Get("SSMethodName");
            this.Acquiredby = entry.Get("Acquiredby");
            this.Dormant = Convert.ToBoolean(entry.Get("Dormant"));
            this.Project = entry.Get("Project"); 

        } 
    }
}
