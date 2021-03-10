using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace MES_WORK.Models
{
    public class CheckData
    {
        GetData GD = new GetData();
        Comm comm = new Comm();




        /// <summary>
        /// 檢查input
        /// </summary>
        /// <param name="pubFieldTable">資料表</param>
        /// <param name="pubPKCode">鍵值欄位</param>
        /// <param name="Key">索引鍵</param>
        /// <param name="pValue">索引值</param>
        /// <param name="pIsChkMulti">是否檢查重複</param>
        /// <returns></returns>
        public string Chk_Input(string pubFieldTable,string pubPKCode, string Key, string pValue,bool pIsChkMulti = true)
        {
            //回傳有錯的input
            string sValue = "";
            string sDataType = GD.Get_Data(pubFieldTable, Key, pubPKCode, "data_type"); //資料型態
            string sFieldLength = GD.Get_Data(pubFieldTable, Key, pubPKCode, "field_length"); //資料長度
            string sFieldName = GD.Get_Data(pubFieldTable, Key, pubPKCode, "field_name");
            string sNeed_Value = GD.Get_Data(pubFieldTable, Key, pubPKCode, "need_value");
            string Is_key = GD.Get_Data(pubFieldTable, Key, pubPKCode, "is_key");
            string sIsMulti = GD.Get_Data(pubFieldTable, Key, pubPKCode, "is_multi");
            string sAlertStr = "";

            if (sNeed_Value == "Y")
            {
                switch (sDataType)
                {
                    case "C":
                        //核取off的時候會是空值
                        break;
                    default:
                        if (pValue == "")
                        {
                            sValue += sFieldName + " 為必填欄位\n";
                            return sValue;
                        }
                        break;
                }
            }

            if(pIsChkMulti)
                if (sIsMulti == "N")
                    if (Chk_IsMulti(Key, pValue)) {
                        sAlertStr += " 資料有重複!";
                    }

            if (Is_key != "Y")
            {
                switch (sDataType)
                {
                    case "S":
                        //字串 > 只判斷長度
                        if (pValue.Length > int.Parse(sFieldLength)) { sAlertStr = " 輸入字串長度過長，限制為" + sFieldLength + "碼"; }
                        break;
                    case "I":
                        //整數
                        if (!IsNumeric(pValue) || pValue == "") { sAlertStr = " 輸入數字格式有誤"; }
                        else
                        {
                            decimal mValue = decimal.Parse(pValue);
                            if (mValue != Math.Ceiling(mValue)) { sAlertStr = " 輸入數字須為整數"; }
                        }
                        break;
                    case "F":
                        //浮點數
                        if (!IsNumeric(pValue) || pValue == "") { sAlertStr = " 輸入數字格式有誤"; }
                        break;
                    case "D":
                        //日期
                        if (!IsDate(pValue)) { sAlertStr = " 輸入日期格式有誤"; }
                        break;
                    default:
                        break;
                }
            }
            if (sAlertStr != "") { sValue += sFieldName + sAlertStr; }
            //sValue有值的話就會發出警告
            return sValue;
        }

        /// <summary>
        /// 檢查該表單內的該欄位是否重複
        /// </summary>
        /// <param name="Key">欄位鍵值</param>
        /// <returns></returns>
        public bool Chk_IsMulti(string Key,string pValue)
        {
            string sEpbCode = GD.Get_Data("EPB02_0100", Key, "epb02_0100", "epb_code");
            string sFieldCode = GD.Get_Data("EPB02_0100", Key, "epb02_0100", "field_code");
            bool sValue = false;
            string sSql = "select * from EPB03_0000 " +
                          " where epb_code = @epb_code" +
                          "   and field_code = @field_code" +
                          "   and field_value = @field_value ";
            var dtTmp = comm.Get_DataTable(sSql, "epb_code,field_code,field_value", sEpbCode + ","+ sFieldCode + "," + pValue);
            if (dtTmp.Rows.Count > 0)
            {
                sValue = true;
            }
            return sValue;
        }


        /// <summary>
        /// 檢查該表單是否只有一個鍵值
        /// </summary>
        /// <param name="pEpbCode">表單代號</param>
        /// <returns></returns>
        public bool Chk_OnlyKey(string pEpbCode) {
            bool sValue = false;
            string sSql = "select * from EPB02_0100 " +
                          " where epb_code = @epb_code" +
                          "   and is_key = 'Y'";
            var dtTmp = comm.Get_DataTable(sSql, "epb_code", pEpbCode);
            if (dtTmp.Rows.Count <= 1) {
                sValue = true;
            }
            return sValue;
        }



        public bool IsDate(string strDate)
        {
            try
            {
                DateTime.Parse(strDate);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsNumeric(String strNumber)
        {
            Regex NumberPattern = new Regex("[^0-9.-]");
            return !NumberPattern.IsMatch(strNumber);
        }


    }
}