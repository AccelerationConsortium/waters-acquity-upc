#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    SampleSetMethodLineData.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using CommonSTF.Tools.Loggers;
using MillenniumToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonSTF
{
    /// <summary>
    /// Contains converted/formatted data from toolkit Sample Set Line.
    /// It implements ICloneable interface, so it can deep clone lines (for copying lines from existing empower lines).
    /// </summary>   
    public class SampleSetMethodLineData : ICloneable
    {
        internal  SampleSetLine originalToolkitLine { get; set; }  

        public string Vial { get; set; }//vial position on plate
        public double? InjVol { get; set; }
        public int? NumOfInjs { get; set; }
        public string Label { get; set; }
        public string SampleName { get; set; }
        public string Level { get; set; }
        public int? Function { get; set; }
        public string FunctionName
        {
            get
            {                
                return ToolkitUtils.GetFunctionName(Function);
            }
        }
        public string MethodSetOrReportMethod { get; set; }
        public string LabelReference { get; set; }
        public int? Processing { get; set; }
        public double? RunTime { get; set; }
        public double? DataStart { get; set; }
        public double? NextInjDelay { get; set; }

        public SampleSetMethodLineField SampleSetMethodLineField
        {
            get => default(SampleSetMethodLineField);
            set
            {
            }
        }

        /// <summary>
        /// Any additional columns that are not in the standard empower column list.
        /// </summary>
        public List<KeyValuePair<string, string>> nonStandardAdditionalColumns = new List<KeyValuePair<string, string>>();     

        /// <summary>
        /// Constructor for creating new object with key value pair of  fields.
        /// </summary>
        /// <param name="fields">KeyValue list of fields (can be standard Empower or non standard fields).</param>
        public SampleSetMethodLineData(List<KeyValuePair<string, string>> fields)
        { 
            this.nonStandardAdditionalColumns = fields;
        }

        /// <summary>
        /// Constructor for creating new object with passed parameter values.
        /// </summary>
        /// <param name="Vial">Vial label</param>
        /// <param name="InjVol">Injection volume amount</param>
        /// <param name="NumOfInjs">Number of Injections</param>
        /// <param name="Label">Line label</param>
        /// <param name="SampleName">Sample name</param>
        /// <param name="Level">Level</param>
        /// <param name="Function">Function value (int)</param>
        /// <param name="MethodSetOrReportMethod">Name of MethosSet or Report Method</param>
        /// <param name="LabelReference">Label Reference</param>
        /// <param name="Processing">Processing type</param>
        /// <param name="RunTime">Estimated Run Time</param>
        /// <param name="DataStart">Putting an offset into the data collection from the detector... as it takes time for the "result" 
        /// (the "stuff" to go from the separation system down the pipe to the detector, 
        /// so you can set a delay if you want to line up the actual time of leaving the chromatography separation system and the detector)</param>
        /// <param name="NextInjDelay">A delay time before moving to the next injection</param>
        /// <param name="additionalColumns">Additional non standard fields (key value pair: name of the field and value)</param>
        public SampleSetMethodLineData(string Vial, double? InjVol, int? NumOfInjs, string Label, string SampleName,
            string Level, int? Function, string MethodSetOrReportMethod, string LabelReference, int? Processing,
             double? RunTime, double? DataStart, double? NextInjDelay,  
             List<KeyValuePair<string, string>> additionalColumns = null)
        {
            this.InjVol = InjVol;
            this.Label = Label;
            this.NumOfInjs = NumOfInjs;
            this.SampleName = SampleName;
            this.Vial = Vial;
            this.Level = Level;
            this.Function = Function;
            this.MethodSetOrReportMethod = MethodSetOrReportMethod;
            this.LabelReference = LabelReference;
            this.Processing = Processing;
            this.RunTime = RunTime;
            this.DataStart = DataStart;
            this.NextInjDelay = NextInjDelay; 
            this.nonStandardAdditionalColumns = additionalColumns;          
        }

        /// <summary>
        /// Clone method for deep cloning of object (by value). It will also clone original empower line if it is included in object.
        /// In this way we can copy existing sample set line from empower, changes some values if needed, and add newly created line 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var cloned =(SampleSetMethodLineData) this.MemberwiseClone();

            if (nonStandardAdditionalColumns != null)
            {
                cloned.nonStandardAdditionalColumns = new List<KeyValuePair<string, string>>();

                foreach(var pair in nonStandardAdditionalColumns)
                {
                    cloned.nonStandardAdditionalColumns.Add(new KeyValuePair<string, string>((string)pair.Key.Clone(), (string)pair.Value.Clone()));
                }
            }

            if (originalToolkitLine != null)
            {
                var clonedOrigLine = new SampleSetLine();
                clonedOrigLine.Components = originalToolkitLine.Components;                
                clonedOrigLine.KAlphas = originalToolkitLine.KAlphas;
                clonedOrigLine.MoleWeights = originalToolkitLine.MoleWeights;
                clonedOrigLine.Moments = originalToolkitLine.Moments;

                Fields f = originalToolkitLine.Fields;
                for (int i = 0; i < f.Count; ++i)
                {
                    string fieldName = f.Item(i);
                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        clonedOrigLine.Set(fieldName, originalToolkitLine.Get(fieldName));
                    }
                }

                cloned.originalToolkitLine = clonedOrigLine;
            }
            return cloned;
        }
 
    }
}
