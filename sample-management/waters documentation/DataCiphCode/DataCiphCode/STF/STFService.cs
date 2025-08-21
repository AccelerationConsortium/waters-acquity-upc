#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    STFService
//        NS:    STFService
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    STFService.cs                                                          
//    Author:    Vedran Jašarević | Egeo d.o.o.                                        
//   Created:    30.9.2020 6:34 AM
//   Updated:    10.11.2020 6:45 AM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@gmail.com                                            
//==================================================================================

#endregion
using CommonSTF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace STFService
{
    /// <summary>
    /// Service base class with start and stop function for main service.
    /// ServiceHandler from Common STF project is doing all the job.
    /// It is because debugging of service is much easier in this way.
    /// </summary>
    public partial class STFService : ServiceBase
    {
        private STFServiceHandler _serviceLogic;
        
        public STFService()
        {
            InitializeComponent();
      
            _serviceLogic = new STFServiceHandler(ServiceName, Globals.GetConfigReader(), Globals.GetMultichannelLogger());
        }

        protected override void OnStart(string[] args)
        {
            _serviceLogic.StartService();
        }

        protected override void OnStop()
        {
            _serviceLogic.StopService();
        }
    }
}
