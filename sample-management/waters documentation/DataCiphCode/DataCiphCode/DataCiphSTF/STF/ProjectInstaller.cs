#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    STFService
//        NS:    STF
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    ProjectInstaller.cs                                                          
//    Author:    Vedran Jašarević | Egeo d.o.o.                                        
//   Created:    1.10.2020 8:33 AM
//   Updated:    10.11.2020 6:45 AM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@gmail.com                                            
//==================================================================================

#endregion
using CommonSTF.Tools;
using CommonSTF.Tools.Configuration;
using CommonSTF.Tools.Loggers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace STF
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private static ILogger _eventLogger1;
        private static ILogger _fileLogger1;
        private static ILogger _multiLogger;

        private ILogger MultiLogger
        {
            get
            {
                if (_multiLogger == null)
                {
                    var pathLogger = FileLogger; 
                    _multiLogger = new MultiLogger(new List<ILogger> { pathLogger, EventLogger }); 
                }
                return _multiLogger;
            }
        }

        private ILogger FileLogger
        {
            get
            {
                if (_fileLogger1 == null)
                {
                    try
                    {
                        string path = getValueFromAppConfig(ConfigKeys.LogFileNamePath) + ".installer.log";
                        string drive = System.IO.Path.GetPathRoot(path);   // e.g. K:\

                        if (System.IO.Directory.Exists(drive))
                        {

                            _fileLogger1 = new FileLogger(path);
                        }
                        else
                        {
                            EventLogger.LogError("Path for log file does not exist: " + path + ", switching to c: ");
                            path = getValueFromAppConfig(ConfigKeys.LogFileNamePath) + ".installer.log";
                            path = "C:" + path.Substring(2, path.Length - 2);

                            EventLogger.LogError("New path: " + path);
                            _fileLogger1 = new FileLogger(path);
                        }
                    }
                    catch (Exception ee)
                    {
                        EventLogger.LogError("Error getting file logger: " + ee.Message); 
                    }
                }
                return _fileLogger1;
            }
        }

        private ILogger EventLogger
        {
            get
            {
                if (_eventLogger1 == null)
                {
                    _eventLogger1 = new EventLogger("STFService");
                }
                return _eventLogger1;
            }
        }

        public ProjectInstaller()
        {
            InitializeComponent();
        }
         
        private string getValueFromAppConfig(ConfigKeys key)
        {
            string assemblypath = Context.Parameters["assemblypath"]; 

            Configuration c = ConfigurationManager.OpenExeConfiguration(assemblypath);
            KeyValueConfigurationCollection configuration = c.AppSettings.Settings;
            var val = configuration[key.ToString()].Value;

            return val;

        }

        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {
            try
            {

                MultiLogger.LogInfo("STFServiceInstaller_AfterInstall:Start service:" + serviceInstaller1.ServiceName);

                // Start the service using ServiceController instead of ProcessManager
                using (var serviceController = new ServiceController(serviceInstaller1.ServiceName))
                {
                    if (serviceController.Status != ServiceControllerStatus.Running)
                    {
                        serviceController.Start();
                        serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    }
                    var isStarted = serviceController.Status == ServiceControllerStatus.Running;
                    MultiLogger.LogInfo("STFServiceInstaller_AfterInstall:ServiceStatus:" + isStarted); 
                }
            }
            catch (Exception ex)
            {
                MultiLogger.LogError("STFServiceInstaller_AfterInstall Exception:" + ex.ToString());
            }
        }

        

        private void serviceInstaller1_AfterUninstall(object sender, InstallEventArgs e)
        { 
        }
         
        private void serviceInstaller1_BeforeUninstall(object sender, InstallEventArgs e)
        { 
        }

        private void serviceInstaller1_Committed(object sender, InstallEventArgs e)
        {
            MultiLogger.LogInfo("STFServiceInstaller_Committed");

            SetRecoveryOptions(serviceInstaller1.ServiceName); 
        }

       
        private void SetRecoveryOptions(string serviceName)
        {
            int exitCode;
            try
            {
                using (var process = new Process())
                {
                    var startInfo = process.StartInfo;
                    startInfo.FileName = "sc";
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    // Tell Windows that the service should restart if it fails.
                    startInfo.Arguments = $"failure \"{serviceName}\" reset= 0 actions= restart/60000";
                    // Failure "your service name" reset= 300 command= "some exe file to execute" actions= restart/20000/run/1000/reboot/1000

                    process.Start();
                    process.WaitForExit();

                    exitCode = process.ExitCode;
                }

                if (exitCode != 0)
                    throw new InvalidOperationException();
            }
            catch (Exception ex)
            {
                MultiLogger.LogError($"SetRecoveryOptions exception: {ex.ToString()}");
            }

        }
    }
}
