using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using System.Reflection;
using System.Web.Mvc;

using System.Web.Helpers;
using MVC.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Dapper;
using System.Data.Objects.SqlClient;
namespace MES_WORK.Models
{
    //程式固定 向下//
    public static class Utils
    {
        /// <summary>
        /// 判斷IEnumerable是否非null
        /// </summary>
        public static bool IsAny<T>(this IEnumerable<T> data)
        {
            return data != null && data.Any();
        }
    }
    //程式固定 向上//
    public class Comm
    {
        /// <summary>
        /// 取得資料庫連線字串
        /// </summary>
        /// <returns>資料庫連線字串</returns>
        public string Get_ConnStr()
        {
            string connStr = WebConfigurationManager.ConnectionStrings["con_db"].ConnectionString;
            return connStr;
        }

        /// <summary>
        /// 與資料庫做連線
        /// </summary>
        /// <returns>回傳一個SqlConnection的物件</returns>
        public SqlConnection Set_DBConnection()
        {
            SqlConnection Connection_Db;
            Connection_Db = new SqlConnection(Get_ConnStr());
            Connection_Db.Open();
            return Connection_Db;
        }

        /// <summary>
        /// 傳入一個SQL語法，回傳一個DataTable
        /// </summary>
        /// <param name="pSql">Select語法</param>
        /// <returns></returns>
        public DataTable Get_DataTable(string pSql)
        {
            DataTable datatable = new DataTable();
            try
            {
                if (pSql.Length > 0)
                {
                    using (SqlConnection con_db = Set_DBConnection())
                    {
                        SqlDataAdapter Adapter = new SqlDataAdapter(pSql, con_db);
                        Adapter.Fill(datatable);
                    }
                }
                return datatable;
            }
            catch (Exception)
            {
                //錯誤處理
                throw;
            }
        }


        /// <summary>
        /// 傳入一個SQL語法，自定義傳入參數，回傳一個DataTable
        /// </summary>
        /// <param name="pSql">Select語法</param>
        /// <returns></returns>
        public DataTable Get_DataTable(string pSql, string pPara = "", string pParaValue = "")
        {
            DataTable datatable = new DataTable();
            try
            {
                if (pSql.Length > 0)
                {
                    using (SqlConnection con_db = Set_DBConnection())
                    {
                        SqlCommand sqlCommand = new SqlCommand(pSql);
                        sqlCommand.Connection = con_db;
                        for (int i = 0; i < pPara.Split(',').Length; i++)
                        {
                            sqlCommand.Parameters.Add(new SqlParameter("@" + pPara.Split(',')[i], pParaValue.Split(',')[i]));
                        }
                        SqlDataReader reader = sqlCommand.ExecuteReader();
                        datatable.Load(reader);
                    }
                }
                return datatable;
            }
            catch (Exception)
            {
                //錯誤處理
                throw;
            }
        }


        /// <summary>
        /// 傳入一個SQL的資料表，自定義傳入參數，回傳一個DataTable
        /// </summary>
        /// <param name="sTable">SQL 資料表</param>
        ///  <param name="pOption">選擇方式</param>
        /// <param name="pSearchname">搜尋哪個資料表</param>
        ///  <param name="pSearchValue">搜尋哪個值</param>
        /// <returns></returns>
        public string Get_strSQL(string sTable, string pOption = "", string pSearchname = "", string pSearchValue = "")
        {
            string strSql;
            switch (pOption)
            {
                case "s":
                    strSql = "select * from " + sTable + " where " + pSearchname + " = '" + pSearchValue + "'";
                    break;
                default:
                    strSql = "select * from " + sTable + " where " + pSearchname + " = @" + pSearchValue;
                    break;
            }

            return strSql;
        }




        /// <summary>
        /// 取得單一Table明確1對1鍵值資料共用函數
        /// </summary>
        /// <param name="pTableCode">要查詣的TABLE CODE</param>
        /// <param name="pKeyValue">使用者輸入的鍵值</param>
        /// <param name="pKeyCode">Table中鍵值的欄位名稱</param>
        /// <param name="pFieldValue">要取回的欄位名稱</param>
        /// <returns></returns>
        public string Get_QueryData(string pTableCode, string pKeyValue, string pKeyCode, string pFieldValue)
        {
            //串SQL字串
            string sSql = "select " + pFieldValue + " from " + pTableCode + " where " + pKeyCode + " = @" + pKeyCode;
            using (SqlConnection con_db = Set_DBConnection())
            {
                SqlCommand sqlCommand = new SqlCommand(sSql);
                sqlCommand.Connection = con_db;
                sqlCommand.Parameters.Add(new SqlParameter(pKeyCode, pKeyValue));
                SqlDataReader reader = sqlCommand.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    return reader.GetString(reader.GetOrdinal(pFieldValue));
                }
            }
            return "";
        }
        /// <summary>
        /// Chk_QtyInWMT02
        /// </summary>
        /// <param name="pProCode"></param>
        /// <param name="pLotNo"></param>
        /// <param name="pProQty"></param>
        /// <returns></returns>
        public bool Chk_QtyInWMT02(string pProCode, string pLotNo, string pProQty)
        {
            bool isChk = true;
            double dProQty = pProQty != "" ? double.Parse(pProQty) : 0;
            string sSql = "Select top 1  pro_qty from WMT0200 " + "  where pro_code='" + pProCode.Trim() + "'"
                + "    and lot_no='" + pLotNo.Trim() + "'" + "  order by  ins_date desc  ";
            DataTable dtTmp = Get_DataTable(sSql);

            if (dtTmp.Rows.Count > 0)
            {
                string sProQty = dtTmp.Rows[0]["pro_qty"].ToString();
                double dTmpQty = sProQty != "" ? double.Parse(sProQty) : 0;
                if (dTmpQty < dProQty) isChk = false;
            }
            else isChk = false;
            return isChk;
        }

        /// <summary>
        /// 取得單一Table明確1對1鍵值資料共用函數
        /// </summary>
        /// <param name="pTableCode">要查詣的TABLE CODE</param>
        /// <param name="pKeyValue">使用者輸入的鍵值</param>
        /// <param name="pKeyCode">Table中鍵值的欄位名稱</param>
        /// <param name="pFieldValue">要取回的欄位名稱</param>
        /// <returns></returns>
        public bool Chk_Basic(string pTableCode, string pKeyValue, string pKeyCode, string pFieldWhere = "")
        {
            //串SQL字串
            string sSql = "select " + pKeyCode + " from " + pTableCode + " where " + pKeyCode + " = @" + pKeyCode + " " + pFieldWhere;
            try
            {
                using (SqlConnection con_db = Set_DBConnection())
                {
                    SqlCommand sqlCommand = new SqlCommand(sSql);
                    sqlCommand.Connection = con_db;
                    sqlCommand.Parameters.Add(new SqlParameter(pKeyCode, pKeyValue));
                    SqlDataReader reader = sqlCommand.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }


        public T Get_QueryData<T>(string pTableCode, string pKeyValue, string pKeyCode, string pFieldValue)
        {
            T t = Activator.CreateInstance<T>();
            string sSql = "select " + pFieldValue + " from " + pTableCode + " where " + pKeyCode + " = @" + pKeyCode;
            using (SqlConnection con_db = Set_DBConnection())
            {
                SqlCommand sqlCommand = new SqlCommand(sSql);
                sqlCommand.Connection = con_db;
                sqlCommand.Parameters.Add(new SqlParameter(pKeyCode, pKeyValue));
                SqlDataReader reader = sqlCommand.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    var value = reader.GetValue(reader.GetOrdinal(pFieldValue));
                    return (T)value;

                }
            }
            return t;
        }


        public string Get_QueryData(string pTableCode, string pWhere, string pFieldValue)
        {
            //串SQL字串
            string sSql = "select " + pFieldValue + " from " + pTableCode + "  " + pWhere;

            using (SqlConnection con_db = Set_DBConnection())
            {
                SqlCommand sqlCommand = new SqlCommand(sSql);
                sqlCommand.Connection = con_db;
                SqlDataReader reader = sqlCommand.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    return reader.GetString(reader.GetOrdinal(pFieldValue));
                }
                return "";
            }
        }


        /// <summary>
        /// 取得BDP21_0000的選項名稱內容
        /// </summary>
        /// <param name="pCodeCode">選項代碼</param>
        /// <param name="pFieldValue">欄位代碼</param>
        /// <returns></returns>
        public string Get_BDP21_0000(string pCodeCode, string pFieldValue)
        {
            return "";
        }



        /// <summary>
        /// 有重覆資料則回傳false
        /// </summary>
        /// <param name="pTableCode"></param>
        /// <param name="pWhere"></param>
        /// <returns></returns>
        public Boolean Chk_RelData(string pTableCode, string pWhere)
        {
            //串SQL字串
            string sSql = "select top 1 *  from " + pTableCode + " " + pWhere;
            using (SqlConnection con_db = Set_DBConnection())
            {
                SqlCommand sqlCommand = new SqlCommand(sSql);
                sqlCommand.Connection = con_db;
                SqlDataReader reader = sqlCommand.ExecuteReader();
                if (reader.HasRows)
                {
                    //有重覆資料
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// 檢查登入帳號密碼
        /// </summary>
        /// <param name="usr_code">使用者代號</param>
        /// <param name="usr_pass">使用者密碼(未加密)</param>
        /// <returns></returns>
        public Boolean Chk_Login(string usr_code, string usr_pass)
        {
            //string usr_code = userdata.usr_code;
            //string usr_pass = userdata.usr_pass;

            if (string.IsNullOrEmpty(usr_code))
            {
                return false;
            }
            string sSql = "select * from BDP08_0000 " +
                          " where usr_code=@usr_code " +
                          "   and usr_pass=@usr_pass ";

            using (SqlConnection con_db = Set_DBConnection())
            {
                SqlCommand sqlCommand = new SqlCommand(sSql);
                sqlCommand.Connection = con_db;
                sqlCommand.Parameters.Add(new SqlParameter("@usr_code", usr_code));
                sqlCommand.Parameters.Add(new SqlParameter("@usr_pass", usr_pass));

                SqlDataReader reader = sqlCommand.ExecuteReader();

                if (reader.HasRows)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 取得使用者權限字串
        /// </summary>
        /// <param name="pUsrCode"></param>
        /// <param name="pPrgCode"></param>
        /// <returns></returns>
        public string Get_LimitByUsrCode(string pUsrCode, string pPrgCode)
        {
            string sLimitStr = "";
            string sLimitType = Get_QueryData("BDP08_0000", pUsrCode, "usr_code", "limit_type");

            switch (sLimitType)
            {
                case "A": // BDP09_0000
                    sLimitStr = Get_LimitStr("BDP09_0000", pUsrCode, "usr_code", pPrgCode, "prg_code");
                    break;
                case "B": // BDP09_0100
                    string pGrpCode = Get_QueryData("BDP08_0000", pUsrCode, "usr_code", "grp_code");
                    sLimitStr = Get_LimitStr("BDP09_0100", pGrpCode, "grp_code", pPrgCode, "prg_code");
                    break;
                default:
                    break;
            }
            return sLimitStr;
        }

        /// <summary>
        /// 取得權限字串，傳入Table、使用者和程式相關參數
        /// </summary>
        /// <param name="pTableCode">資料表代號BDP090/BDP091</param>
        /// <param name="pKeyValue">usr_code/grp_code</param>
        /// <param name="pKeyCode">值</param>
        /// <param name="pPrgValue">程式代碼</param>
        /// <returns></returns>
        private string Get_LimitStr(string pTableCode, string pKeyValue, string pKeyCode, string pPrgValue, string pPrgCode)
        {
            string sSql = "select limit_str from " + pTableCode +
                          " where " + pKeyCode + " = @" + pKeyCode + " and " + pPrgCode + " = @" + pPrgCode;
            using (SqlConnection con_db = Set_DBConnection())
            {
                SqlCommand sqlCommand = new SqlCommand(sSql);
                sqlCommand.Connection = con_db;
                sqlCommand.Parameters.Add(new SqlParameter(pKeyCode, pKeyValue));
                sqlCommand.Parameters.Add(new SqlParameter(pPrgCode, pPrgValue));

                SqlDataReader reader = sqlCommand.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    return reader.GetString(reader.GetOrdinal("limit_str"));
                }
            }
            return "";
        }

        /// <summary>
        /// 處理Html值到DB的值轉換的集中函數
        /// </summary>
        /// <param name="pHtmlValue">Html控制項取到的值</param>
        /// <param name="pHtmlType">控制項類型名稱</param>
        /// <returns></returns>
        public string Chg_HtmlToDB(object pHtmlValue, string pHtmlType)
        {

            string sReturn = "";

            switch (pHtmlType)
            {
                case "checkbox": //switch類型的checkbox
                    if (pHtmlValue == null)
                    {
                        sReturn = "N";
                    }
                    else
                    {
                        sReturn = "Y";
                    }
                    break;
                case "textbox": //一般輸入框
                    if (pHtmlValue == null)
                    {
                        sReturn = "";
                    }
                    else
                    {
                        sReturn = pHtmlValue.ToString();
                    }
                    break;
                case "multiselect":
                    sReturn = (pHtmlValue == null ? "" : pHtmlValue.ToString().Replace(",", ""));
                    break;
                default:
                    break;
            }
            return sReturn;
        }

        /// <summary>
        /// 將DB讀出來的值，依HTML控項所需要的值做變更
        /// </summary>
        /// <param name="pDBValue">DB讀出來的值</param>
        /// <param name="pHtmlType">控制項類別</param>
        /// <returns></returns>
        public string Chg_DBToHtml(string pDBValue, string pHtmlType)
        {

            string sReturn = "";

            switch (pHtmlType)
            {
                case "checkbox": //switch類型的checkbox
                    if (pDBValue == "Y")
                    {
                        sReturn = "checked= checked";
                    }
                    break;
                default:
                    break;
            }
            return sReturn;
        }


        /// <summary>
        /// 連結DB 使用SQL語法
        /// </summary>
        /// <param name="pSql">SQL敘述</param>
        /// <param name="pPara">用@的參數</param>
        /// <param name="pParaValue">替代參數的值</param>
        public void Connect_DB(string pSql, string pPara = "", string pParaValue = "")
        {
            using (SqlConnection con_db = Set_DBConnection())
            {
                SqlCommand sqlCommand = new SqlCommand(pSql);

                for (int i = 0; i < pPara.Split(',').Length; i++)
                {
                    sqlCommand.Parameters.Add(new SqlParameter("@" + pPara.Split(',')[i], pParaValue.Split(',')[i]));
                }
                sqlCommand.Connection = con_db;
                sqlCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 取得指定機台正在上工中的人員
        /// </summary>
        /// <param name="pMacCode">機台</param>
        /// <returns></returns>
        public string Get_MacPerCode(string pMacCode)
        {
            string sSql = "select * from MEM01_0000 " +
                          " where mac_code = '" + pMacCode + "'" +
                          "   and status = 'Y' ";
            var dtTmp = Get_DataTable(sSql);

            string sPerCode = "";
            for (int i = 0; i < dtTmp.Rows.Count; i++)
            {
                if (i > 0) { sPerCode += ","; }
                sPerCode += dtTmp.Rows[i]["per_code"].ToString();
            }
            return sPerCode;
        }


        /// <summary>
        /// 檢查指定人員是否在指定機台有上工
        /// </summary>
        /// <param name="pMacCode">機台號</param>
        /// <param name="pPerCode">人員號</param>
        /// <returns></returns>
        public Boolean Chk_MacPerCode(string pMacCode, string pPerCode)
        {
            string sSql = "select * from MEM01_0000 " +
                          " where mac_code = '" + pMacCode + "' " +
                          "   and per_code = '" + pPerCode + "' " +
                          "   and status = 'Y' ";
            var dtTmp = Get_DataTable(sSql);

            if (dtTmp.Rows.Count > 0)
            { return true; }
            else
            { return false; }
        }

        /// <summary>
        /// 利用目前機台取得供應商編號
        /// </summary>
        /// <param name="pMacCode">機台編號</param>
        /// <returns></returns>
        public string Get_SupCode(string pMacCode)
        {
            //string sSql = "select * from CNB01_0000 " +
            //              " where mac_code = '" + pMacCode + "' ";
            //var dtTmp = Get_DataTable(sSql);
            //if (dtTmp.Rows.Count > 0)
            //{ return dtTmp.Rows[0]["sup_code"].ToString(); }
            //else
            //{ return ""; }
            return "";
        }


        /// <summary>
        /// 利用SQL語法取得指定欄位的字串形式
        /// </summary>
        /// <param name="pFieldCode">指定欄位</param>
        /// <returns></returns>
        public string DataFieldToStr(string pSqlStr, string pFieldCode)
        {
            var dtTmp = Get_DataTable(pSqlStr);
            string sValue = "";
            for (int i = 0; i < dtTmp.Rows.Count; i++)
            {
                if (i > 0) { sValue += ","; };
                sValue += dtTmp.Rows[i][pFieldCode].ToString();
            }
            return sValue;
        }

        /// <summary>
        /// 取得製令明細檔的指定欄位
        /// </summary>
        /// <param name="pModCode">機台號</param>
        /// <param name="pFieldCode">欄位</param>
        /// <returns></returns>
        public string Get_ModDetail(string pModCode, string pFieldCode)
        {
            string sSql = "select * from MFT01_0100 " +
                          " where mod_code = '" + pModCode + "' ";
            return DataFieldToStr(sSql, pFieldCode);
        }

        /// <summary>
        /// 取得製令主檔檔的指定欄位
        /// </summary>
        /// <param name="pModCode">機台號</param>
        /// <param name="pFieldCode">欄位</param>
        /// <returns></returns>
        public string Get_ModMain(string pModCode, string pFieldCode)
        {
            string sSql = "select MFT01_0000.* from MFT01_0000 " +
                          "  left join MFT01_0100 on MFT01_0000.mo_code = MFT01_0100.mo_code " +
                          " where MFT01_0100.mod_code = '" + pModCode + "' ";
            return DataFieldToStr(sSql, pFieldCode);
        }


        /// <summary>
        /// 取得良品數與不良品數
        /// </summary>
        /// <param name="pTkCode"></param>
        /// <returns></returns>
        public string Get_QtyAmt(string pModCode)
        {
            Double ok_qty = 0;
            Double ng_qty = 0;
            string sReturn = "0|0";

            string sSql = "select isnull(sum(ok_qty),'0') as ok_sum,isnull(sum(ng_qty),'0') as ng_sum " +
                          "  from MEM02_0100 " +
                          " where mod_code = '" + pModCode + "'";
            var dtTmp = Get_DataTable(sSql);
            ok_qty = Math.Ceiling(Convert.ToDouble(dtTmp.Rows[0]["ok_sum"].ToString()));
            ng_qty = Math.Ceiling(Convert.ToDouble(dtTmp.Rows[0]["ng_sum"].ToString()));
            sReturn = ok_qty.ToString() + "|" + ng_qty.ToString();

            return sReturn;
        }


        /// <summary>
        /// 由連字元分隔的32位數字
        /// </summary>
        /// <returns></returns>
        public string Get_Guid()
        {
            System.Guid guid = new Guid();
            guid = Guid.NewGuid();
            return guid.ToString();
        }


        //public string Get_ModOrder(string pModCode, string pOrderMode)
        //{
        //    string sMoCode = pModCode.Substring(0, 15);
        //    string sOrder = pModCode.Substring(16, 4);
        //    string sNewOrder = sOrder;
        //    string sSql = "select * from MFT01_0100 " +
        //                  " where mo_code = '" + sMoCode + "'";
        //    var dtTmp = Get_DataTable(sSql);

        //    for (int i = 0; i < dtTmp.Rows.Count; i++){
        //        if (dtTmp.Rows[i]["mod_code"].ToString() == pModCode) {
        //            switch (pOrderMode)
        //            {
        //                case "U":
        //                    if (i + 1 < dtTmp.Rows.Count)
        //                    {
        //                        sNewOrder = dtTmp.Rows[i + 1]["TA003"].ToString();
        //                    }
        //                    else {
        //                        sNewOrder = dtTmp.Rows[i]["TA003"].ToString();
        //                    }
        //                    break;
        //                case "D":
        //                    if (i - 1 >= 0)
        //                    {
        //                        sNewOrder = dtTmp.Rows[i - 1]["TA003"].ToString();
        //                    }
        //                    else
        //                    {
        //                        sNewOrder = dtTmp.Rows[i]["TA003"].ToString();
        //                    }
        //                    break;
        //                default:                            
        //                    break;
        //            }
        //        }
        //    }                       
        //    return sMoCode + "-" + sNewOrder;                                                        
        //}

        public string Get_ModOrder(string pModCode, string pOrderMode)
        {
            string sMoCode = pModCode.Substring(0, 15);
            int sOrder = Int32.Parse(pModCode.Substring(16, 4));
            string sNewOrder = "";
            switch (pOrderMode)
            {
                case "U":
                    sNewOrder = (sOrder + 10).ToString("0000");
                    break;
                case "D":
                    if (sOrder - 10 != 0)
                    {
                        sNewOrder = (sOrder - 10).ToString("0000");
                    }
                    else
                    {
                        sNewOrder = "0000";
                    }
                    break;
                default:
                    break;
            }
            return sMoCode + "-" + sNewOrder;
        }

        public string Get_Data(string pTableCode, string pKeyValue, string pKeyCode, string pFieldValue)
        {
            //串SQL字串
            string sSql = "select " + pFieldValue + " from " + pTableCode + " where " + pKeyCode + " = '" + pKeyValue + "'";
            return DataFieldToStr(sSql, pFieldValue);
        }


        public Decimal sGetDecimal(string pValue)
        {
            bool success = false;
            Decimal result = 0;
            success = Decimal.TryParse(pValue, out result);

            return result;
        }

        public Int32 sGetInt32(string pValue)
        {
            bool success = false;
            Int32 result = 0;
            success = Int32.TryParse(pValue, out result);

            return result;
        }

        public Int64 sGetInt64(string pValue)
        {
            bool success = false;
            Int64 result = 0;
            success = Int64.TryParse(pValue, out result);

            return result;
        }

        public String sGetString(string pValue)
        {
            string result = pValue ?? "";
            return result;
        }

        public Double sGetDouble(string pValue)
        {
            bool success = false;
            Double result = 0;
            success = Double.TryParse(pValue, out result);

            return result;
        }

        public float sGetfloat(string pValue)
        {
            bool success = false;
            float result = 0;
            success = float.TryParse(pValue, out result);

            return result;
        }



        public DateTime sGetDateTime(string pValue)
        {
            bool success = false;
            DateTime result = DateTime.Now;
            success = DateTime.TryParse(pValue, out result);

            return result;
        }

        /// <summary>
        /// 給傳進來的model設值
        /// </summary>
        /// <typeparam name="T">model類別</typeparam>
        /// <param name="obj">要設值的model</param>
        /// <param name="form">從html傳來的form資料</param>
        public void Set_ModelDefaultValue<T>(T obj)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                switch (property.PropertyType.Name.ToLower())
                {
                    case "string":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetString(""));
                        break;
                    case "int":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetInt32("0"));
                        break;
                    case "int32":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetInt32("0"));
                        break;
                    case "int64":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetInt64("0"));
                        break;
                    case "double":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetDouble("0"));
                        break;
                    case "decimal":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetDecimal("0"));
                        break;
                    case "datetime":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetDateTime("yyyy/MM/DD"));
                        break;
                    default:
                        break;
                };
            }
        }

        /// <summary>
        /// 給傳進來的model設值
        /// </summary>
        /// <typeparam name="T">model類別</typeparam>
        /// <param name="obj">要設值的model</param>
        /// <param name="form">從html傳來的form資料</param>
        public void Set_ModelValue<T>(T obj, FormCollection form)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                switch (property.PropertyType.Name.ToLower())
                {
                    case "string":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetString(form[property.Name]));
                        break;
                    case "int":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetInt32(form[property.Name]));
                        break;
                    case "int32":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetInt32(form[property.Name]));
                        break;
                    case "int64":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetInt64(form[property.Name]));
                        break;
                    case "double":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetDouble(form[property.Name]));
                        break;
                    case "decimal":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetDecimal(form[property.Name]));
                        break;
                    case "datetime":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetDateTime(form[property.Name]));
                        break;
                    default:
                        break;
                };
            }
        }

        /// <summary>
        /// 給傳進來的model設值
        /// </summary>
        /// <typeparam name="T">model類別</typeparam>
        /// <param name="obj">要設值的model</param>
        /// <param name="form">從html傳來的form資料</param>
        public void Set_ModelValue<T>(T obj, JArray jObject)
        {

            foreach (var property in obj.GetType().GetProperties())
            {
                switch (property.PropertyType.Name.ToLower())
                {
                    case "string":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetString(jObject[property.Name].ToString()));
                        break;
                    case "int":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetInt32(jObject[property.Name].ToString()));
                        break;
                    case "int32":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetInt32(jObject[property.Name].ToString()));
                        break;
                    case "int64":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetInt64(jObject[property.Name].ToString()));
                        break;
                    case "double":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetDouble(jObject[property.Name].ToString()));
                        break;
                    case "decimal":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetDecimal(jObject[property.Name].ToString()));
                        break;
                    case "datetime":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetDateTime(jObject[property.Name].ToString()));
                        break;
                    default:
                        break;
                };

            }
        }

        /// <summary>
        /// 給傳進來的model設值
        /// </summary>
        /// <typeparam name="T">model類別</typeparam>
        /// <param name="obj">要設值的model</param>
        /// <param name="form">從html傳來的form資料</param>
        public void Set_ModelValue<T>(T obj, JObject jObject)
        {

            foreach (var property in obj.GetType().GetProperties())
            {
                switch (property.PropertyType.Name.ToLower())
                {
                    case "string":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetString(jObject[property.Name].ToString()));
                        break;
                    case "int":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetInt32(jObject[property.Name].ToString()));
                        break;
                    case "int32":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetInt32(jObject[property.Name].ToString()));
                        break;
                    case "int64":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetInt64(jObject[property.Name].ToString()));
                        break;
                    case "double":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetDouble(jObject[property.Name].ToString()));
                        break;
                    case "decimal":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetDecimal(jObject[property.Name].ToString()));
                        break;
                    case "datetime":
                        obj.GetType().GetProperty(property.Name).SetValue(obj, sGetDateTime(jObject[property.Name].ToString()));
                        break;
                    default:
                        break;
                };

            }
        }

        /// <summary>
        /// 取得Sever端的時間
        /// </summary>
        /// <param name="pType">yyyy/MM/dd, HH:mm:ss,yyyy/MM/dd HH:mm:ss</param>
        /// <returns></returns>
        public string Get_ServerDateTime(string pType)
        {
            string sSql = "";
            DataTable dtTmp;
            sSql += "  SELECT GETDATE() as DATETIME";
            dtTmp = Get_DataTable(sSql);
            if (dtTmp.Rows.Count > 0)
            {
                DateTime dt = DateTime.Parse(dtTmp.Rows[0]["DATETIME"].ToString());
                return dt.ToString(pType);
            }
            return "";
            //switch (pType)
            //{
            //    case "yyyy/MM/dd":
            //        return String.Format("yyyy/MM/dd", dtTmp.Rows[0]["DATETIME"].ToString());

            //    case "HH:mm:ss":
            //        return String.Format("HH:mm:ss", dtTmp.Rows[0]["DATETIME"].ToString());

            //    case "yyyy/MM/dd HH:mm:ss":
            //        return String.Format("yyyy/MM/dd HH:mm:ss", dtTmp.Rows[0]["DATETIME"].ToString());

            //    default:
            //        return "";
            //}

        }
        /// <summary>
        /// 檢查該物料是否已被使用
        /// </summary>
        /// <param name="pWrkCode"></param>
        /// <returns></returns>
        private bool Chk_MEM01OkQty(string pWrkCode)
        {
            string sSql = "";

            return true;
        }

        /// <summary>
        /// 檢查MEM05_0000 是否有資料
        /// </summary>
        /// <param name="pMacCode"></param>
        /// <param name="pMoCode"></param>
        /// <param name="pProCode"></param>
        /// <param name="pLotNo"></param>
        /// <returns></returns>
        public bool Chk_MEM05Data(string pMacCode, string pMoCode, string pProCode, string pLotNo)
        {
            string sSql = "";
            DataTable dtTmp;
            sSql = "select * from MEM05_0000 " + "  where  mac_code ='" + pMacCode + "'" + "    and  mo_code ='" + pMoCode + "'" + "    and  pro_code ='" + pProCode + "'" + "    and  lot_no='" + pLotNo + "'";
            dtTmp = Get_DataTable(sSql);
            return dtTmp.Rows.Count <= 0;
        }

        /// <summary>
        /// 確認MEM01中是否有資料，若有則回傳False，沒有回傳True
        /// </summary>
        /// <param name="pWorkCode"></param>
        /// <param name="pMoCode"></param>
        /// <param name="pStationCode"></param>
        /// <param name="pMacCode"></param>
        /// <returns></returns>
        public bool Chk_MEM01Data(string pWorkCode, string pMoCode, string pStationCode, string pMacCode)
        {
            string sSql = "";
            DataTable dtTmp;
            sSql = "select * from MEM01_0000 " + " where work_code='" + pWorkCode + "'" + "   and mo_code='" + pMoCode + "'" + "   and station_code='" + pStationCode + "'" + "   and mac_code ='" + pMacCode + "'";
            dtTmp = Get_DataTable(sSql);
            return dtTmp.Rows.Count <= 0;
        }

        /// <summary>
        /// 傳入一派工單號，檢查是否已完成上料，在上料檢查檔中需要的物料都有上料了才算有上，不考慮量是否滿足
        /// </summary>
        /// <param name="pWrkCode"></param>
        /// <returns></returns>
        public bool Chk_WrkCodeIsReady(string pWrkCode)
        {
            string sSql = "";
            DataTable dtFun;
            sSql = "select * from WMT07_0000 where res_qty=0 and wrk_code='" + pWrkCode + "'";
            dtFun = Get_DataTable(sSql);
            return dtFun.Rows.Count <= 0;
        }

        /// <summary>
        /// 檢查時間
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pSpilt"></param>
        /// <param name="pType"></param>
        /// <returns></returns>
        public bool Chk_DateForm(string pDate, char pSpilt = '/', string pType = "Y")
        {

            if (pType == "Y")
            {
                if (pDate.Length != 10)
                    return false;
                string[] tmpSpilt = pDate.Split(pSpilt);
                if (tmpSpilt.Length != 3)
                    return false;
                if (IsNumeric(tmpSpilt[0]) == false | IsNumeric(tmpSpilt[1]) == false | IsNumeric(tmpSpilt[2]) == false)
                    return false;
                if (System.Convert.ToInt32(tmpSpilt[0]) < 1000 | System.Convert.ToInt32(tmpSpilt[0]) > 3000)
                    return false;
                if (System.Convert.ToInt32(tmpSpilt[1]) < 1 | System.Convert.ToInt32(tmpSpilt[1]) > 12)
                    return false;
                if (System.Convert.ToInt32(tmpSpilt[2]) < 1 | System.Convert.ToInt32(tmpSpilt[2]) > 31)
                    return false;
                try { DateTime.Parse(tmpSpilt[0] + "/" + tmpSpilt[1] + "/" + tmpSpilt[2]); } catch { return false; }
            }
            else
            {
                if (pDate.Length == 8)
                    pDate = "0" + pDate;
                if (pDate.Length != 9)
                    return false;
                string[] tmpSpilt = pDate.Split(pSpilt);
                if (tmpSpilt.Length != 3)
                    return false;
                if (IsNumeric(tmpSpilt[0]) == false | IsNumeric(tmpSpilt[1]) == false | IsNumeric(tmpSpilt[2]) == false)
                    return false;
                if (System.Convert.ToInt32(tmpSpilt[0]) < 1 | System.Convert.ToInt32(tmpSpilt[0]) > 200)
                    return false;
                if (System.Convert.ToInt32(tmpSpilt[1]) < 1 | System.Convert.ToInt32(tmpSpilt[1]) > 12)
                    return false;
                if (System.Convert.ToInt32(tmpSpilt[2]) < 1 | System.Convert.ToInt32(tmpSpilt[2]) > 31)
                    return false;
                try { DateTime.Parse(tmpSpilt[0] + 1911 + "/" + tmpSpilt[1] + "/" + tmpSpilt[2]); } catch { return false; }

            }
            return true;
        }

        /// <summary>
        /// 檢查是否是數值
        /// </summary>
        /// <param name="Expression"></param>
        /// <returns></returns>
        public bool IsNumeric(object Expression)
        {
            bool isNum;

            double retNum;

            isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);

            return isNum;
        }

        /// <summary>
        /// 檢查製令製程狀態
        /// </summary>
        /// <param name="pModCode"></param>
        /// <returns></returns>
        public string Chk_Status(string pModCode)
        {
            string sStatus = "未開工";
            string sSql = "select * from MEM02_0000 " +
                          " where mod_code = '" + pModCode + "' " +
                          "  order by time_s DESC ";
            var dtTmp = Get_DataTable(sSql);

            if (dtTmp.Rows.Count > 0)
            {
                switch (dtTmp.Rows[0]["status"].ToString())
                {
                    case "E":
                        sStatus = "生產中";
                        break;
                    case "P":
                        sStatus = "暫停中";
                        break;
                    case "Y":
                        sStatus = "已完工";
                        break;
                    default:
                        sStatus = "未開工";
                        break;
                }
            }
            return sStatus;
        }

        //字串處理函數
        public string Left(string param, int length)
        {
            string result = param;
            if (param.Length >= length)
            {
                result = param.Substring(0, length);
            }
            return result;
        }

        public string Right(string param, int length)
        {
            string result = "";
            if (param.Length > 0)
            {
                result = param.Substring(param.Length - length, length);
            }
            return result;
        }

        public string Mid(string param, int startIndex, int length)
        {
            string result = param.Substring(startIndex, length);
            return result;
        }


        #region
        /// <summary>
        /// 取得下拉選項的資料來源
        /// </summary>
        /// <param name="pCode">選項代碼關聯BDP210</param>
        /// <returns></returns>
        public List<DDLList> Get_DDLOption(string pCode)
        {
            List<DDLList> list = new List<DDLList>();
            string sSql = "";

            //使用BDP210的設定值
            //if (pType == "A")
            //{
            sSql = "SELECT field_code, field_name, (SELECT show_type FROM BDP21_0000 WHERE code_code=@code_code ) as show_type FROM BDP21_0100 where code_code=@code_code and is_use='Y' order by scr_no";
            //}

            using (SqlConnection con_db = Set_DBConnection())
            {
                SqlCommand sqlCommand = new SqlCommand(sSql);
                sqlCommand.Connection = con_db;
                //if (pType == "A")
                //{
                sqlCommand.Parameters.Add(new SqlParameter("@code_code", pCode));
                //}
                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    DDLList data = new DDLList();
                    data.field_code = reader["field_code"].ToString();
                    data.field_name = reader["field_name"].ToString();
                    data.show_type = reader["show_type"].ToString();
                    list.Add(data);
                }

            }
            return list;
        }

        /// <summary>
        /// 下拉選項單一table單一鍵值使用
        /// </summary>
        /// <param name="pTableCode">資料表</param>
        /// <param name="pFieldCode">欄位鍵值</param>
        /// <param name="pFieldName">欄位名稱</param>
        /// <param name="pShowType">A:全秀(預設),B:秀值,C:秀名</param>
        /// <returns></returns>
        public List<DDLList> Get_DDLOption(string pTableCode, string pFieldCode = "", string pFieldName = "", string pShowType = "A")
        {
            List<DDLList> list = new List<DDLList>();
            string sSql = "";

            sSql = "SELECT " + pFieldCode + " as field_code, " + pFieldName + " as field_name, '" + pShowType + "' as show_type FROM " + pTableCode + " order by " + pFieldCode + " ";

            using (SqlConnection con_db = Set_DBConnection())
            {
                SqlCommand sqlCommand = new SqlCommand(sSql);
                sqlCommand.Connection = con_db;
                SqlDataReader reader = sqlCommand.ExecuteReader();
                sqlCommand.Parameters.Add(new SqlParameter("@code_code", pFieldCode));

                while (reader.Read())
                {
                    DDLList data = new DDLList();
                    data.field_code = reader["field_code"].ToString();
                    data.field_name = reader["field_name"].ToString();
                    data.show_type = reader["show_type"].ToString();
                    list.Add(data);
                }
            }
            return list;
        }

        /// <summary>
        /// 取得下拉選項資料來源 用自訂的SQL語法
        /// </summary>
        /// <param name="pSql">自定義有field_code,field_name的sql語法</param>
        /// <param name="pShowType">A:全秀(預設),B:秀值,C:秀名</param>
        /// <returns></returns>
        public List<DDLList> Get_DDLOption(string pSql, string pShowType = "A")
        {
            List<DDLList> list = new List<DDLList>();
            string sSql = "";

            sSql = pSql;

            using (SqlConnection con_db = Set_DBConnection())
            {
                SqlCommand sqlCommand = new SqlCommand(sSql);
                sqlCommand.Connection = con_db;
                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    DDLList data = new DDLList();
                    data.field_code = reader["field_code"].ToString();
                    data.field_name = reader["field_name"].ToString();
                    data.show_type = pShowType;
                    list.Add(data);
                }
            }
            return list;
        }

        #endregion

        /// <summary>
        /// 取得現在日期字串，格式: yyyy/MM/dd
        /// </summary>
        /// <returns></returns>
        public string Get_Date()
        {
            return DateTime.Now.ToString("yyyy/MM/dd");
        }


        public string Get_Time()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        /// <summary>
        /// 更新單一欄位
        /// </summary>
        /// <param name="pTable">資料表</param>
        /// <param name="pKeyField">鍵值欄位</param>
        /// <param name="pKeyValue">鍵值</param>
        /// <param name="pUpdField">更新欄位</param>
        /// <param name="pUpdValue">更新值</param>
        public void Upd_Data(string pTable, string pKeyField, string pKeyValue, string pUpdField, string pUpdValue)
        {
            string sSql = "update " + pTable + "" +
                          "  set " + pUpdField + " = '" + pUpdValue + "'" +
                          " where " + pKeyField + " = '" + pKeyValue + "'";
            Connect_DB(sSql);
        }

        /// <summary>
        /// 儲存訊息到BDP20_0000資料表
        /// </summary>
        /// <param name="pUsrCode">使用者代號</param>
        /// <param name="pPrgCode">程式代號</param>
        /// <param name="pUsrType">使用類型</param>
        /// <param name="pCMEMO">使用備註</param>
        public void Ins_BDP20_0000(string pUsrCode, string pPrgCode, string pUsrType, string pCMEMO, object pSqlParams = null, bool bCheckIsSave = true)
        {
            if (bCheckIsSave)
            {
                string is_save = Get_QueryData("BDP00_0000", "save_data_log", "par_name", "par_value");
                if (is_save != "Y")
                {
                    return;
                }
            }

            BDP20_0000 data = new BDP20_0000()
            {
                usr_code = pUsrCode,
                prg_code = pPrgCode,
                usr_date = Get_Date(),
                usr_time = Get_Time(),
                usr_type = pUsrType,
                cmemo = pCMEMO,
                params_json = pSqlParams == null ? "" : JsonConvert.SerializeObject(pSqlParams)
            };

            string sSql = "INSERT INTO " +
                          " BDP20_0000 ( usr_code,  prg_code," +
                          "              usr_date,  usr_time,  cmemo, usr_type, params_json) " +
                          "     VALUES (@usr_code, @prg_code," +
                          "             @usr_date, @usr_time, @cmemo, @usr_type, @params_json)";

            using (SqlConnection con_db = Set_DBConnection())
            {
                con_db.Execute(sSql, data);
            }
        }

        /// <summary>
        /// 更新單一欄位，1對1鍵值
        /// </summary>
        /// <param name="pTableCode">資料表</param>
        /// <param name="pKeyCode">鍵值欄位</param>
        /// <param name="pKeyValue">鍵值</param>
        /// <param name="pFieldCode">更新欄位</param>
        /// <param name="pFieldValue">更新值</param>
        public void Upd_QueryData(string pTableCode, string pKeyCode, string pKeyValue, string pFieldCode, string pFieldValue)
        {
            string sSql = " UPDATE " + pTableCode +
                          "    SET " + pFieldCode + " = @" + pFieldCode +
                          "  WHERE " + pKeyCode + "   = @" + pKeyCode;
            using (SqlConnection con_db = Set_DBConnection())
            {
                SqlCommand sqlCommand = new SqlCommand(sSql);
                sqlCommand.Connection = con_db;
                sqlCommand.Parameters.Add(new SqlParameter(pKeyCode, pKeyValue));
                sqlCommand.Parameters.Add(new SqlParameter(pFieldCode, pFieldValue));
                sqlCommand.ExecuteNonQuery();
            }
        }

    }
}
