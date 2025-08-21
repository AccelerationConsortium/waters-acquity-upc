#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    EncryptPassword
//        NS:    EncryptPassword
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    Program.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:59 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EncryptPassword
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new EncryptPassword());
        }
    }
}
