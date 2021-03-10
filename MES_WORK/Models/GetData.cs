using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MES_WORK.Models;
using System.Web.Mvc;
using System.Data;
using Newtonsoft.Json;
using System.Web.Services.Description;

namespace MES_WORK.Models
{
    public class GetData
    {
        Comm comm = new Comm();
        //WebReference.WmsApi WA = new WebReference.WmsApi();


        /// <summary>
        /// 取得正在停機的資料
        /// </summary>
        /// <returns></returns>
        public string Get_DataCode(string pTable, string pCode)
        {
            string sSql = "select TOP 10 * from " + pTable;
            var dtTmp = comm.Get_DataTable(sSql);
            return DataFieldToStr(dtTmp, pCode);
        }

        /// <summary>
        /// 取得正在停機的資料
        /// </summary>
        /// <returns></returns>
        public string Get_OnStopData(string pTable,string pFieldCode) {
            string sSql = "select * from " + pTable +
                          " where date_e = ''";
            var dtTmp = comm.Get_DataTable(sSql);
            return DataFieldToStr(dtTmp, pFieldCode);
        }




        /// <summary>
        /// 傳入欄位及欄位值轉成json格式
        /// </summary>
        /// <param name="sApiFieldArray"></param>
        /// <param name="sApiValueArray"></param>
        /// <returns></returns>
        #region
        public string DataToJson(string sApiFieldArray,string sApiValueArray) {
            DataTable dtApi = new DataTable();

            for (int i = 0; i < sApiFieldArray.Split(',').Length; i++)
            {
                string sApiField = sApiFieldArray.Split(',')[i];
                dtApi.Columns.Add(sApiField);
            }
            DataRow Row = dtApi.NewRow();
            for (int i = 0; i < sApiFieldArray.Split(',').Length; i++)
            {
                string sApiField = sApiFieldArray.Split(',')[i];
                string sApiValue = sApiValueArray.Split(',')[i];
                Row[sApiField] = sApiValue;
            }
            dtApi.Rows.Add(Row);
            string JsonApi = JsonConvert.SerializeObject(dtApi);
            JsonApi = JsonApi.Replace("[", "").Replace("]", "");
            return JsonApi;
        }

        public string DataToJson(string sApiFieldArray, FormCollection form)
        {
            DataTable dtApi = new DataTable();

            for (int i = 0; i < sApiFieldArray.Split(',').Length; i++)
            {
                string sApiField = sApiFieldArray.Split(',')[i];
                dtApi.Columns.Add(sApiField);
            }
            DataRow Row = dtApi.NewRow();
            for (int i = 0; i < sApiFieldArray.Split(',').Length; i++)
            {
                string sApiField = sApiFieldArray.Split(',')[i];
                if (!string.IsNullOrEmpty(form[sApiField]))
                {
                    Row[sApiField] = form[sApiField];
                }
                else
                {
                    Row[sApiField] = "";
                }                
            }
            dtApi.Rows.Add(Row);
            string JsonApi = JsonConvert.SerializeObject(dtApi);
            JsonApi = JsonApi.Replace("[", "").Replace("]", "");
            return JsonApi;
        }
        #endregion

        
        /// <summary>
        /// 取得使用者可使用的電子表單
        /// </summary>
        /// <param name="pUsrCode"></param>
        /// <returns></returns>
        public string Get_UsrEpbArray(string pUsrCode) {
            //取得電子表單權限
            string sSql = "select * from BDP09_0200 " +
                          " where usr_code = '" + pUsrCode + "'" +
                          "   and is_use = 'Y'";
            string sEpbCodeArray = DataFieldToStr(sSql, "epb_code");
            return sEpbCodeArray;
        }



        /// <summary>
        /// 取得表單欄位
        /// </summary>
        /// <param name="Key">表單代號</param>
        /// <returns></returns>
        public string Get_EpbField(string Key)
        {
            string sSql = "select * from WRK02_0100 " +
                          " where epb_code = @epb_code" +
                          "  order by scr_no ";
            var dtTmp = comm.Get_DataTable(sSql, "epb_code", Key);
            return DataFieldToStr(dtTmp, "wrk02_0100");
        }



        /// <summary>
        /// 取得使用者可使用的電子表單的類別
        /// </summary>
        /// <param name="pUsrCode"></param>
        /// <returns></returns>
        public string Get_EpbCanUseType(string pUsrCode)
        {
            string sValue = "";
            string sSql = "select epb_type_code from EPB02_0000" +
                          " where epb_code in (" + StrArrayToSql(Get_UsrEpbArray(pUsrCode))  + ")" +
                          "  group by epb_type_code";
            sValue = DataFieldToStr(sSql, "epb_type_code");
            return sValue;
        }



        /// <summary>
        /// 檢查使用者在該表單裡面是否有指定權限代號
        /// </summary>
        /// <param name="pUsrCode">使用者</param>
        /// <param name="pEpbCode">表單代號</param>
        /// <param name="pLimitCode">權限代號</param>
        /// <returns></returns>
        public bool Chk_UsrEpbLimit(string pUsrCode, string pEpbCode, string pLimitCode)
        {
            bool sValue = false;
            string sSql = "select * from BDP09_0200 " +
                          " where usr_code = '" + pUsrCode + "'" +
                          "   and epb_code = '" + pEpbCode + "'";
            var dtTmp = comm.Get_DataTable(sSql);
            if (dtTmp.Rows.Count > 0)
            {
                string sLimitStr = dtTmp.Rows[0]["limit_str"].ToString();
                if (sLimitStr.Contains(pLimitCode)) { sValue = true; }
            }
            return sValue;
        }





        /// <summary>
        /// 依照輸入型態，分類where語法
        /// </summary>
        /// <param name="form"></param>
        /// <param name="CtrType"></param>
        /// <param name="sField"></param>
        /// <returns></returns>
        public string Sort_WhereType(FormCollection form, string CtrType, string sField)
        {
            string sValue = "";
            string sWhereStr = "";
            if (!string.IsNullOrEmpty(form[sField]))
                switch (CtrType)
                {
                    case "A":
                        //區間
                        if (form[sField].Split(',')[0] != "" && form[sField].Split(',')[1] != "")
                        {
                            sValue += sWhereStr + " and " + sField + " between '" + form[sField].Split(',')[0] + "' and '" + form[sField].Split(',')[1] + "'";
                        }
                        break;
                    case "T":
                        //Textbox                            
                        sValue += sWhereStr + " and " + sField + " like '" + form[sField] + "'";
                        break;
                    case "S":
                        //下拉                           
                        sValue += sWhereStr + " and " + sField + " = '" + form[sField] + "'";
                        break;
                    case "D":
                        //日期                          
                        if (form[sField].Split(',')[0] != "" && form[sField].Split(',')[1] != "")
                        {
                            sValue += sWhereStr + " and " + sField + " between '" + form[sField].Split(',')[0] + "' and '" + form[sField].Split(',')[1] + "'";
                        }
                        break;
                    case "M":
                        //複選下拉                            
                        sValue += sWhereStr + " and " + sField + " in (" + StrArrayToSql(form[sField]) + ")";
                        break;
                }
            return sValue;
        }



        /// <summary>
        /// 取得下拉選單
        /// </summary>
        /// <param name="pSelectCode">下拉選單代號</param>
        /// <returns></returns>
        public string Get_DDLData(string pSelectCode)
        {
            string sValue = "";
            string sSql_str = "";
            string sSql = "select * from BDP31_0000 " +
                          " where select_code = '"+ pSelectCode + "'";
            var dtTmp = comm.Get_DataTable(sSql);
            if (dtTmp.Rows.Count > 0) {
                sSql_str = dtTmp.Rows[0]["tsql_select"].ToString() + " " + dtTmp.Rows[0]["tsql_where"].ToString() + " " + dtTmp.Rows[0]["tsql_order"].ToString();
                dtTmp = comm.Get_DataTable(sSql_str);
                for (int i = 0; i < dtTmp.Rows.Count;i++ ) {
                    if (i > 0) { sValue += ","; }

                    for (int u = 0; u < dtTmp.Columns.Count; u++)
                    {
                        if (u > 0) { sValue += "-"; }
                        string ColName = dtTmp.Columns[u].ColumnName;
                        sValue += dtTmp.Rows[i][ColName].ToString();
                    }
                }
            }            
            return sValue;
        }

        /// <summary>
        /// 使","逗號分隔的字串轉換成Sql的單引號語法
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public string StrArrayToSql(string pString) {
            string sValue = "";
            if (!string.IsNullOrEmpty(pString))
            {
                for (int i = 0; i < pString.Split(',').Length; i++)
                {
                    if (i > 0) { sValue += ","; }
                    sValue += "'" + pString.Split(',')[i] + "'";
                }
            }
            else
            {
                sValue = "''";
            }           
            return sValue;
        }


        /// <summary>
        /// 表單預設值
        /// </summary>
        /// <param name="pCtrType">欄位型態</param>
        /// <param name="pValue">預設值</param>
        /// <returns></returns>
        public string Default_Value(string pCtrType, string pValue)
        {
            string sValue = "";
            switch (pCtrType)
            {
                case "T":
                    sValue = pValue;
                    break;
                case "D":
                    switch (pValue)
                    {
                        case "NOW":
                            sValue = DateTime.Now.ToString("yyyy/MM/dd");
                            break;
                        default:
                            sValue = pValue;
                            break;
                    }
                    break;
            }
            return sValue;
        }


        /// <summary>
        /// 利用SQL語法取得指定欄位(","逗號分隔)的字串形式
        /// </summary>
        /// <param name="pSqlStr">Sql語法</param>
        /// <param name="pFieldCode">欄位的","逗號分隔字串</param>
        /// <returns></returns>
        public string DataFieldToSTA(string pSqlStr, string pFieldCode,string Split = ",")
        {
            var dtTmp = comm.Get_DataTable(pSqlStr);
            string sValue = "";
            for (int i = 0; i < dtTmp.Rows.Count; i++)
            {
                if (i > 0) { sValue += Split; };
                for (int u = 0; u < pFieldCode.Split(',').Length; u++)
                {
                    string sField = pFieldCode.Split(',')[u];
                    if (u > 0) { sValue += "|"; }
                    sValue += dtTmp.Rows[i][sField].ToString();
                }
            }
            return sValue;
        }

        public string DataFieldToSTA(DataTable dtTmp, string pFieldCode, string Split = ",")
        {
            string sValue = "";
            for (int i = 0; i < dtTmp.Rows.Count; i++)
            {
                if (i > 0) { sValue += Split; };
                for (int u = 0; u < pFieldCode.Split(',').Length; u++)
                {
                    string sField = pFieldCode.Split(',')[u];
                    if (u > 0) { sValue += "|"; }
                    sValue += dtTmp.Rows[i][sField].ToString();
                }
            }
            return sValue;
        }



        /// <summary>
        /// 利用SQL語法取得指定欄位的字串形式
        /// </summary>
        /// <param name="pFieldCode">指定欄位</param>
        /// <returns></returns>
        public string DataFieldToStr(string pSqlStr, string pFieldCode)
        {
            var dtTmp = comm.Get_DataTable(pSqlStr);
            string sValue = "";
            for (int i = 0; i < dtTmp.Rows.Count; i++)
            {
                if (i > 0) { sValue += ","; };
                sValue += dtTmp.Rows[i][pFieldCode].ToString();
            }
            return sValue;
        }

        public string DataFieldToStr(DataTable dtTmp, string pFieldCode)
        {
            string sValue = "";
            for (int i = 0; i < dtTmp.Rows.Count; i++)
            {
                if (i > 0) { sValue += ","; };
                sValue += dtTmp.Rows[i][pFieldCode].ToString();
            }
            return sValue;
        }


        public string Get_DutName(string pUsrCode) {
            string sDutCode = comm.Get_QueryData("BDP08_0000", pUsrCode, "usr_code", "dut_code");
            return comm.Get_QueryData("BDP11_0000", sDutCode, "dut_code", "dut_name");
        }

        /// <summary>
        /// 取得資料
        /// </summary>
        /// <param name="pTableCode">資料表</param>
        /// <param name="pKeyValue">索引值</param>
        /// <param name="pKeyCode">索引鍵</param>
        /// <param name="pFieldValue">欄位資料</param>
        /// <returns></returns>
        public string Get_Data(string pTableCode, string pKeyValue, string pKeyCode, string pFieldValue)
        {
            //串SQL字串
            string sSql = "select " + pFieldValue + " from " + pTableCode + " where " + pKeyCode + " = @" + pKeyCode + "";
            DataTable dtTmp = comm.Get_DataTable(sSql, pKeyCode, pKeyValue);
            return DataFieldToStr(dtTmp, pFieldValue);
        }

        public string Get_DataByArray(string pTableCode, string pKeyArray, string pKeyCode, string pFieldValue)
        {
            //串SQL字串
            string sSql = "select " + pFieldValue + " from " + pTableCode + " where " + pKeyCode + " in (" + StrArrayToSql(pKeyArray) + ")";
            DataTable dtTmp = comm.Get_DataTable(sSql);
            return DataFieldToStr(dtTmp, pFieldValue);
        }

    }
}