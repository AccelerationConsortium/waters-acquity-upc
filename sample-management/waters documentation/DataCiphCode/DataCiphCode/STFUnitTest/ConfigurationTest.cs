#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    STFUnitTests
//        NS:    STFUnitTests
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    ConfigurationTest.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:59 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using System;
using CommonSTF;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STFUnitTests;

namespace STFUnitTests
{
    [TestClass]
    public class ConfigurationTest
    {
        [TestMethod]
        public void ExpectConfigurationReaderToPassWithoutException()
        {
            STFServiceHandler serviceHandler = new STFServiceHandler("Servic01", Globals.GetConfigReader(), Globals.GetMultichannelLogger());
            var privateObject = new PrivateObject(serviceHandler);
            bool result = Convert.ToBoolean(privateObject.Invoke("ReadConfiguration"));
            Assert.IsTrue(result);// If no exception test passed.
        }
    }
}
