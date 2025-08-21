#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    STFUnitTests
//        NS:    STFUnitTests
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    JsonObjectTests.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:59 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using System;
using System.Collections.Generic;
using CommonSTF;
using CommonSTF.FileHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace STFUnitTests
{
    [TestClass]
    public class JSONObjectTests
    {
        // IntegratorID_integratorjobID_yymmdd_hhmm.new.json
        private const string _fileNameValid = "1234_5678_200930_1547.new.json";

        [TestMethod]
        public void ExpectFileNameParsingToSucceed()
        {
            JsonObject jo = new JsonObject();
            jo.FileName = _fileNameValid;

            Assert.AreEqual("1234", jo.IntegratorID);
            Assert.AreEqual("5678", jo.IntegratorJobID);
            Assert.AreEqual(new DateTime(2020,09,30,15,47,00), jo.Date);
        }

        [TestMethod]
        public void ExpectJsonValidationToSucceed()
        {
            var jo = new JsonObject();

            jo.FileName = _fileNameValid; 

            jo.SampleSetDetails = new List<SampleSetDetailsData>();
            jo.HeaderFields = new HeaderFieldsData() { SampleSets = 1 };
            jo.HeaderFields.EmpowerUn = "testun";
            jo.HeaderFields.EmpowerPw = "testpws";
            jo.TrailerReport = new TrailerReportData();
            var ssDetail = new SampleSetDetailsData();
            ssDetail.NumberOfSamples = 2;
            var lines = new List<Dictionary<string, object>>();
            lines.Add(new Dictionary<string, object>() {
                {"LineNumber", 1 },
                {"Function", "StandardInjection" },
                {"Vial", "A:1" },
            });
            lines.Add(new Dictionary<string, object>() {
                {"LineNumber", 2 },
                {"Function", "StandardInjection" },
                {"Vial", "A:2" },
            });

            ssDetail.Samples = lines;

            jo.SampleSetDetails.Add(ssDetail);


            JsonValidator jonValidator = new JsonValidator(Globals.GetMultichannelLogger());
            Assert.AreEqual(true, jonValidator.ValidateJSONObject(jo, out string error));
        }
    }
}
