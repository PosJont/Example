using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Dapper;
using System.Web.Mvc;
using System.ComponentModel;

namespace MES_WORK.Models
{
    public class MesApi
    {
        Comm comm = new Comm();
        Work iWork = new Work();
        SetBasic_Data Chk_Data = new SetBasic_Data();
        MesApiResponse ApiResponse = new MesApiResponse();
        string sDate = DateTime.Now.ToString("yyyy/MM/dd");
        string sTime = DateTime.Now.ToString("HH:mm:ss");

        /// <summary>
        /// 檢查資料
        /// </summary>
        /// <param name="job"></param>
        /// <param name="sToken"></param>
        public void ChkData(JObject job,string sToken , string sFunName="")
        {
            try
            {             
                string sErrMsg = "";
                Chk_Data.sToken = comm.Get_QueryData("BDP08_0000", sToken, "token", "usr_code");
                Chk_Data.sMacCode = job["mac_code"].ToString() != "" ? job["mac_code"].ToString() : iWork.Get_MacCodeByMacAddress();
                Chk_Data.sUser = job["per_code"].ToString() != "" ? job["per_code"].ToString() : iWork.Get_UserCodeByMacCode(Chk_Data.sMacCode);
                Chk_Data.sStationCode = iWork.Get_StationCodeByMacCode(Chk_Data.sMacCode);
                Chk_Data.sWorkCode = iWork.Get_WorkCodeByStationCode(Chk_Data.sStationCode);
                foreach (JProperty jProperty in job.Properties())
                {
                    if (jProperty.Name == "mo_code") Chk_Data.sMoCode =  job[jProperty.Name].ToString() ;
                    else Chk_Data.sMoCode =iWork.Get_MacCodeByMacAddress();

                    if (jProperty.Name == "wrk_code") Chk_Data.sWrkCode = job[jProperty.Name].ToString();
                    else Chk_Data.sWrkCode = iWork.Get_WrkCodeByMacCode(Chk_Data.sMacCode);

                    switch (jProperty.Name)
                    {
                        case "pallet_code":
                            Chk_Data.sPalletCode = job[jProperty.Name].ToString() != null ? job[jProperty.Name].ToString() : "";
                            //sErrMsg += !comm.Chk_Basic("MEB29_0000", Chk_Data.sPalletCode, "ng_code") ? "無此容器編號。" : "";
                            break;
                        case "pro_lot_no":
                            Chk_Data.sLotNo = job[jProperty.Name].ToString() != null ? job[jProperty.Name].ToString() : "";
                            break;

                        case "ng_code":
                            Chk_Data.sNgCode = job[jProperty.Name].ToString() != null ? job[jProperty.Name].ToString() : "";
                            sErrMsg += !comm.Chk_Basic("MEB37_0000", Chk_Data.sNgCode, "ng_code") ? "無此不良代碼。" : "";
                            break;


                        case "pro_qty": case"ng_qty":
                            Chk_Data.sProQty = job[jProperty.Name].ToString();
                            break;
                    }
                }
                Chk_Data.sProCode =
                    (Chk_Data.sWorkCode == "L101" | Chk_Data.sWorkCode == "L302") ?
                        comm.Get_QueryData("WMT07_0000", Chk_Data.sWrkCode, "wrk_code", "pro_code") :// 剝蛋站使用半成品料號
                        comm.Get_QueryData("MET01_0000", Chk_Data.sMoCode, "mo_code", "pro_code");// 其餘使用成品料號

                sErrMsg += !Chk_Token(sToken) ? "Token驗證失敗。" : "";
                sErrMsg += !comm.Chk_Basic("MEB29_0000", Chk_Data.sMacCode,    "mac_code") && Chk_Data.sMacCode != "PDA" ? "無此機器號碼。" : "";
                sErrMsg += !comm.Chk_Basic("BDP08_0000", Chk_Data.sUser,       "usr_code") && sFunName  ==""? "無此人員資料。" : "";
                sErrMsg += !comm.Chk_Basic("MEB20_0000", Chk_Data.sProCode,    "pro_code") && sFunName == "" ? "無此產品編號。" : "";
                sErrMsg += !comm.IsNumeric(Chk_Data.sProQty) ? "數量格式錯誤" : "";
                if (sErrMsg != "") ApiResponse.Set_MesResponse("NG", sErrMsg);
            }
            catch (Exception ex) { ApiResponse.Set_MesResponse("NG", "Json格式錯誤" + ex.Message); }
        }

        /// <summary>
        /// 寫入生產良品數量
        /// </summary>
        /// <param name="sToken"></param>
        /// <param name="sJson"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public string Ins_ProQtyData(string sToken, string sJson, FormCollection form = null)
        {
            // ---設定Funtion名稱
            ApiResponse.Set_MesResponse("OK", "");
            string sFunName = "Ins_ProQtyData";

            //輸入參數紀錄
            List<JsonData> data = new List<JsonData>() {
               new JsonData(){ pToken = sToken, pJson = sJson }
            };
            //清空裡面資料
            comm.Set_ModelDefaultValue(Chk_Data);
            //確認資料
            ChkData((JObject)JsonConvert.DeserializeObject(sJson), sToken);

            // 以上檢查OK才執行功能
            if (ApiResponse.Result == "OK")
            {
                // ---資料存檔語法 MED09_0000
                MED09_0000 data_m09 = new MED09_0000();
                comm.Set_ModelDefaultValue(data_m09);
                comm.Set_ModelValue(data_m09, form);
                data_m09.mo_code = Chk_Data.sMoCode;
                data_m09.wrk_code = Chk_Data.sWrkCode;
                data_m09.mac_code = Chk_Data.sMacCode;
                data_m09.pro_code= Chk_Data.sProCode;
                data_m09.work_code = Chk_Data.sWorkCode;
                data_m09.station_code = Chk_Data.sStationCode;
                data_m09.pro_lot_no = Chk_Data.sLotNo != "" ? Chk_Data.sLotNo :  comm.Get_ServerDateTime("yyyy/MM/dd").Replace("/", "");  // 產品批號先暫時以產品編號+日期
                data_m09.pro_qty = Chk_Data.sProQty != "" ? double.Parse(Chk_Data.sProQty) : 0;
                data_m09.ins_date = sDate;
                data_m09.ins_time = sTime;
                data_m09.usr_code = Chk_Data.sUser;
                data_m09.is_end = "N";
                data_m09.is_ng = "N";
                if (ApiResponse.Message != "")
                {
                    data_m09.is_ng = "Y";
                    data_m09.des_memo = ApiResponse.Message;
                    ApiResponse.Set_MesResponse("NG", ApiResponse.Message);
                }

                // 更新歷程檔
                if (ApiResponse.Result != "NG")
                {
                    // Insert Data to MED090A
                    data_m09.InsertData(data_m09);

                    if (comm.Chk_MEM01Data(data_m09.work_code, data_m09.mo_code, data_m09.station_code, data_m09.mac_code)) 
                        Upd_MEM01Data(data_m09.work_code, data_m09.mo_code, data_m09.station_code, data_m09.mac_code, data_m09.usr_code);
                    Upd_MEM01OKQty(data_m09.work_code, data_m09.mo_code);
                    ApiResponse.Set_MesResponse("OK", "報工成功");
                }
            }
            // 寫入紀錄
            Set_Apilog(Chk_Data.sUser, sFunName, data);
            return JsonConvert.SerializeObject(ApiResponse);
        }

        /// <summary>
        /// 寫入不良數量及不良原因
        /// </summary>
        /// <param name="sToken">Token</param>
        /// <param name="sJson">Json</param>
        /// <returns></returns>
        public string Ins_MoNgData(string sToken, string sJson, FormCollection form = null)
        {
            ApiResponse.Set_MesResponse("OK", "");
            string sFunName = "Ins_MoNgData";

            //輸入參數紀錄
            List<JsonData> data = new List<JsonData>() {
               new JsonData(){ pToken = sToken, pJson = sJson }
            };

            //清空裡面資料
            comm.Set_ModelDefaultValue(Chk_Data);
            //確認資料
            ChkData((JObject)JsonConvert.DeserializeObject(sJson), sToken);


            //檢查OK才執行功能
            if (ApiResponse.Result == "OK")
            {
                MED03_0000 data_NG = new MED03_0000();
                comm.Set_ModelDefaultValue(data_NG);
                comm.Set_ModelValue(data_NG, form);
                data_NG.mo_code = Chk_Data.sMoCode;
                data_NG.wrk_code = Chk_Data.sWrkCode;
                data_NG.mac_code = Chk_Data.sMacCode;
                data_NG.pro_code = Chk_Data.sProCode;
                data_NG.ng_code = Chk_Data.sNgCode;
                data_NG.ng_qty = Chk_Data.sProQty !="" ? float.Parse(Chk_Data.sProQty) :0;
                data_NG.ins_date = sDate;
                data_NG.ins_time = sTime;
                data_NG.usr_code = Chk_Data.sUser;
                data_NG.station_code = Chk_Data.sStationCode;
                data_NG.work_code = Chk_Data.sWorkCode;
                data_NG.is_end = "N";
                data_NG.is_ng = "N";

                if (ApiResponse.Message != "")
                {
                    data_NG.is_ng = "Y";
                    data_NG.des_memo = ApiResponse.Message;
                }
                if(ApiResponse.Result != "NG")
                {
                    data_NG.InsertData(data_NG);
                    Upd_MEM01NgQty(data_NG.mo_code, data_NG.work_code, data_NG.ng_qty);//更新MEM1NG數量
                }
            }
            Set_Apilog(Chk_Data.sUser, sFunName, data); // 寫入查詢紀錄
            return JsonConvert.SerializeObject(ApiResponse);
        }


        /// <summary>
        /// 寫入退料記錄
        /// </summary>
        /// <param name="pToken"></param>
        /// <param name="JSON"></param>
        /// <returns></returns>
        public string Ins_PreparEdData(string sToken, string sJson, FormCollection form)
        {
            ApiResponse.Set_MesResponse("OK", "");
            string sFunName = "Ins_PreparEdData";

            //輸入參數紀錄
            List<JsonData> data = new List<JsonData>() {
               new JsonData(){ pToken = sToken, pJson = sJson }
            };

            //清空裡面資料
            comm.Set_ModelDefaultValue(Chk_Data);
            //確認資料
            ChkData((JObject)JsonConvert.DeserializeObject(sJson), sToken, "Ins_PreparEdData");

            // 關聯 參數
            RTR07_0000 data_log = new RTR07_0000();
            comm.Set_ModelValue(data_log, (JObject)JsonConvert.DeserializeObject(sJson));


            //檢查OK才執行功能
            if (ApiResponse.Result == "OK")
            {
                // ---資料存檔語法
                string sProQty = Chk_Data.sProQty;
                string sProUnit = "";
                string pro_code = Chk_Data.sProCode ;
                string lot_no = Chk_Data.sLotNo;
                string[] sProQtyUnit = Get_ProQtyUnit(pro_code, lot_no).Split('|');

                if (sProQtyUnit.Length > 1)
                {
                    sProQty = sProQtyUnit[0];
                    sProUnit = sProQtyUnit[1];
                }

                // pro_qty必須是數字格式'
                if (string.IsNullOrEmpty(sProQty)) sProQty = "0";

               string sSql =
                     " select  sum(m.pro_qty) as pro_qty,m.mo_code ,m.wrk_code , b.pro_qty as res_qty,m.work_code,                 " +
                     " m.mo_code, m.ins_date from MED06_0000 m                                                                     " +
                     " left join MED02_0000 a on m.wrk_code = a.wrk_code                                                           " +
                     " left join MET01_0100 b on b.mo_code = m.mo_code and m.pro_code = b.pro_code  and m.work_code = b.work_code  " +
                     " where m.work_code = '" + Chk_Data.sWorkCode + "' and lot_no = '" + lot_no + "' and m.pro_code='"+ pro_code + "'      " +
                     " Group by m.mo_code, m.ins_date, m.mo_code,m.wrk_code , b.pro_qty,m.work_code                                "+
                " order by m.ins_date desc ";

                DataTable dTmp = comm.Get_DataTable(sSql);
                if (dTmp.Rows.Count > 0)
                {
                    foreach (DataRow drTmp in dTmp.Rows)
                    {
                        //int i = 0;
                        double iProQty = drTmp["pro_qty"].ToString() != "" ? double.Parse(drTmp["pro_qty"].ToString().Trim()) : 0; // 資料庫上料檔中應上料數量
                        double iResQty = double.Parse(drTmp["res_qty"].ToString().Trim()); // 資料庫上料檔中已上料數量
                        double iUseQty = data_log.pro_qty != "" ? double.Parse(data_log.pro_qty) : 0; //這次上料的實際數量
                        string iMoCode = drTmp["mo_code"].ToString().Trim();
                        string iWrkCode = drTmp["wrk_code"].ToString().Trim();
                        double iCanQty = iUseQty;// 可退料數量

                        //string sChkupCode = Get_WMT07ChkupCode(iWrkCode, pro_code);
                        string sLineCode = Get_LineByWrkCode(iWrkCode);
                        string sLocCode = Get_LocCode(sLineCode, pro_code, 1);
                        MED07_0000 data_m07 = new MED07_0000();

                        if (iCanQty <= iProQty)
                        {
                            // 可退料量小於或等於已上料量
                            // '更新MED07_0000退料檔
                            comm.Set_ModelDefaultValue(data_m07);
                            data_m07.mo_code = iMoCode;
                            data_m07.wrk_code = iWrkCode;
                            data_m07.work_code = Chk_Data.sWorkCode;
                            data_m07.mac_code = Chk_Data.sMacCode;
                            data_m07.station_code = Chk_Data.sStationCode;
                            data_m07.pro_code = data_log.pro_code;
                            data_m07.lot_no = data_log.lot_no;
                            data_m07.pro_qty = decimal.Parse(iCanQty.ToString());
                            data_m07.pro_unit = sProUnit;
                            data_m07.ins_date = sDate;
                            data_m07.ins_time = sTime;
                            data_m07.usr_code = data_log.per_code;
                            data_m07.is_end = "N";
                            data_m07.is_ng = "N";
                        }
                        else
                        {
                            // 可退料量大於或等於已上料量 
                            // '更新MED07_0000退料檔
                            comm.Set_ModelDefaultValue(data_m07);
                            data_m07.mac_code = Chk_Data.sMacCode;
                            data_m07.mo_code = iMoCode;
                            data_m07.wrk_code = iWrkCode;
                            data_m07.work_code = Chk_Data.sWorkCode;
                            data_m07.station_code = Chk_Data.sStationCode;
                            data_m07.pro_code = data_log.pro_code;
                            data_m07.lot_no = data_log.lot_no;
                            data_m07.pro_qty = decimal.Parse(iCanQty.ToString());
                            data_m07.pro_unit = sProUnit;
                            data_m07.ins_date = sDate;
                            data_m07.ins_time = sTime;
                            data_m07.usr_code = data_log.per_code;
                            data_m07.is_end = "N";
                            data_m07.is_ng = "N";

                            if (ApiResponse.Message != "")
                            {
                                data_m07.is_end = "Y";
                                data_m07.des_memo = ApiResponse.Message;
                                ApiResponse.Set_MesResponse("NG", ApiResponse.Message);
                            }
                        }
                        if (ApiResponse.Result == "OK")
                        {
                            data_m07.InsertData(data_m07);
                            Ins_MEM05Out(data_m07.mac_code, pro_code, lot_no, iProQty.ToString());
                            // '更新WMT07以上料量
                            //Upd_WMT07ResQty(sChkupCode, double.Parse("-" + iProQty + ""));
                            // '更新工單狀態
                            if (!comm.Chk_WrkCodeIsReady(iWrkCode))
                            {
                                Upd_MET03_status(iWrkCode, "NONE");
                                Upd_MET01_status(Chk_Data.sMoCode, "20");
                            }
                        }
                        WMT0200 mT0200 = new WMT0200();
                        mT0200.Ins_WMT02(iWrkCode, lot_no, pro_code, data_log.pro_qty, sLocCode, "202", "I", data_log.per_code);
                        ApiResponse.Set_MesResponse("OK", "退料成功");
                    }

                }
            }
            // 寫入查詢紀錄
            Set_Apilog(Chk_Data.sUser, sFunName, data);
            return JsonConvert.SerializeObject(ApiResponse);

        }

        /// <summary>
        /// 寫入上料記錄
        /// </summary>
        /// <param name="pToken"></param>
        /// <param name="JSON"></param>
        /// <returns></returns>
        public string Ins_PreparData(string sToken, string sJson, FormCollection form = null)
        {
            ApiResponse.Set_MesResponse("OK", "");
            string sFunName = "Ins_PreparData";

            //輸入參數紀錄
            List<JsonData> data = new List<JsonData>() {
               new JsonData(){ pToken = sToken, pJson = sJson }
            };

            //清空裡面資料
            comm.Set_ModelDefaultValue(Chk_Data);
            //確認資料
            ChkData((JObject)JsonConvert.DeserializeObject(sJson), sToken , "Ins_PreparData");

            // 取得Json參數
            JObject job = (JObject)JsonConvert.DeserializeObject(sJson);
            string sErrMsg = "";

            // SET RTR070A 參數
            RTR07_0000 data_log = new RTR07_0000();
            comm.Set_ModelValue(data_log, job);


            //檢查OK才執行功能
            if (ApiResponse.Result == "OK")
            {
                // ---資料存檔語法
                //GET WMT06_0110 QTY AND UNIT
                string sProQty = ""; //生產數量
                string sProUnit = "";//生產單位
                string[] sProQtyUnit = Get_ProQtyUnit(data_log.pro_code, data_log.lot_no).Split('|');
                if (sProQtyUnit.Length > 1)
                {
                    sProQty = sProQtyUnit[0];
                    sProUnit = sProQtyUnit[1];
                }
                if (string.IsNullOrEmpty(sProQty)) sProQty = "0";

                //GET 上料檔資料
                string sSql =
                " SELECT  e.chkup_code,m.mo_code,m.wrk_code, m.work_code ,                                                                    " +
                " ISNULL(ISNULL(sum( c.pro_qty),0)-ISNULL((SELECT  sum(pro_qty)  FROM WMT0200 WHERE   (pro_code = m.pro_code) AND  rel_type = '202' AND rel_code = m.wrk_code),0),0) as res_qty " +
                " ,sum(b.pro_qty) as pro_qty    FROM MET03_0000 m                                    " +
                " left join MET01_0000 a on m.mo_code=a.mo_code                                                                 " +
                " left join MET01_0100 b on b.mo_code = m .mo_code and m.pro_code =b.pro_code and m.work_code =b.work_code      " +
                " left join MED06_0000 c on c.pro_code = m.pro_code and m.mo_code  = c.mo_code                                  " +
                " left join MEB20_0000 d on d.pro_code = m.pro_code " +
                " left join WMT07_0000 e on m.mo_code = e.mo_code  and m.pro_code = e.pro_code and m.work_code =e.work_code "+
                " WHERE m.pro_code = '" + data_log.pro_code + "' and m.work_code='" + Chk_Data.sWorkCode + "' " +
                " and a.sch_date_s='" + comm.Get_ServerDateTime("yyyy/MM/dd") + "'" +
                "    GROUP BY m.mo_code,m.wrk_code, m.work_code ,b.pro_qty ,m.pro_code,d.pro_code,d.pro_name,e.scr_no, e.chkup_code ";
                DataTable dTmp = comm.Get_DataTable(sSql);
                foreach (DataRow drTmp in dTmp.Rows)
                {
                    int i = 0;
                    string mo_status = (i == 0) ? "IN" : "IN" + i;
                    double iProQty = double.Parse(drTmp["pro_qty"].ToString()); // 資料庫上料檔中應上料數量
                    double iResQty = double.Parse(drTmp["res_qty"].ToString()); // 資料庫上料檔中已上料數量
                    double iCanQty = iProQty - iResQty; // 可上料數量
                    double iUseQty = double.Parse(data_log.pro_qty);
                    string sChkUpCode = drTmp["chkup_code"].ToString().Trim();
                    string sWrkCode=  Chk_Data.sWrkCode;
                    string sMoCode =  Chk_Data.sMoCode ;
                    string sMacCode = Chk_Data.sMacCode;

                    if (iCanQty - iUseQty >= 0 && iUseQty != 0)
                    {
                        MED06_0000 data_m06 = new MED06_0000();
                        comm.Set_ModelDefaultValue(data_m06); //設定回預設值
                        comm.Set_ModelValue(data_m06, form);
                        data_m06.wrk_code = sWrkCode;
                        data_m06.work_code = Chk_Data.sWorkCode;
                        data_m06.station_code = Chk_Data.sStationCode;
                        data_m06.mac_code = sMacCode;
                        data_m06.pro_code = data_log.pro_code;
                        data_m06.lot_no = data_log.lot_no;
                        data_m06.pro_qty = decimal.Parse(iUseQty.ToString());
                        data_m06.pro_unit = sProUnit;
                        data_m06.loc_code = data_log.loc_code;
                        data_m06.ins_date = sDate;
                        data_m06.ins_time = sTime;
                        data_m06.usr_code = data_log.per_code;
                        data_m06.mo_code = sMoCode;
                        data_m06.is_end = "N";

                        if (sErrMsg != "")
                        {
                            data_m06.is_end = "Y";
                            data_m06.des_memo = sErrMsg;
                            ApiResponse.Set_MesResponse("NG", sErrMsg);
                        }
                        if (ApiResponse.Result == "OK")
                        {
                            // 可上料數量 - 此次上料實際數量 >= 0  代表可上這次的料，所以直接取用這個chkupCode進行量的增減，離開迴圈
                            //更新WMT07
                            Upd_WMT07ResQty(sChkUpCode, iUseQty);

                            if (!comm.Chk_WrkCodeIsReady(sWrkCode))
                            {
                                Upd_MET03_status(sWrkCode, "READY");
                                Upd_MET01_status(sMoCode, "30");
                            }
                            // 更新MEM04_0000 生產歷程檔
                            Upd_MEM04MoCode(sMacCode, sMoCode, sWrkCode);
                            // 更新MEM01_0000 生產歷程檔
                            if (comm.Chk_MEM01Data(Chk_Data.sWorkCode, sMoCode, Chk_Data.sStationCode, sMacCode))
                                Upd_MEM01Data(Chk_Data.sWorkCode, sMoCode, Chk_Data.sStationCode, sMacCode, data_log.per_code);

                            Ins_MEM05(sMacCode, sMoCode, data_log.pro_code, data_log.lot_no, iUseQty.ToString()); // 寫入MEM05_0000

                            data_m06.InsertData(data_m06);
                            iUseQty -= iCanQty;
                        }
                    }
                    WMT0200 mT0200 = new WMT0200();
                    mT0200.Ins_WMT02(sWrkCode, data_log.lot_no, data_log.pro_code, data_log.pro_qty, data_log.loc_code, "201", "O", data_log.per_code);
                }
                ApiResponse.Set_MesResponse("OK", "上料完成，目前派工單：" + Chk_Data.sWrkCode);
            }
            // 寫入查詢紀錄
            Set_Apilog(Chk_Data.sUser, sFunName, data);
            return JsonConvert.SerializeObject(ApiResponse);
        }


        /// <summary>
        /// 傳入資料更新MEM05_0000退料數量
        /// </summary>
        /// <param name="pMacCode"></param>
        /// <param name="pProCode"></param>
        /// <param name="pLotNo"></param>
        /// <param name="pProQty"></param>
        private void Ins_MEM05Out(string pMacCode, string pProCode, string pLotNo, string pProQty)
        {
            string station_code = Get_StationCodeByMacCode(pMacCode);
            string work_code = Get_WorkCodeByStationCode(station_code);
            string sSql = "update MEM05_0000 " + "   set out_qty =out_qty+" + pProQty + ", out_time='" +
                DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "'  where mac_code='" +
                pMacCode + "'  and lot_no='" + pLotNo + "'  and pro_code='" + pProCode + "'";
            SaveData(sSql);
        }

        /// <summary>
        /// 傳入資料更新MEM05_0000上料數量
        /// </summary>
        /// <param name="pMacCode"></param>
        /// <param name="pMoCode"></param>
        /// <param name="pProCode"></param>
        /// <param name="pLotNo"></param>
        /// <param name="pProQty"></param>
        private void Ins_MEM05(string pMacCode, string pMoCode, string pProCode, string pLotNo, string pProQty)
        {
            string sSql = "";
            string station_code = Get_StationCodeByMacCode(pMacCode);
            string work_code = Get_WorkCodeByStationCode(station_code);
            if (comm.Chk_MEM05Data(pMacCode, pMoCode, pProCode, pLotNo))
            {
                MEM05_0000 data = new MEM05_0000()
                {
                    mo_code = pMacCode,
                    work_code = work_code,
                    station_code = station_code,
                    mac_code = pMacCode,
                    in_time = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"),
                    out_time = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"),
                    pro_code = pProCode,
                    lot_no = pLotNo,
                    in_qty = decimal.Parse(pProQty),
                    in_unit = "",
                    out_qty = 0,
                    out_unit = "",
                };
                data.InsertData(data);
            }
            else
            {
                sSql = "update MEM05_0000 " + "   set in_qty =in_qty+" + pProQty + "" +
                    "   where mac_code='" + pMacCode + "'" + "     and mo_code='" +
                    pMoCode + "'" + "     and lot_no='" + pLotNo + "'";
                SaveData(sSql);
            }
        }

        /// <summary>
        /// 更新上料歷程檔的上料狀態
        /// </summary>
        /// <param name="pChkUpCode"></param>
        private void Refresh_WMT07_status(string pChkUpCode)
        {
            string sSql = " update WMT07_0000 set chkup_status='Y' " + "  where chkup_code='" + pChkUpCode + "'" + "    and res_qty >= pro_qty";
            SaveData(sSql);
        }

        /// <summary>
        /// 更新MEM010A 良品數量
        /// </summary>
        /// <param name="pWorkCode"></param>
        /// <param name="pMoCode"></param>
        private void Upd_MEM01OKQty(string pWorkCode, string pMoCode)
        {
            string sSql = "";
            double iOkQty =Get_OKQtyByMED09(pWorkCode, pMoCode);
            sSql = "update MEM01_0000 " + "   set ok_qty=N'" + iOkQty + "'" + " where mo_code='" + pMoCode + "'" + "   and work_code='" + pWorkCode + "'";
            SaveData(sSql);
        }

        /// <summary>
        /// Upd_MEM01Data
        /// </summary>
        /// <param name="pWorkCode"></param>
        /// <param name="pMoCode"></param>
        /// <param name="pStationCode"></param>
        /// <param name="pMacCode"></param>
        /// <param name="pUsrCode"></param>
        private void Upd_MEM01Data(string pWorkCode, string pMoCode, string pStationCode, string pMacCode, string pUsrCode)
        {
            string sSql = "update MEM01_0000 " + "   set station_code='" + pStationCode + "'," + "       mac_code='"
                + pMacCode + "'," + "       usr_code='" + pUsrCode + "'," + "       work_time_s='"
                + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + "'" + " where mo_code='" + pMoCode + "'" + "   and work_code='" + pWorkCode + "'";
            SaveData(sSql);
        }

        /// <summary>
        /// Update_MEM04MoCode
        /// </summary>
        /// <param name="pMacCode"></param>
        /// <param name="pMoCode"></param>
        /// <param name="pWrkCode"></param>
        private void Upd_MEM04MoCode(string pMacCode, string pMoCode, string pWrkCode)
        {
            String sSql = "update MEM04_0000 " + "   Set mo_code='" + pMoCode + "'," + "       wrk_code='" + pWrkCode + "'" + " where mac_code='" + pMacCode + "'";
            SaveData(sSql);
        }

        /// <summary>
        /// 更新製令狀態
        /// </summary>
        /// <param name="pMoCode"></param>
        /// <param name="pStaus"></param>
        private void Upd_MET01_status(string pMoCode, string pStaus)
        {
            string sSql = " update MET01_0000 set mo_status='" + pStaus + "' " + "  where mo_code='" + pMoCode + "'";
            SaveData(sSql);
        }

        /// <summary>
        /// 更新派工單狀態
        /// </summary>
        /// <param name="pWrkCode"></param>
        /// <param name="pStaus"></param>
        private void Upd_MET03_status(string pWrkCode, string pStaus)
        {
            string sSql = " update MET03_0000 set mo_status='" + pStaus + "' " + "  where wrk_code='" + pWrkCode + "'";
            SaveData(sSql);
        }

        /// <summary>
        /// 更新MEM1NG數量
        /// </summary>
        /// <param name="pMoCode">工單編號</param>
        /// <param name="pWorkCode">製程代碼</param>
        /// <param name="pNgQty">不良數量</param>
        private void Upd_MEM01NgQty(string pMoCode, string pWorkCode, double pNgQty)
        {
            MEM01_0000 data = new MEM01_0000()
            {
                ng_qty = (float)pNgQty,
                mo_code = pMoCode,
                work_code = pWorkCode
            };
            data.Update_NgData(data);
        }

        /// <summary>
        /// 更新上料歷程檔的已上料數量
        /// </summary>
        /// <param name="pChkUpCode"></param>
        /// <param name="pQty"></param>
        private void Upd_WMT07ResQty(string pChkUpCode, double pQty)
        {
            // 更新已上料量
            string sSql = "UPDATE WMT07_0000 SET res_qty=res_qty+" + pQty + " where chkup_code='" + pChkUpCode + "'";

            // 更新已上料量滿足時
            SaveData(sSql);
        }

        /// <summary>
        /// SQL存檔
        /// </summary>
        /// <param name="sSql"></param>
        private void SaveData(string sSql)
        {
            try
            {
                // 更新已上料量滿足時
                using (SqlConnection con_db = comm.Set_DBConnection())
                {
                    con_db.Execute(sSql);
                }
            }
            catch { }
        }

        /// <summary>
        /// 取得容器資料表
        /// </summary>
        /// <param name="pPalletCode">容器代碼</param>
        /// <returns></returns>
        public DataTable Get_DT_PalletContent(string pPalletCode)
        {
            string sSql = "";

            sSql = "select * from MED09_0000 " +
                   " where pallet_code='" + pPalletCode + "'" +
                   " order by ins_date desc,ins_time desc ";
            DataTable dtFun = comm.Get_DataTable(sSql);
            return dtFun;
        }

        /// <summary>
        /// 取得MED090A良品 
        /// </summary>
        /// <param name="pWorkCode"></param>
        /// <param name="pMoCode"></param>
        /// <returns></returns>
        private double Get_OKQtyByMED09(string pWorkCode, string pMoCode)
        {
            string sSql = "";
            DataTable dtTmp;
            sSql = "select sum(pro_qty) as sum_qty from MED09_0000 " + " where work_code='" + pWorkCode + "'" + "  and mo_code='" + pMoCode + "'";
            dtTmp = comm.Get_DataTable(sSql);
            return (dtTmp.Rows.Count > 0) ? double.Parse(dtTmp.Rows[0]["sum_qty"].ToString()) : 0;
        }

        /// <summary>
        /// 取得儲位代碼
        /// </summary>
        /// <param name="pLineCode"></param>
        /// <param name="pProCode"></param>
        /// <param name="iScrNo"></param>
        /// <returns></returns>
        private string Get_LocCode(string pLineCode, string pProCode, int iScrNo)
        {
            string sProName = comm.Get_QueryData("MEB20_0000", pProCode, "pro_code", "pro_name");
            switch (pLineCode)
            {
                case "A1L01": // 一線
                    if (sProName.Contains("瓶胚") || sProName.Contains("空瓶")) return "L1A01";
                    if (sProName.Contains("蓋")) return "L1B01";
                    if (sProName.Contains("瓶標")) return iScrNo % 2 == 1 ? "L1C01" : "L1C02";
                    if (sProName.Contains("瓶口套")) return iScrNo % 2 == 1 ? "L1D01" : "L1D02";
                    if (sProName.Contains("紙箱")) return "L1E01";
                    if (sProName.Contains("量販包")) return "LSA01";
                    break;
                case "A1L02": // 二線
                    if (sProName.Contains("瓶胚")) return sProName.Contains("2000") ? "L2A01" : "L2A01,L2A02";
                    if (sProName.Contains("空瓶")) return "L2A03";
                    if (sProName.Contains("蓋")) return "L2B01";
                    if (sProName.Contains("瓶標")) return iScrNo % 2 == 1 ? "L2C01" : "L2C02";
                    if (sProName.Contains("瓶口套")) return iScrNo % 2 == 1 ? "L2D01" : "L2D02";
                    if (sProName.Contains("紙箱")) return "L2E01";
                    if (sProName.Contains("量販包")) return "LSA01";
                    break;
            }
            return "";
        }

        /// <summary>
        /// 傳入派工單號 回傳線別
        /// </summary>
        /// <param name="pWrkCode"></param>
        /// <returns></returns>
        private string Get_LineByWrkCode(string pWrkCode)
        {
            string sSql = "";
            string sMoCode = pWrkCode.Split('-')[0];
            sSql = "Select plan_line_code From MET01_0000 " + " Where mo_code ='" + sMoCode + "'";
            DataTable dtTmp = comm.Get_DataTable(sSql);

            return dtTmp.Rows.Count > 0 ? dtTmp.Rows[0]["plan_line_code"].ToString().Trim() : "";
        }

        /// <summary>
        /// 取得WMT07_0000 chkup_code 資料
        /// </summary>
        /// <param name="pWrkCode"></param>
        /// <param name="pProCode"></param>
        /// <returns></returns>
        public string Get_WMT07ChkupCode(string pWrkCode, string pProCode)
        {
            string sSql = "";
            DataTable dtTmp;
            sSql = "select chkup_code from WMT07_0000" + " where wrk_code ='" + pWrkCode + "'" + "   and pro_code ='" + pProCode + "'";
            dtTmp = comm.Get_DataTable(sSql);
            return (dtTmp.Rows.Count > 0) ? dtTmp.Rows[0]["chkup_code"].ToString().Trim() : "";
        }

        /// <summary>
        /// 取得退料資料
        /// </summary>
        /// <param name="pToken"></param>
        /// <param name="pMacCode"></param>
        /// <returns></returns>
        public string Get_PreparEdList(string sToken, string sMacCode)
        {
            ApiResponse.Set_MesResponse("OK", "");
            string sFunName = "Get_PreparEdList";

            //輸入參數紀錄
            List<MacData> data = new List<MacData>() {
               new MacData(){ pToken = sToken, pMacCode = sMacCode  }
            };

            //驗證token
            string sUsrCode = "";
            if (!Chk_Token(sToken))
            {
                ApiResponse.Set_MesResponse("NG", "Token驗證失敗");
            }
            else
            {
                sUsrCode = comm.Get_QueryData("BDP08_0000", sToken, "token", "usr_code");
            }

            //驗證機器號碼
            if (!comm.Chk_Basic("MEB29_0000", sMacCode, "mac_code")) ApiResponse.Set_MesResponse("NG", "無此機器號碼");

            string sStationCode = Get_StationCodeByMacCode(sMacCode);
            string sWorkCode = Get_WorkCodeByStationCode(sStationCode);
            if (ApiResponse.Result == "OK")
            {
                //string sSql = "select top 1 *,  " +
                //    " (select top 1 pro_qty as sum_qty from WMT0200 where pro_code=WMT07_0000.pro_code " +
                //    " and ins_type = 'O' and lot_no=MEM05_0000.lot_no order by ins_date desc) as inventory from WMT07_0000 " +
                //    "     left join  MEM05_0000  on (WMT07_0000.mo_code=MEM05_0000.mo_code and WMT07_0000.pro_code=MEM05_0000.pro_code)" +
                //    "     left join  MEB20_0000  on WMT07_0000.pro_code=MEB20_0000.pro_code" +
                //    "     left join  MET01_0000  on WMT07_0000.mo_code=MET01_0000.mo_code" +
                //    " where WMT07_0000.work_code='" + sWorkCode + "'" +
                //    "	 and MEM05_0000.in_time <>''" +
                //    "	 And MET01_0000.sch_date_s='" + comm.Get_ServerDateTime("yyyy/MM/dd") + "'" +
                //     "	 order by MEM05_0000.in_time desc ";
                string sSql =
                    " SELECT m.mo_code,m.wrk_code, m.work_code,m.pro_code ,d.pro_name ,                                              " +
                    " c.lot_no,c.ins_date, " +
                    " 	ISNULL(sum(c.pro_qty),0) -  " +
                    "  	ISNULL((SELECT  sum(pro_qty)  FROM WMT0200 WHERE   (pro_code = m.pro_code) AND  rel_type = '202' AND rel_code = m.wrk_code),0) " +
                    " as pro_qty , " +
                    " ISNULL((select sum(qty) as sum_qty from V_STO_QTY  where pro_code = m.pro_code) ,0) as inventory               " +
                    " FROM MET03_0000 m                                                                                              " +
                    " left join MET01_0000 a on m.mo_code = a.mo_code                                                                " +
                    " left join MET01_0100 b on b.mo_code = m.mo_code and m.pro_code = b.pro_code and m.work_code = b.work_code      " +
                    " left join MED06_0000 c on c.pro_code = m.pro_code and m.mo_code = c.mo_code                                    " +
                    " left join MEB20_0000 d on d.pro_code = m.pro_code                                                              " +
                    " WHERE m.work_code = '" + sWorkCode + "' and a.sch_date_s = '" + comm.Get_ServerDateTime("yyyy/MM/dd") + "'     " +
                    " AND c.ins_date <> '' " +
                    " GROUP BY m.mo_code,m.wrk_code, m.work_code  ,m.pro_code,d.pro_name ,c.lot_no,c.ins_date                        " +
                   " HAVING   ISNULL(sum(c.pro_qty), 0) - ISNULL((SELECT  sum(pro_qty)  FROM WMT0200 WHERE "+
                    " (pro_code = m.pro_code) AND  rel_type = '202' AND rel_code = m.wrk_code),0) > 0";
                    ApiResponse.Data = comm.Get_DataTable(sSql);
            }

            Set_Apilog(sUsrCode, sFunName, data);

            return JsonConvert.SerializeObject(ApiResponse);
        }

        /// <summary>
        /// 取得產品數量及單位
        /// </summary>
        /// <param name="pProCode"></param>
        /// <param name="pLotNo"></param>
        /// <returns></returns>
        private string Get_ProQtyUnit(string pProCode, string pLotNo)
        {
            string sSql = "SELECT TOP 1 pro_qty,pro_unit FROM WMT06_0110" +
                " WHERE pro_code='" + pProCode + "'" + " AND lot_no='" + pLotNo + "'" + " ORDER BY wmt06_0110 DESC";
            DataTable dtTmp = comm.Get_DataTable(sSql);
            return (dtTmp.Rows.Count > 0) ? dtTmp.Rows[0]["pro_qty"].ToString() + "|" + dtTmp.Rows[0]["pro_unit"].ToString() : "";
        }

        /// <summary>
        /// QRcode
        /// </summary>
        /// <param name="pQrcode"></param>
        /// <returns></returns>
        public string Get_SplitQrCode(string sQrcode)
        {
            ApiResponse.Set_MesResponse("OK", "");
            //string sFunName = "Get_SplitQrCode";

            //輸入參數紀錄
            DataTable dtRec = new DataTable();
            dtRec.Columns.Add("pQrCode", Type.GetType("System.String"));


            // 拆解條碼
            string[] sQrCodeTmp = sQrcode.Split('%');
            if (sQrCodeTmp.Length < 7) ApiResponse.Set_MesResponse("NG", "請確認條碼是否正確");
            //驗證token

            if (ApiResponse.Result == "OK")
            {
                DataTable dtData = new DataTable();
                dtData.Columns.Add("pro_code", Type.GetType("System.String"));
                dtData.Columns.Add("lot_no", Type.GetType("System.String"));
                dtData.Columns.Add("sor_code", Type.GetType("System.String"));
                dtData.Columns.Add("tra_Code", Type.GetType("System.String"));
                dtData.Columns.Add("qr_type", Type.GetType("System.String"));
                dtData.Columns.Add("tracking_no", Type.GetType("System.String"));
                dtData.Columns.Add("pro_qty", Type.GetType("System.String"));
                DataRow datarow = dtData.NewRow();
                datarow["pro_code"] = sQrCodeTmp[0]; // 批號
                datarow["lot_no"] = sQrCodeTmp[1];
                datarow["sor_code"] = sQrCodeTmp[2];
                datarow["tra_Code"] = sQrCodeTmp[3];
                datarow["qr_type"] = sQrCodeTmp[4];
                datarow["tracking_no"] = sQrCodeTmp[5];
                datarow["pro_qty"] = sQrCodeTmp[6];
                dtData.Rows.Add(datarow);
                ApiResponse.Data = dtData;
            }

            //Set_Apilog(sUsrCode, sFunName, data); // 寫入查詢紀錄
            return JsonConvert.SerializeObject(ApiResponse);

        }

        

        /// <summary>
        /// 回傳上料檔( WMT07_0000 )資料
        /// </summary>
        /// <param name="sToken"></param>
        /// <param name="sDate"></param>
        /// <param name="sMacCode"></param>
        /// <returns></returns>
        public string Get_PreparList(string sToken, string sDate, string sMacCode)
        {
            ApiResponse.Set_MesResponse("OK", "");
            string sFunName = "Get_PreparList";

            //輸入參數紀錄
            List<MacData> data = new List<MacData>() {
               new MacData(){ pToken = sToken, pMacCode = sMacCode , pDate =sDate }
            };

            //驗證token
            string sUsrCode = "";
            if (!Chk_Token(sToken))
            {
                ApiResponse.Set_MesResponse("NG", "Token驗證失敗");
            }
            else
            {
                sUsrCode = comm.Get_QueryData("BDP08_0000", sToken, "token", "usr_code");
            }

            //驗證機器號碼
            if (!comm.Chk_Basic("MEB29_0000", sMacCode, "mac_code")) ApiResponse.Set_MesResponse("NG", "無此機器號碼");
            if (!comm.Chk_DateForm(sDate)) ApiResponse.Set_MesResponse("NG", "日期格式錯誤[yyyy/MM/dd]");

            string sStationCode = Get_StationCodeByMacCode(sMacCode);
            string sWorkCode = Get_WorkCodeByStationCode(sStationCode);
            if (ApiResponse.Result == "OK")
            {

                string sSql = "SELECT a.chkup_code,a.prepare_code,a.mo_code,a.wrk_code,a.work_code" +
                     ",a.pro_code,n.pro_name,a.pro_qty,a.res_qty"+
                     "- ISNULL((SELECT  sum(pro_qty)  FROM WMT0200 WHERE  (pro_code = a.pro_code) AND  rel_type = '202' AND rel_code = a.wrk_code),0) as res_qty " +
                     ",a.scr_no,b.plan_line_code, " +
                     "  ISNULL((select sum(qty) as sum_qty from V_STO_QTY  where pro_code=a.pro_code) ,0) as inventory  " +
                     " FROM WMT07_0000 a" + " LEFT JOIN MET01_0000 b ON a.mo_code=b.mo_code" +
                     " LEFT JOIN MEB30_0100 c On a.work_code=c.work_code" +
                     " LEFT JOIN WMT06_0000 d ON a.prepare_code=d.prepare_code" +
                     " LEFT JOIN MET03_0000 e ON a.wrk_code=e.wrk_code " +
                     " LEFT JOIN MEB20_0000 n ON n.pro_code = a.pro_code " +
                     " WHERE c.station_code='" + sStationCode + "'" +
                     " AND d.prepare_date='" + sDate + "'" +
                     " AND e.mo_status <> 'END' " + " ORDER BY a.prepare_code,a.scr_no";

     //           string sSql = "SELECT a.chkup_code,a.prepare_code,a.mo_code,a.wrk_code,a.work_code" +
     //",a.pro_code,n.pro_name,a.pro_qty,a.res_qty,a.scr_no,b.plan_line_code, " +

     //"  ISNULL((select sum(qty) as sum_qty from V_STO_QTY  where pro_code=a.pro_code) ,0) as inventory  " +
     //" FROM WMT07_0000 a" + " LEFT JOIN MET01_0000 b ON a.mo_code=b.mo_code" +
     //" LEFT JOIN MEB30_0100 c On a.work_code=c.work_code" +
     //" LEFT JOIN WMT06_0000 d ON a.prepare_code=d.prepare_code" +
     //" LEFT JOIN MET03_0000 e ON a.wrk_code=e.wrk_code " +
     //" LEFT JOIN MEB20_0000 n ON n.pro_code = a.pro_code " +
     //" WHERE c.station_code='" + sStationCode + "'" +
     //" AND d.prepare_date='" + sDate + "'" +
     //" AND e.mo_status <> 'END' " + " ORDER BY a.prepare_code,a.scr_no";
                //           string sSql = "SELECT a.chkup_code,a.prepare_code,a.mo_code,a.wrk_code,a.work_code" +
                //",a.pro_code,n.pro_name,a.pro_qty,a.res_qty,a.scr_no,b.plan_line_code, " +
                //" ISNULL((select sum(pro_qty)  FROM MED06_0000 where mo_code= a.mo_code) ,0) -" +
                //" ISNULL((SELECT pro_qty FROM WMT0200 WHERE rel_type = '202' and pro_code=a.pro_code and rel_code = a.wrk_code),0)" +
                //" as pro_qty" +
                //"  ISNULL((select sum(qty) as sum_qty from V_STO_QTY  where pro_code=a.pro_code) ,0) as inventory  " +
                //" FROM WMT07_0000 a" + " LEFT JOIN MET01_0000 b ON a.mo_code=b.mo_code" +
                //" LEFT JOIN MEB30_0100 c On a.work_code=c.work_code" +
                //" LEFT JOIN WMT06_0000 d ON a.prepare_code=d.prepare_code" +
                //" LEFT JOIN MET03_0000 e ON a.wrk_code=e.wrk_code " +
                //" LEFT JOIN MEB20_0000 n ON n.pro_code = a.pro_code " +
                //" WHERE c.station_code='" + sStationCode + "'" +
                //" AND d.prepare_date='" + sDate + "'" +
                //" AND e.mo_status <> 'END' " + " ORDER BY a.prepare_code,a.scr_no";

                ApiResponse.Data = comm.Get_DataTable(sSql);
            }

            Set_Apilog(sUsrCode, sFunName, data);

            return JsonConvert.SerializeObject(ApiResponse);
        }

        /// <summary>
        /// 回傳途程工站關聯
        /// </summary>
        /// <param name="pStationCode"></param>
        /// <returns></returns>
        private string Get_WorkCodeByStationCode(string pStationCode)
        {
            // 輸入為空
            if (string.IsNullOrEmpty(pStationCode))
                return "";

            // 取得資料
            string sSql = " Select top 1 * " + " from MEB30_0100 " + " where station_code = '" + pStationCode + "'";
            DataTable dtTmp = comm.Get_DataTable(sSql);

            // 無資料
            if ((dtTmp.Rows.Count <= 0)) return "";
            else
            {
                DataColumnCollection columns = dtTmp.Columns;
                string work_code = dtTmp.Rows[0]["work_code"].ToString();
                return work_code;
            }
        }

        /// <summary>
        /// 回傳工站機器關聯
        /// </summary>
        /// <param name="pMacCode"></param>
        /// <returns></returns>
        private string Get_StationCodeByMacCode(string pMacCode)
        {

            // 輸入為空
            if (string.IsNullOrEmpty(pMacCode)) return "";

            // 取得資料
            string sSql = " Select top 1 * " + " from MEB29_0000 " + " where mac_code = '" + pMacCode.Trim() + "'";
            DataTable dtTmp = comm.Get_DataTable(sSql);

            return dtTmp.Rows.Count > 0 ? dtTmp.Rows[0]["station_code"].ToString() : "";
        }

        /// <summary>
        /// 回傳除外工時
        /// </summary>
        /// <param name="sToken">Token</param>
        /// <returns></returns>
        public string Get_ExceptCodeList(string sToken, string sMacCode = "")
        {
            ApiResponse.Set_MesResponse("OK", "");
            string sFunName = "Get_ExceptCodeList";

            //輸入參數紀錄
            List<MacData> data = new List<MacData>() {
               new MacData(){ pToken = sToken, pMacCode = sMacCode }
            };

            //驗證token
            string sUsrCode = "";
            if (!Chk_Token(sToken))
            {
                ApiResponse.Set_MesResponse("NG", "Token驗證失敗");
            }
            else
            {
                sUsrCode = comm.Get_QueryData("BDP08_0000", sToken, "token", "usr_code");
            }

            if (ApiResponse.Result == "OK")
            {
                string sSql = "SELECT except_code,except_name FROM MEB46_0000";
                ApiResponse.Data = comm.Get_DataTable(sSql);
            }

            Set_Apilog(sUsrCode, sFunName, data);

            return JsonConvert.SerializeObject(ApiResponse);

        }

        /// <summary>
        /// 回傳不良報工資料
        /// </summary>
        /// <param name="sToken">Token</param>
        /// <param name="sMacCode">機台代碼</param>
        /// <returns></returns>
        public string Get_NgCodeList(string sToken, string sMacCode)
        {
            ApiResponse.Set_MesResponse("OK", "");
            string sFunName = "Get_NgCodeList";

            //輸入參數紀錄
            List<MacData> data = new List<MacData>() {
               new MacData(){ pToken = sToken, pMacCode = sMacCode }
            };

            //驗證token
            string sUsrCode = "";
            if (!Chk_Token(sToken))
            {
                ApiResponse.Set_MesResponse("NG", "Token驗證失敗");
            }
            else
            {
                sUsrCode = comm.Get_QueryData("BDP08_0000", sToken, "token", "usr_code");
            }

            //驗證機器號碼
            if (!comm.Chk_Basic("MEB29_0000", sMacCode, "mac_code")) ApiResponse.Set_MesResponse("NG", "無此機器號碼");

            if (ApiResponse.Result == "OK")
            {
                string sSql = "SELECT Distinct e.ng_code,e.ng_name FROM MEB15_0000 a" +
                            " LEFT JOIN MEB29_0000 b On a.mac_code=b.mac_code" +
                            " LEFT JOIN MEB30_0100 c ON b.station_code=c.station_code" +
                            " LEFT JOIN MEB30_0300 d ON c.work_code=d.work_code" +
                            " LEFT JOIN MEB37_0000 e ON d.ng_code=e.ng_code" +
                            " WHERE a.mac_code=N'" + sMacCode + "'";

                ApiResponse.Data = comm.Get_DataTable(sSql);
            }

            Set_Apilog(sUsrCode, sFunName, data);

            return JsonConvert.SerializeObject(ApiResponse);

        }

        /// <summary>
        /// 回傳停機原因資料
        /// </summary>
        /// <param name="sToken">Token</param>
        /// <param name="sMacCode">機台代碼</param>
        /// <returns></returns>
        public string Get_StopCodeList(string sToken, string sMacCode)
        {
            ApiResponse.Set_MesResponse("OK", "");
            string sFunName = "Get_StopCodeList";

            //輸入參數紀錄
            List<MacData> data = new List<MacData>() {
               new MacData(){ pToken = sToken, pMacCode = sMacCode }
            };

            //驗證token
            string sUsrCode = "";
            if (!Chk_Token(sToken))
            {
                ApiResponse.Set_MesResponse("NG", "Token驗證失敗");
            }
            else
            {
                sUsrCode = comm.Get_QueryData("BDP08_0000", sToken, "token", "usr_code");
            }

            //驗證機器號碼
            if (!comm.Chk_Basic("MEB29_0000", sMacCode, "mac_code")) ApiResponse.Set_MesResponse("NG", "無此機器號碼");

            if (ApiResponse.Result == "OK")
            {
                string sSql = "SELECT c.stop_code,c.stop_name FROM MEB15_0000 a" +
                        " LEFT JOIN MEB45_0100 b On a.mac_code=b.mac_code" +
                        " LEFT JOIN MEB45_0000 c ON b.stop_code=c.stop_code" +
                        " WHERE a.mac_code='" + sMacCode + "'";
                ApiResponse.Data = comm.Get_DataTable(sSql);
            }
            Set_Apilog(sUsrCode, sFunName, data);

            return JsonConvert.SerializeObject(ApiResponse);
        }


        /// <summary>
        /// 檢查Token
        /// </summary>
        /// <param name="sToken">Token</param>
        /// <returns></returns>
        private bool Chk_Token(string sToken)
        {
            if (string.IsNullOrEmpty(sToken)) return false;
            string sSQL = "SELECT usr_code FROM BDP08_0000 WHERE token=N'" + sToken + "'";
            DataTable dTmp = comm.Get_DataTable(sSQL);
            return dTmp.Rows.Count > 0;
        }

        /// <summary>
        /// Set_ApiLog 存取紀錄
        /// </summary>
        /// <param name="pUsrCode"></param>
        /// <param name="pFunCode"></param>
        /// <param name="dtRec"></param>
        public void Set_Apilog(string pUsrCode, string pFunCode, object dtRec)
        {
            BDP27_0000 data_log = new BDP27_0000()
            {
                fun_code = pFunCode,
                search_paras = JsonConvert.SerializeObject(dtRec),
                message = ApiResponse.Message,
                result = ApiResponse.Result,
                data = JsonConvert.SerializeObject(ApiResponse.Data),
                ins_date = DateTime.Now.ToString("yyyy/MM/dd"),
                ins_time = DateTime.Now.ToString("HH:mm:ss"),
                usr_code = pUsrCode,
            };
            data_log.InsertData(data_log);
        }


        //以下未修改
        public string Ins_Login(string pToken, string JSON)
        {
            try
            {
                //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
                NameValueCollection postParams = HttpUtility.ParseQueryString(string.Empty);
                postParams.Add("Content-Type", "text/xml; charset=utf-8");
                postParams.Add("pToken", pToken);
                postParams.Add("JSON", JSON);

                //Console.WriteLine(postParams.ToString());// 將取得"version=1.0&action=preserveCodeCheck&pCode=pCode&TxID=guid&appId=appId", key和value會自動UrlEncode
                string sQueryString = postParams.ToString();

                string responseStr = Send_Request("Ins_Login", sQueryString);

                XmlDocument xml = new XmlDocument();

                xml.LoadXml(responseStr);

                string json = xml.InnerText;

                return json;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string Ins_Logout(string pToken, string JSON)
        {
            try
            {
                //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
                NameValueCollection postParams = HttpUtility.ParseQueryString(string.Empty);
                postParams.Add("Content-Type", "text/xml; charset=utf-8");
                postParams.Add("pToken", pToken);
                postParams.Add("JSON", JSON);

                //Console.WriteLine(postParams.ToString());// 將取得"version=1.0&action=preserveCodeCheck&pCode=pCode&TxID=guid&appId=appId", key和value會自動UrlEncode
                string sQueryString = postParams.ToString();

                string responseStr = Send_Request("Ins_Logout", sQueryString);

                XmlDocument xml = new XmlDocument();

                xml.LoadXml(responseStr);

                string json = xml.InnerText;

                return json;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        public string Get_UserToken(string pUsrCode, string pPassWord)
        {
            try
            {
                //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
                NameValueCollection postParams = HttpUtility.ParseQueryString(string.Empty);
                postParams.Add("Content-Type", "text/xml; charset=utf-8");
                postParams.Add("pUsrCode", pUsrCode);
                postParams.Add("pPassWord", pPassWord);

                //Console.WriteLine(postParams.ToString());// 將取得"version=1.0&action=preserveCodeCheck&pCode=pCode&TxID=guid&appId=appId", key和value會自動UrlEncode
                //要發送的字串轉為byte[] 
                string sQueryString = postParams.ToString();

                string responseStr = Send_Request("Get_UserToken", sQueryString);

                XmlDocument xml = new XmlDocument();

                xml.LoadXml(responseStr);

                string json = xml.InnerText;

                return json;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string Get_PerCodeListByMacCode(string pToken, string pMacCode)
        {
            try
            {

                //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
                NameValueCollection postParams = HttpUtility.ParseQueryString(string.Empty);
                postParams.Add("Content-Type", "text/xml; charset=utf-8");
                postParams.Add("pToken", pToken);
                postParams.Add("pMacCode", pMacCode);

                //Console.WriteLine(postParams.ToString());// 將取得"version=1.0&action=preserveCodeCheck&pCode=pCode&TxID=guid&appId=appId", key和value會自動UrlEncode
                //要發送的字串轉為byte[] 
                string sQueryString = postParams.ToString();

                string responseStr = Send_Request("Get_PerCodeListByMacCode", sQueryString);

                XmlDocument xml = new XmlDocument();

                xml.LoadXml(responseStr);

                string json = xml.InnerText;

                return json;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }



        public string Ins_MoEndData(string pToken, string JSON)
        {
            try
            {

                //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
                NameValueCollection postParams = HttpUtility.ParseQueryString(string.Empty);
                postParams.Add("Content-Type", "text/xml; charset=utf-8");
                postParams.Add("pToken", pToken);
                postParams.Add("JSON", JSON);

                //Console.WriteLine(postParams.ToString());// 將取得"version=1.0&action=preserveCodeCheck&pCode=pCode&TxID=guid&appId=appId", key和value會自動UrlEncode
                //要發送的字串轉為byte[] 
                string sQueryString = postParams.ToString();

                string responseStr = Send_Request("Ins_MoEndData", sQueryString);

                XmlDocument xml = new XmlDocument();

                xml.LoadXml(responseStr);

                string json = xml.InnerText;

                return json;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }
        public string Upd_MoStatus(string pToken, string JSON)
        {
            try
            {

                //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
                NameValueCollection postParams = HttpUtility.ParseQueryString(string.Empty);
                postParams.Add("Content-Type", "text/xml; charset=utf-8");
                postParams.Add("pToken", pToken);
                postParams.Add("JSON", JSON);

                //Console.WriteLine(postParams.ToString());// 將取得"version=1.0&action=preserveCodeCheck&pCode=pCode&TxID=guid&appId=appId", key和value會自動UrlEncode
                //要發送的字串轉為byte[] 
                string sQueryString = postParams.ToString();

                string responseStr = Send_Request("Upd_MoStatus", sQueryString);

                XmlDocument xml = new XmlDocument();

                xml.LoadXml(responseStr);

                string json = xml.InnerText;

                return json;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }
        public string Get_IOTProQty(string pToken, string pMacCode)
        {
            try
            {

                //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
                NameValueCollection postParams = HttpUtility.ParseQueryString(string.Empty);
                postParams.Add("Content-Type", "text/xml; charset=utf-8");
                postParams.Add("pToken", pToken);
                postParams.Add("pMacCode", pMacCode);

                //Console.WriteLine(postParams.ToString());// 將取得"version=1.0&action=preserveCodeCheck&pCode=pCode&TxID=guid&appId=appId", key和value會自動UrlEncode
                //要發送的字串轉為byte[] 
                string sQueryString = postParams.ToString();

                string responseStr = Send_Request("Get_IOTProQty", sQueryString);

                XmlDocument xml = new XmlDocument();

                xml.LoadXml(responseStr);

                string json = xml.InnerText;

                return json;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        public string Ins_ExceptData(string pToken, string JSON)
        {
            try
            {

                //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
                NameValueCollection postParams = HttpUtility.ParseQueryString(string.Empty);
                postParams.Add("Content-Type", "text/xml; charset=utf-8");
                postParams.Add("pToken", pToken);
                postParams.Add("JSON", JSON);

                //Console.WriteLine(postParams.ToString());// 將取得"version=1.0&action=preserveCodeCheck&pCode=pCode&TxID=guid&appId=appId", key和value會自動UrlEncode
                //要發送的字串轉為byte[] 
                string sQueryString = postParams.ToString();

                string responseStr = Send_Request("Ins_ExceptData", sQueryString);

                XmlDocument xml = new XmlDocument();

                xml.LoadXml(responseStr);

                string json = xml.InnerText;

                return json;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }



        private string Send_Request(string pFunc, string pQueryString)
        {
            // 參考: https://dotblogs.com.tw/shadow/2017/12/06/223813

            string APIUrl = ConfigurationManager.AppSettings["MES_API_URL"];
            string url = APIUrl + pFunc;
            //string url = "http://localhost:246/mes-api/mesapi.asmx/" + pFunc;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            //要發送的字串轉為byte[] 
            byte[] byteArray = Encoding.UTF8.GetBytes(pQueryString);
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }//end using

            //API回傳的字串
            string responseStr = "";
            //發出Request
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = sr.ReadToEnd();
                }//end using  
            }

            return responseStr;
        }


    }
}