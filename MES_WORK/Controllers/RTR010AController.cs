using MES_WORK.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MES_WORK.Controllers
{
    public class RTR010AController : Controller
    {
        Comm comm = new Comm();
        Work iWork = new Work();
        MesApi MesApi = new MesApi();
        public string pubToken = "b0daf10bb4df45b9a58c20ead5a94d98";

        // GET: MED010A
        public ActionResult Index()
        {

            ViewBag.Tabel = comm.Get_DataTable(comm.Get_strSQL("MEB48_0000", "s", "table_code", "MED01_0000"));
            return View();
        }

        /// <summary>
        /// 取得資料列表 (限定當天資料)
        /// </summary>
        /// <returns></returns>
        public DataTable Get_Data() {
            //在前端設定需要看到的欄位
            //這裡只負責抓資料

            string sMacCode = iWork.Get_MacCodeByMacAddress();

            DataTable dtDat = new DataTable();
            dtDat.Columns.Add("usr_code", System.Type.GetType("System.String"));
            dtDat.Columns.Add("usr_name", System.Type.GetType("System.String"));
            dtDat.Columns.Add("login_time", System.Type.GetType("System.String"));
            dtDat.Columns.Add("duration", System.Type.GetType("System.String"));

            string sSql = "select * from MED01_0100" +
                          " where date_s ='" + comm.Get_Date() + "'" +
                          "   and mac_code='" + comm.sGetString(sMacCode) + "'" +
                          "   and status='I'" +
                          "  order by date_s desc,time_s desc";
            var dtTmp = comm.Get_DataTable(sSql);

            int i;
            for (i = 0; i < dtTmp.Rows.Count; i++)
            {
                DataRow drow = dtDat.NewRow();
                drow["usr_code"] = dtTmp.Rows[i]["usr_code"];
                drow["usr_name"] = comm.Get_QueryData("BDP08_0000", dtTmp.Rows[i]["usr_code"].ToString(),"usr_code","usr_name");
                drow["login_time"] = dtTmp.Rows[i]["date_s"].ToString() + "　" + dtTmp.Rows[i]["time_s"].ToString();
                drow["duration"] = "";  //2筆資料的差異時間
                dtDat.Rows.Add(drow);
            }
            return dtDat;
        }
       


        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sPerCode = comm.sGetString(form["per_code"]);
            string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);

            if (iWork.Get_PerLoginStatus(sPerCode, sMacCode) =="O")
            {
                //沒有上工記錄，進行上工動作
                //寫入報工歷程檔 RowData
                MED01_0000 med01_0000 = new MED01_0000();
                comm.Set_ModelValue(med01_0000, form); //設定值
                med01_0000.mo_code = "";
                med01_0000.wrk_code = "";
                med01_0000.mac_code = comm.sGetString(sMacCode);
                med01_0000.ins_date = comm.Get_Date();
                med01_0000.ins_time = comm.Get_Time();
                med01_0000.usr_code = comm.sGetString(sPerCode);
                med01_0000.login_status = "I";
                med01_0000.InsertData(med01_0000);

                //寫入報工統計檔
                MED01_0100 med01_0100 = new MED01_0100();
                comm.Set_ModelValue(med01_0100, form);//設定值
                med01_0100.mo_code = "";
                med01_0100.wrk_code = "";
                med01_0100.mac_code = comm.sGetString(sMacCode);
                med01_0100.usr_code = comm.sGetString(sPerCode);
                med01_0100.date_s = comm.Get_Date();
                med01_0100.time_s = comm.Get_Time();
                med01_0100.date_e = "";
                med01_0100.time_e = "";
                med01_0100.status = "I";
                med01_0100.InsertData(med01_0100);
                med01_0000.UPD_MEM01_UsrCode(sMacCode, sPerCode, sMoCode);

            }
            else
            {
                //已有上工記錄，進行下工動作

                //寫入報工歷程檔 RowData
                MED01_0000 med01_0000 = new MED01_0000();
                comm.Set_ModelValue(med01_0000, form);
                med01_0000.mo_code = "";
                med01_0000.wrk_code = "";
                med01_0000.mac_code = comm.sGetString(sMacCode);
                med01_0000.ins_date = comm.Get_Date();
                med01_0000.ins_time = comm.Get_Time();
                med01_0000.usr_code = comm.sGetString(sPerCode);
                med01_0000.login_status = "O";
                med01_0000.InsertData(med01_0000);

                //回寫報工統計檔下工記錄
                string imed01_0100 = iWork.Get_med01_0100(sPerCode, sMacCode);
                iWork.Upd_PerLogOut(imed01_0100);
                med01_0000.UPD_MEM01_UsrCode(sMacCode, "", sMoCode);
            }
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
            
            switch (pID) {
                case "per_code":
                    if (pValue.Length > 20) {
                        sStatus = "N";
                        sMessage = "人員代號過長";
                        sIsSelect = true;
                    }
                    break;
            }

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

            //設定檢查點
            if (comm.Get_Data("BDP08_0000",form["per_code"],"usr_code","usr_code") == "") {
                sStatus = "N";
                sMessage = "無此人員代號";
            }

            //回傳格式
            sReturn = sStatus + "|" + sMessage;
            return sReturn;
        }
        public string Chk_test(FormCollection form)
        {

            string sReturn = ""; //回傳結果
            string sStatus = "";    //狀態 Y:通過 N:不通過 P:警示
            string sMessage = "";   //訊息

            //設定檢查點
            if (comm.Get_Data("BDP08_0000", form["per_code"], "usr_code", "usr_code") == "")
            {
                sStatus = "N";
                sMessage = "無此人員代號";
            }

            //回傳格式
            sReturn = sStatus + "|" + sMessage;
            return sReturn;
        }
    }
}