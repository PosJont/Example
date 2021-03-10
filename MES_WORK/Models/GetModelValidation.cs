using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using System.Reflection;
using System.ComponentModel;
using System.Web.Mvc;

using System.Web.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MES_WORK.Models
{
    public class GetModelValidation
    {   
        /// <summary>
        /// 取得Models的欄位名稱
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="hasNotMapped"></param>
        /// <returns></returns>                    
        public string Get_ModelCode<T>(T obj, bool hasNotMapped = false)
        {
            string sFieldCode = "";
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (var info in properties)
            {
                if (!hasNotMapped)
                {
                    object[] attributes = info.GetCustomAttributes(typeof(NotMappedAttribute), false);
                    if (attributes == null || attributes.Length <= 0)
                    {
                        if (sFieldCode != "") { sFieldCode += ","; }
                        sFieldCode += info.Name;
                    }
                }
                else
                {
                    if (sFieldCode != "") { sFieldCode += ","; }
                    sFieldCode += info.Name;
                }
            }
            return sFieldCode;
        }

        /// <summary>
        /// 取得類別中DisplayName值的清單
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="hasNotMapped">是否抓取設有NotMapped的欄位，預設false不抓</param>
        /// <returns></returns>
        public List<string> Get_DisplayNames<T>(T obj, bool hasNotMapped = false)
        {
            List<string> list = new List<string>();
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (var info in properties)
            {
                if (!hasNotMapped)
                {
                    object[] attributes = info.GetCustomAttributes(typeof(NotMappedAttribute), false);
                    if (attributes == null || attributes.Length <= 0)
                    {
                        list.Add(GetDisplayName(info));
                    }
                }
                else
                {
                    list.Add(GetDisplayName(info));
                }
            }
            return list;
        }
        public string GetDisplayName(PropertyInfo info)
        {
            object[] attributes = info.GetCustomAttributes(typeof(DisplayNameAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                var displayName = (DisplayNameAttribute)attributes[0];
                return displayName.DisplayName;
            }
            return "No DisplayName!";
        }
        public StringLengthAttribute GetStringLength(PropertyInfo info)
        {
            object[] attributes = info.GetCustomAttributes(typeof(StringLengthAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                var stringlength = (StringLengthAttribute)attributes[0];
                return stringlength;
            }
            return new StringLengthAttribute(1);
        }
        public RequiredAttribute GetRequired(PropertyInfo info)
        {
            object[] attributes = info.GetCustomAttributes(typeof(RequiredAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                var required = (RequiredAttribute)attributes[0];
                return required;
            }
            return new RequiredAttribute() { AllowEmptyStrings = true };
        }
        public string GetDataType(PropertyInfo info)
        {
            object[] attributes = info.GetCustomAttributes(typeof(DataTypeAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                var datatype = (DataTypeAttribute)attributes[0];
                return datatype.DataType.ToString();
            }
            return "No GetDataType";
        }
        public string GetNotMapped(PropertyInfo info)
        {
            object[] attributes = info.GetCustomAttributes(typeof(NotMappedAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                var notmapped = (NotMappedAttribute)attributes[0];
                return "";
            }
            return "No NotMapped";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="b">決定要不要抓虛欄位，預設false不抓</param>
        /// <returns></returns>
        public List<string> GetColNames<T>(T obj, bool b = false)
        {

            List<string> list = new List<string>();
            Type type = obj.GetType();
            var properties = type.GetProperties();
            foreach (var info in properties)
            {
                if (b)
                {
                    list.Add(info.Name);
                }
                else
                {
                    object[] attributes = info.GetCustomAttributes(typeof(NotMappedAttribute), false);
                    if (attributes == null || attributes.Length <= 0)
                    {
                        list.Add(info.Name);
                    }
                }
            }
            return list;
        }

        public string GetColNameToStr<T>(T obj, bool b = false)
        {
            string ColName = "";
            int Cnt = 0;        
            Type type = obj.GetType();
            var properties = type.GetProperties();
            foreach (var info in properties)
            {              
                if (b)
                {
                    if (Cnt > 0) { ColName += ","; }
                    ColName += info.Name;
                }
                else
                {
                    object[] attributes = info.GetCustomAttributes(typeof(NotMappedAttribute), false);
                    if (attributes == null || attributes.Length <= 0)
                    {
                        if (Cnt > 0) { ColName += ","; }
                        ColName += info.Name;
                    }
                }
                Cnt += 1;
            }
            return ColName;
        }





        public string GetKey<T>(T obj)
        {
            var key = obj.GetType().GetProperties().FirstOrDefault(prop => prop.IsDefined(typeof(KeyAttribute), false));
            return key.Name;
        }
    }
}