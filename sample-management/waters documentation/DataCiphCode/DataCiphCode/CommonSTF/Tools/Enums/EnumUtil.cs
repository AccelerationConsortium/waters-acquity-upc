#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    CommonSTF
//        NS:    CommonSTF.Tools.Enums
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    EnumUtil.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:58 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using CommonSTF.Tools.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonSTF.Tools.Enums
{
    public static class EnumUtil
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static int GetValue<T>(string keyLabel)
        {
            return (int)Enum.Parse(typeof(T), keyLabel);
        }

        public static IEnumerable<T> GetNames<T>()
        {
            return Enum.GetNames(typeof(T)).Cast<T>();
        }         

        public static string GetName(object value)
        { 
            return Enum.GetName(value.GetType(), value);
        }

        public static string GetEnumItemDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes.FirstOrDefault().Description;
            else
                return value.ToString();
        }

        public static List<EnumModelInt> GetEnumListIntValue<TEnum>()
        {
            List<EnumModelInt> c1 = new List<EnumModelInt>();
            Enum.GetValues(typeof(TEnum)).OfType<TEnum>().ToList()
                .ForEach(s =>
                {
                    Enum v = (Enum)Enum.Parse(typeof(TEnum), s.ToString());
                    c1.Add(new EnumModelInt() { Value =  (int) Convert.ChangeType(s, typeof(int)), Name = GetEnumItemDescription(v) });
                }
                );
            return c1;
        }

        public static List<EnumModelString> GetEnumListStringValue<TEnum>()
        {
            List<EnumModelString> c1 = new List<EnumModelString>();
            Enum.GetValues(typeof(TEnum)).OfType<TEnum>().ToList()
                .ForEach(s =>
                {
                    Enum v = (Enum)Enum.Parse(typeof(TEnum), s.ToString());
                    c1.Add(new EnumModelString() { Value = v, Name = GetEnumItemDescription(v) });
                }
                );
            return c1;
        }
    }
}
