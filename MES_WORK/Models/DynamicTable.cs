using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MES_WORK.Models
{
    public class DynamicTable
    {
        Comm comm = new Comm();
        GetData GD = new GetData();
        CheckData CD = new CheckData();
        //取得資料表的鍵值欄位
        public string Get_Table_PKField(string pTable)
        {
            string sSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = '" + pTable + "'";
            return comm.DataFieldToStr(sSql, "COLUMN_NAME");
        }


        //取得資料表的欄位
        public string Get_TableField(string pTable, string pFieldType = "COLUMN_NAME")
        {
            string sSql = "SELECT COLUMN_NAME,DATA_TYPE,CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + pTable + "'";
            //COLUMN_NAME 欄位名稱
            //DATA_TYPE 欄位型態
            //CHARACTER_MAXIMUM_LENGTH 欄位長度
            return comm.DataFieldToStr(sSql, pFieldType);
        }



        //取得Sql語法資料表的所有欄位
        public string Get_SqlField(string pSql,string pColumnTools = "ColumnName")
        {
            var dtTmp = comm.Get_DataTable(pSql);
            string sValue = "";
            for (int i = 0; i < dtTmp.Columns.Count; i++)
            {
                if (i > 0) { sValue += ","; }
                switch (pColumnTools) {
                    case "ColumnName":
                        sValue += dtTmp.Columns[i].ColumnName;
                        break;
                    case "DataType":
                        sValue += dtTmp.Columns[i].DataType.ToString();
                        break;
                    case "AutoIncrement":
                        sValue += dtTmp.Columns[i].AutoIncrement.ToString();
                        break;
                    case "DefaultValue":
                        sValue += dtTmp.Columns[i].DefaultValue.ToString();
                        break;
                }
                
            }
            return sValue;
        }


        public string Get_SqlField(DataTable pDataTable, string pColumnTools = "ColumnName")
        {
            var dtTmp = pDataTable;
            string sValue = "";
            for (int i = 0; i < dtTmp.Columns.Count; i++)
            {
                if (i > 0) { sValue += ","; }
                switch (pColumnTools)
                {
                    case "ColumnName":
                        sValue += dtTmp.Columns[i].ColumnName;
                        break;
                    case "DataType":
                        sValue += dtTmp.Columns[i].DataType.ToString();
                        break;
                    case "AutoIncrement":
                        sValue += dtTmp.Columns[i].AutoIncrement.ToString();
                        break;
                    case "DefaultValue":
                        sValue += dtTmp.Columns[i].DefaultValue.ToString();
                        break;
                }

            }
            return sValue;
        }

        /// <summary>
        /// 取得資料表內 自動增值的欄位
        /// </summary>
        /// <param name="pTable"></param>
        /// <returns></returns>
        public string Get_Identity(string pTable) {
            string sValue = "";
            string sSql = "select COLUMN_NAME " +
                          "  From INFORMATION_SCHEMA.COLUMNS " +
                          " where columnproperty(object_id('"+ pTable + "'), column_name,'IsIdentity')=1" +
                          "  group by COLUMN_NAME";
            var dtTmp = comm.Get_DataTable(sSql);
            if (dtTmp.Rows.Count > 0) {
                sValue = dtTmp.Rows[0]["COLUMN_NAME"].ToString();
            }
            return sValue;
        }


        /// <summary>
        /// 指定欄位新增資料
        /// </summary>
        /// <param name="data"></param>
        public void InsertData(string pTable, object data)
        {
            string sSql = "select * from " + pTable;
            string sTableField = Get_SqlField(sSql); //欄位
            string sTableType = Get_TableField(pTable, "DATA_TYPE"); //欄位型態
            string Identity = Get_Identity(pTable); //是否為自動增值
            string sNewField = "";
            string CTableField = "";
            int Cnt = 0;
            //data 有塞值得才寫入DB，沒有的則待預設值

            for (int i = 0; i < sTableField.Split(',').Length; i++)
            {
                string sField = sTableField.Split(',')[i];

                if (!Identity.Contains(sField))
                {
                    //檢查data是否有指定欄位                                            
                    if (data.GetType().GetProperty(sField) != null)
                    {
                        if (Cnt > 0)
                        {
                            sNewField += ",";
                            CTableField += ",";
                        }
                        sNewField += sField;
                        CTableField += "@" + sField;
                        Cnt += 1;
                    }
                    else
                    {
                        //如果沒有，則先判斷欄位能不能null，如果可以就不理他
                        if (!Chk_FieldCanNull(pTable, sField))
                        {
                            if (Cnt > 0)
                            {
                                sNewField += ",";
                                CTableField += ",";
                            }
                            //不能null則直接帶預設值
                            string sDataType = sTableType.Split(',')[i];
                            sNewField += sField;
                            CTableField += "'" + Set_DefaultValue(sDataType) + "'";
                            Cnt += 1;
                        }
                    }
                }
            }
            sSql = "INSERT INTO " +
                   pTable + "  ( " + sNewField + " ) " +
                   "    VALUES (" + CTableField + " ) ";
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, data);
            }
        }



        //一對一的新增語法
        public void Dynamic_Insert(FormCollection form,string Table)
        {
            string sValue = "";
            string sDefault = "";
            string sTableField = Get_TableField(Table);
            string sNewField = "";
            string CTableField = "";
            string Identity = Get_Identity(Table);
            int Cnt = 0;

            for (int i = 0; i < sTableField.Split(',').Length; i++) {
                string sField = sTableField.Split(',')[i];
                string sDataType = Get_TableField(Table, "DATA_TYPE").Split(',')[i];

                if (sField != Identity)
                {
                    if (Cnt > 0)
                    {
                        sNewField += ",";
                        CTableField += ",";
                    }
                    sNewField += sField;
                    CTableField += "@" + sField;
                    Cnt += 1;

                    if (!string.IsNullOrEmpty(form[sField]))
                    {
                        sValue += form[sField];
                    }
                    else
                    {
                        switch (sDataType)
                        {
                            case "nvarchar":
                                sDefault = "";
                                break;
                            case "decimal":
                                sDefault = "0";
                                break;
                            case "int":
                                sDefault = "0";
                                break;
                            default:
                                sDefault = "";
                                break;
                        }
                        sValue += sDefault;
                    }
                }                
            }
            string sSql = "INSERT INTO " +
                          " "+ Table + " ("+ sNewField + ") " +
                          "     VALUES ("+ CTableField + ") ";
            comm.Connect_DB(sSql, sTableField, sValue);          
        }

        public string TableDataToJson(string pSqlStr)
        {
            string sData = "";
            var dtTmp = comm.Get_DataTable(pSqlStr);
            sData = JsonConvert.SerializeObject(dtTmp, Formatting.Indented);
            return sData;
        }


        /// <summary>
        /// 設定預設值
        /// </summary>
        /// <param name="pDataType"></param>
        /// <returns></returns>
        public string Set_DefaultValue(string pDataType)
        {
            string val = "";
            switch (pDataType)
            {
                case "nvarchar":
                    val = "";
                    break;
                case "decimal":
                    val = "0";
                    break;
                case "int":
                    val = "0";
                    break;
                case "datetime":
                    val = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    break;
                default:
                    val = "";
                    break;
            }
            return val;
        }

        /// <summary>
        /// 檢查欄位是否可以Null
        /// </summary>
        /// <param name="pTable"></param>
        /// <param name="pField"></param>
        /// <returns></returns>
        public bool Chk_FieldCanNull(string pTable, string pField)
        {
            bool val = false;
            string sSql = "SELECT * FROM INFORMATION_SCHEMA.Columns Where Table_Name = @Table_Name and COLUMN_NAME = @COLUMN_NAME";
            var dtTmp = comm.Get_DataTable(sSql, "Table_Name,COLUMN_NAME", pTable + "," + pField);
            if (dtTmp.Rows.Count > 0)
            {
                switch (dtTmp.Rows[0]["IS_NULLABLE"].ToString())
                {
                    case "YES":
                        val = true;
                        break;
                    case "NO":
                        val = false;
                        break;
                }
            }
            return val;
        }



    }
}