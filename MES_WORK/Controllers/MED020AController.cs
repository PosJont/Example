using MES_WORK.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MES_WORK.Controllers
{
    public class MED020AController : Controller
    {
        Comm comm = new Comm();
        Work iWork = new Work();

        // GET: MED020A
        public ActionResult Index()
        {
            ViewBag.Tabel = comm.Get_DataTable(comm.Get_strSQL("MEB48_0000", "s", "table_code", "MED02_0000"));
            return View();
        }

        /// <summary>
        /// 取得資料列表 (限定當天資料)
        /// </summary>
        /// <returns></returns>
        public DataTable Get_Data()
        {
            //在前端設定需要看到的欄位
            //查詢預計開工日在當天的工單
            //資料邏輯MES API
            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sUsrCode = iWork.Get_UserCodeByMacCode(sMacCode);
            string sStation = iWork.Get_StationCodeByMacCode(sMacCode);
            string sWorkCode = iWork.Get_WorkCodeByStationCode(sStation);
            
            DataTable dtDat = new DataTable();
            dtDat.Columns.Add("scr_no", System.Type.GetType("System.String"));
            dtDat.Columns.Add("mo_code", System.Type.GetType("System.String"));
            dtDat.Columns.Add("sch_datetime_s", System.Type.GetType("System.String"));
            dtDat.Columns.Add("wrk_code", System.Type.GetType("System.String"));
            dtDat.Columns.Add("pro_name", System.Type.GetType("System.String"));
            dtDat.Columns.Add("mo_status_wrk", System.Type.GetType("System.String"));

            string sSql = "SELECT a.mo_code,a.wrk_code,a.pro_code,c.pro_name,a.pro_qty AS plan_qty,a.mo_status,isnull(d.sch_date_s+' '+d.sch_time_s,'')  as sch_datetime_s " +
                    " FROM MET03_0000 a" +
                    "      LEFT JOIN MEB30_0100 b On a.work_code=b.work_code" +
                    "      LEFT JOIN MEB29_0200 e ON b.station_code=e.station_code"+      
                    "      LEFT JOIN MEB20_0000 c ON a.pro_code=c.pro_code" +
                    "      LEFT JOIN MET01_0000 d ON a.mo_code=d.mo_code" +
                    " WHERE e.mac_code='" + comm.sGetString(sMacCode) + "'" +
                    "       AND d.mo_status IN ('20','30')" +
                    "       AND a.mo_status IN ('STOP','NONE')" +
                    "       AND a.wrk_date<='" + comm.Get_Date() + "'";
            var dtTmp = comm.Get_DataTable(sSql);

        
            for (int i = 0; i < dtTmp.Rows.Count; i++)
            {
                DataRow drow = dtDat.NewRow();
                drow["scr_no"] = (i+1).ToString();
                drow["mo_code"] = dtTmp.Rows[i]["mo_code"].ToString();
                drow["sch_datetime_s"] = dtTmp.Rows[i]["sch_datetime_s"].ToString();
                drow["wrk_code"] = dtTmp.Rows[i]["wrk_code"].ToString();
                drow["pro_name"] = comm.Get_QueryData("MEB20_0000", dtTmp.Rows[i]["pro_code"].ToString(), "pro_code", "pro_name");
                drow["mo_status_wrk"] = dtTmp.Rows[i]["mo_status"].ToString(); 
                dtDat.Rows.Add(drow);
            }
            return dtDat;
        }



        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sWrkCode = comm.sGetString(form["wrk_code"]);
            string sMoCode = comm.Get_QueryData("MET03_0000", sWrkCode, "wrk_code", "mo_code");
            string station_Code = comm.Get_QueryData("MEB29_0200", sMacCode, "mac_code", "station_code");
            string work_code = comm.Get_QueryData("MEB30_0100", station_Code, "station_code", "work_code");
            string sUsrCode = iWork.Get_UserCodeByMacCode(sMacCode);
            //寫入報工歷程檔 RowData
            MED02_0000 med02_0000 = new MED02_0000();
            comm.Set_ModelValue(med02_0000, form);
            med02_0000.mo_code = sMoCode;
            med02_0000.wrk_code = sWrkCode;
            med02_0000.mac_code = comm.sGetString(sMacCode);
            med02_0000.ins_date = comm.Get_Date();
            med02_0000.ins_time = comm.Get_Time();
            med02_0000.usr_code = comm.sGetString(sUsrCode);
            med02_0000.mo_status_wrk = "IN";
            med02_0000.des_memo = "";
            med02_0000.is_ng = "N";
            med02_0000.is_end = "N";
            med02_0000.end_memo = "";
            med02_0000.end_date = "";
            med02_0000.end_time = "";
            med02_0000.end_usr_code = "";

            med02_0000.InsertData(med02_0000);

            //更新派工單狀態 為開工中
            iWork.Upd_WrkStatus(sWrkCode, "IN");

            //更新開工時間
            iWork.Upd_MEM01Data(sMoCode, work_code, "work_time_s", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            iWork.Upd_MEM01Data(sMoCode, work_code, "mac_code", sMacCode);
            iWork.Upd_MEM01Data(sMoCode, work_code, "station_code", station_Code);
            iWork.Upd_MEM01Data(sMoCode, work_code, "usr_code", sUsrCode);

            //若MET01_0000中有開工時間則不更新,若如無開工時間則更新時間
            if (!Chk_MET010000_MoStartDate(sMoCode))
            {
                med02_0000.Upd_MET01_0000_MoStartDate(sMoCode);
            }

            MED08_0000 med08_0000 = new MED08_0000();
            comm.Set_ModelValue(med08_0000, form);
            med08_0000.mo_code = sMoCode;
            med08_0000.wrk_code = sWrkCode;
            med08_0000.mac_code = comm.sGetString(sMacCode);
            
          
            med08_0000.date_s = DateTime.Now.ToString("yyyy/MM/dd");
            med08_0000.time_s = DateTime.Now.ToString("HH:mm:ss");
            med08_0000.des_memo = "";
            med08_0000.is_ng = "N";
            med08_0000.is_end = "N";
            med08_0000.end_memo = "";
            med08_0000.end_date = "";
            med08_0000.end_time = "";
            med08_0000.end_usr_code = "";
            med08_0000.user_field_01 = form["user_field_01"];
            med08_0000.user_field_02 = form["user_field_02"];
            med08_0000.user_field_03 = form["user_field_03"];
            med08_0000.user_field_04 = form["user_field_04"];
            med08_0000.user_field_05 = form["user_field_05"];
            med08_0000.user_field_06 = form["user_field_06"];
            med08_0000.user_field_07 = form["user_field_07"];
            med08_0000.user_field_08 = form["user_field_08"];
            med08_0000.user_field_09 = form["user_field_09"];
            med08_0000.user_field_10 = form["user_field_10"];
            if (form["user_field_01"] == null) med08_0000.user_field_01 = "";
            if (form["user_field_02"] == null) med08_0000.user_field_02 = "";
            if (form["user_field_03"] == null) med08_0000.user_field_03 = "";
            if (form["user_field_04"] == null) med08_0000.user_field_04 = "";
            if (form["user_field_05"] == null) med08_0000.user_field_05 = "";
            if (form["user_field_06"] == null) med08_0000.user_field_06 = "";
            if (form["user_field_07"] == null) med08_0000.user_field_07 = "";
            if (form["user_field_08"] == null) med08_0000.user_field_08 = "";
            if (form["user_field_09"] == null) med08_0000.user_field_09 = "";
            if (form["user_field_10"] == null) med08_0000.user_field_10 = "";
            med08_0000.InsertData(med08_0000);
            ////新增生產途程紀錄
            //MEM01_0000 mem01_0000 = new MEM01_0000();
            //mem01_0000.mo_code = sMoCode;
            //mem01_0000.work_code = work_code;
            //mem01_0000.station_code = station_Code;
            //mem01_0000.mac_code = comm.sGetString(sMacCode);
            //mem01_0000.work_time_s = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            //mem01_0000.work_time_e = "";
            //mem01_0000.ok_qty = 0;
            //mem01_0000.ng_qty = 0;
            //mem01_0000.ng_unit = "";
            //mem01_0000.ok_unit = "";
            //mem01_0000.work_sec = 0;
            //mem01_0000.usr_code = sUsrCode;
            //mem01_0000.InsertData(mem01_0000);



            return RedirectToAction("Index");
        }


        /// <summary>
        /// 欄位輸入完後的檢查函式
        /// </summary>
        /// <param name="Id">控制項的ID</param>
        /// <param name="Value">控制項輸入的值</param>
        /// <returns></returns>
        public string Chk_Input(string pID, string pValue)
        {
            string sReturn = ""; //回傳結果
            string sStatus = "";    //狀態 Y:通過 N:不通過 P:警示
            string sMessage = "";   //訊息
            bool sIsSelect = false; //是否要反白控制項

            //switch (pID)
            //{
            //    case "per_code":
            //        if (pValue.Length > 20)
            //        {
            //            sStatus = "N";
            //            sMessage = "人員代號過長";
            //            sIsSelect = true;
            //        }
            //        break;
            //}

            //回傳格式
            sReturn = sStatus + "|" + sMessage + "|" + sIsSelect.ToString();
            return sReturn;
        }

        /// <summary>
        /// 按下存檔後的檢查函式
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public string Chk_Submit(FormCollection form)
        {

            string sReturn = ""; //回傳結果
            string sStatus = "";    //狀態 Y:通過 N:不通過 P:警示
            string sMessage = "";   //訊息

            ////設定檢查點
            //if (comm.Get_Data("BDP08_0000", form["per_code"], "usr_code", "usr_code") == "")
            //{
            //    sStatus = "N";
            //    sMessage = "無此人員代號";
            //}

            //回傳格式
            sReturn = sStatus + "|" + sMessage;
            return sReturn;
        }

        //public string Get_ProName(string pWrkCode)
        //{
        //    string sReturn = "";
        //    sReturn = comm.Get_QueryData("MET03_0000", pWrkCode, "wrk_code", "pro_code");
        //    sReturn = comm.Get_QueryData("MEB20_0000", sReturn, "pro_code", "pro_name");
        //    return sReturn;
        //}
        public bool Chk_ID()
        {
            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string tmpID = iWork.Get_UserCodeByMacCode(sMacCode);
            if (tmpID == "") { return false; }
            return true;
        }
        public bool Chk_MET010000_MoStartDate(string pMoCode)
        {
            string sSql = "";
            DataTable dtTmp = new DataTable();
            sSql = "Select mo_start_date from MET01_0000 where mo_code='" + pMoCode + "'";
            dtTmp = comm.Get_DataTable(sSql);
            if (dtTmp.Rows.Count > 0)
            {
                if (dtTmp.Rows[0]["mo_start_date"].ToString() == "")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return true;
        }

    }
}