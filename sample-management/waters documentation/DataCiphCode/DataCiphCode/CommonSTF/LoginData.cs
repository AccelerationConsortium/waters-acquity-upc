#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    LoginData.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
namespace CommonSTF
{
    /// <summary>
    /// Holder for project login attributes.
    /// </summary>
    public class LoginData
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string UserType { get; set; }
        public string Project { get; set; }
        public string Schema { get; set; }

        public LoginData(string Username, string Password, string Database)
        {
            this.Database = Database;
            this.Password = Password;
            this.Username = Username;
            this.Project = string.Empty;
            this.UserType = string.Empty;
            this.Schema = string.Empty;

        }

        public LoginData(string Username, string Password, string Database, string Project)
            : this(Username, Password, Database)
        {
            this.Project = Project;
        }

        public LoginData(string Username, string Password, string Database, string Project, string UserType)
            : this(Username, Password, Database, Project)
        {
            this.UserType = UserType;
        }
    }
}
