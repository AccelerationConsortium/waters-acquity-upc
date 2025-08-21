#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    STFTest
//        NS:    STFTest
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    STFTest.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:59 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using CommonSTF;
using System; 
using System.Windows.Forms;

namespace STFTest
{
    /// <summary>
    /// Class used to manually run Service Handler without installation.
    /// </summary>
    public partial class frmSTFTest : Form
    {
        public frmSTFTest()
        {
            InitializeComponent();
        }

        private void btnTestService_Click(object sender, EventArgs e)
        {
            STFServiceHandler svc = new STFServiceHandler("STF Service TESTER",
                Globals.GetConfigReader(), Globals.GetMultichannelLogger());
            svc.StartService();
        }
    }
}
