#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    STFService
//        NS:    STF
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    Program.cs                                                          
//    Author:    Vedran Jašarević | Egeo d.o.o.                                        
//   Created:    30.9.2020 6:34 AM
//   Updated:    10.11.2020 6:45 AM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@gmail.com                                            
//==================================================================================

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace STF
{
    /// <summary>
    /// Main class for running STF Service. It will use ServiceHandler from Common STF.
    /// </summary>
    static class Program
    {
        public static STFService.STFService STFService
        {
            get => default(STFService.STFService);
            set
            {
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        { 
            // Start service.
            var svc = new STFService.STFService();
            var servicesToRun = new ServiceBase[]
            {
              svc
            };

            ServiceBase.Run(servicesToRun);
        }
    }
}
