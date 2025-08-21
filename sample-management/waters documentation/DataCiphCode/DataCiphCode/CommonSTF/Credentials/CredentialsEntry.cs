#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    CredentialsEntry.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using CommonSTF.Tools;

namespace CommonSTF
{
    /// <summary>
    /// User/password holder. It can return encrypted or decrypted password according to AES.
    /// </summary>
    public class CredentialsEntry
    {         
             
        public string Username { get; set; } 
                
        public string EncodedPassword { get; set; }                 

        public string GetDecodedPassword()
        {
            return Crypto.DecryptStringAES(EncodedPassword, Crypto.SharedSecret);
        }

    }
}
