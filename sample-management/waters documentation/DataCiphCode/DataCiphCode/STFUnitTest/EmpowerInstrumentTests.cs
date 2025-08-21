#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    STFUnitTests
//        NS:    STFUnitTests
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    EmpowerInstrumentTests.cs                                                          
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
    public class EmpowerInstrumentTests
    {
        [TestMethod]
        public void ExpectConnectionToInstrumentToSucceed()
        {
            EmpowerProject ep = new EmpowerProject(Globals.GetMultichannelLogger());
            ep.Login(new LoginData("system", "manager1", "WAT10", "Defaults1\\stf_FAT_build_project"), out string error);

            EmpowerInstrument ei = new EmpowerInstrument(Globals.GetMultichannelLogger());

            Assert.IsTrue(ei.ConnectToInstrumentAndWaitForConnection("Win-ju14ss8pet8", "Vedran_2_fake", 5, out error));           
        }

        [TestMethod]
        public void ExpectConnectionToInstrumentToFail()
        {
            EmpowerProject ep = new EmpowerProject(Globals.GetMultichannelLogger());
            ep.Login(new LoginData("system", "manager1", "WAT10", "Defaults1\\stf_FAT_build_project"), out string error);

            EmpowerInstrument ei = new EmpowerInstrument(Globals.GetMultichannelLogger());

            Assert.IsFalse(ei.ConnectToInstrumentAndWaitForConnection(Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(), 5, out error));
        }

        [TestMethod]
        public void ExpectRunJobToFail()
        {
            EmpowerProject ep = new EmpowerProject(Globals.GetMultichannelLogger());
            ep.Login(new LoginData("system", "manager1", "WAT10", "Defaults1\\stf_FAT_build_project"), out string error);

            EmpowerInstrument ei = new EmpowerInstrument(Globals.GetMultichannelLogger());
           
            if (ei.ConnectToInstrumentAndWaitForConnection("Win-ju14ss8pet8", "Vedran_2_fake", 5, out error))
            {
                // This is inside of FAT project. SSM needs to be saved with comment before - so this wil fail.
                Assert.IsFalse(ei.Run("STF_base_SSM", "STF_base_SSM", out error));
            }
            else
            {
                Assert.Fail();
            }
        }


        [TestMethod]
        public void ExpectPauseInstrumentToSucceed()
        {
            EmpowerProject ep = new EmpowerProject(Globals.GetMultichannelLogger());
            ep.Login(new LoginData("system", "manager1", "WAT10", "Defaults1\\stf_FAT_build_project"), out string error);

            EmpowerInstrument ei = new EmpowerInstrument(Globals.GetMultichannelLogger());

            if (ei.ConnectToInstrumentAndWaitForConnection("Win-ju14ss8pet8", "Vedran_2_fake", 5, out error))
            {
                Assert.IsTrue(ei.Pause(20, out error));
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void ExpectReplaceJobInQueueToFail()
        {
            EmpowerProject ep = new EmpowerProject(Globals.GetMultichannelLogger());
            ep.Login(new LoginData("system", "manager1", "WAT10", "Defaults1\\stf_FAT_build_project"), out string error);

            EmpowerInstrument ei = new EmpowerInstrument(Globals.GetMultichannelLogger());

            if (ei.ConnectToInstrumentAndWaitForConnection("Win-ju14ss8pet8", "Vedran_2_fake", 5, out error))
            {
                Assert.IsFalse(ei.Replace(Guid.NewGuid().ToString(), out error));
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void ExpectRemoveSSMFromQueueToFail()
        {
            EmpowerProject ep = new EmpowerProject(Globals.GetMultichannelLogger());
            ep.Login(new LoginData("system", "manager1", "WAT10", "Defaults1\\stf_FAT_build_project"), out string error);

            EmpowerInstrument ei = new EmpowerInstrument(Globals.GetMultichannelLogger());

            if (ei.ConnectToInstrumentAndWaitForConnection("Win-ju14ss8pet8", "Vedran_2_fake", 5, out error))
            {
                Assert.IsFalse(ei.RemoveSSMFromQueue(Guid.NewGuid().GetHashCode(), out error));
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}
