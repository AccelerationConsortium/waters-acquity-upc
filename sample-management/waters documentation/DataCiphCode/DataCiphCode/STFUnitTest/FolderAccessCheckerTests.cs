#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    STFUnitTests
//        NS:    STFUnitTests
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    FolderAccessCheckerTests.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:59 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using System;
using CommonSTF.FileHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STFUnitTests;

namespace STFUnitTests
{
    [TestClass]
    public class FolderAccessCheckerTests
    {
        [TestMethod]
        public void TestCheckFolderAccessRights()
        {
            FolderAccessChecker fac = new FolderAccessChecker(Globals.GetMultichannelLogger());

           Assert.AreEqual(true, fac.CheckFolderAccessRights("c:\\STF"));
        }

        [TestMethod]
        public void TestCheckNonExistingFolderAccessRights()
        {
            FolderAccessChecker fac = new FolderAccessChecker(Globals.GetMultichannelLogger());

            Assert.AreNotEqual(true, fac.CheckFolderAccessRights("c:\\STF_1"));
        }
    }
}
