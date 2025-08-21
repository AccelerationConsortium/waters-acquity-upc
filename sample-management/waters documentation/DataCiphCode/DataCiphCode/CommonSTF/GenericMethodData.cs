#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    GenericMethodData.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonSTF
{
    /// <summary>
    /// Wrapper object for Method toolkit object. It contains ID, Name, Date, ModifiedBy and Comments.
    /// </summary>
    public class GenericMethodData
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string ModifiedBy { get; set; }
        public string Comments { get; set; }

        public GenericMethodData(int ID, string Name, DateTime Date, string ModifiedBy, string Comments)
        {
            this.ID = ID;
            this.Name = Name;
            this.Date = Date;
            this.ModifiedBy = ModifiedBy;
            this.Comments = Comments;
        }
    }
}
