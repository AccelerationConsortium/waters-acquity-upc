#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    EmpowerProject.cs                                                          
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
    /// Wrapper for the Project object which is a general-purpose object you use to log into the Empower database and to launch Empower applications.
    /// Attention: You must log in using the Project object before using any other object.
    /// Attempting to create any other objects before logging in generates an error.
    /// It cares logging and error handling.
    /// </summary>
    public class EmpowerProject
    {
        #region Fields
        #region _project
        /// <summary>
        /// Toollkit original object.
        /// </summary>
        Project _project = new Project();
        #endregion

        private ILogger _logger;
        #endregion

        #region Constructor
        public EmpowerProject(ILogger logger)
        {
            _logger = logger;
        }
        #endregion

        #region Properties                
        #region DefaultAuditTrailComment
        /// <summary> 
        /// Default comment for all FAT projects when saving back changes.
        /// </summary>
        public string DefaultAuditTrailComment
        {
            set {  _project.DefaultAuditTrailComment = value; }
            get { return _project.DefaultAuditTrailComment; }
        }

        public LoginData LoginData
        {
            get => default(LoginData);
            set
            {
            }
        }
        #endregion

        #endregion

        #region Functions    
        #region Login
        /// <summary>
        /// Providing login function to Project object. In most cases this will be first call when 
        /// working with toolkit (you can only read database list and project list before).
        /// </summary>
        /// <param name="loginData">Object containing empower database, full project path, user name, password and login type.</param>
        /// <param name="errorMessage">Returning possible error message.</param>
        /// <returns>True if action completed successful, false if error occurred.</returns>
        public bool Login(LoginData loginData, out string errorMessage)
        {
            errorMessage = "";
            try
            {                 
                _project.Login(loginData.Database, loginData.Project, loginData.Username, 
                    loginData.Password, loginData.UserType); 
                return true;
            }
            catch (Exception exc)
            {
                errorMessage = exc.Message;
                if (exc is System.Runtime.InteropServices.COMException)
                {
                    errorMessage = ToolkitUtils.GetErrorMessagefromToolkitException((System.Runtime.InteropServices.COMException)exc);
                }
                _logger.LogError("Login() : " + errorMessage);
                return false;
            }
        }
        #endregion

        #region Logoff
        /// <summary>
        /// Log off from project before using another project.
        /// </summary>
        /// <param name="errorMessage">Returning possible error message.</param>
        /// <returns>True if action completed successful, false if error occurred.</returns>
        public bool Logoff(out string errorMessage)
        {
            errorMessage = "";
            try
            {
                _project.Logoff();
                return true;
            }
            catch (Exception exc)
            {
                errorMessage = exc.Message;
                if (exc is System.Runtime.InteropServices.COMException)
                {
                    errorMessage = ToolkitUtils.GetErrorMessagefromToolkitException((System.Runtime.InteropServices.COMException)exc);
                }
                _logger.LogError(errorMessage, exc);
                return false;
            }
        }
        #endregion
        #endregion
    }
}