using MES_WORK.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MES_WORK.Controllers
{
    public class RTR030AController : Controller
    {
        Comm comm = new Comm();
        Work iWork = new Work();

        // GET: MED010A
        public ActionResult Index()
        {
            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
            string sWrkCode = iWork.Get_WrkCodeByMacCode(sMacCode);

            ViewBag.mo_code = sMoCode;
            ViewBag.pro_name = comm.sGetString(comm.Get_QueryData("MET01_0000", sMoCode, "mo_code", "pro_code"));
            ViewBag.mo_qty = comm.Get_QueryData<decimal>("MET01_0000", sMoCode, "mo_code", "plan_qty");
            ViewBag.res_qty =iWork.Get_ProQtyByMocode(sMoCode, sWrkCode);
            ViewBag.Tabel = comm.Get_DataTable(comm.Get_strSQL("MEB48_0000", "s", "table_code", "MED03_0000"));
            return View();
        }

        /// <summary>
        /// 取得資料列表 (限定當天資料)
        /// </summary>
        /// <returns></returns>
        public DataTable Get_Data()
        {
            //在前端設定需要看到的欄位
            //這裡只負責抓資料

            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sPerCode = iWork.Get_UserCodeByMacCode(sMacCode);
            string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);

            DataTable dtDat = new DataTable();
            dtDat.Columns.Add("usr_code", System.Type.GetType("System.String"));
            dtDat.Columns.Add("usr_name", System.Type.GetType("System.String"));
            dtDat.Columns.Add("mo_time", System.Type.GetType("System.String"));
            dtDat.Columns.Add("mo_qty", System.Type.GetType("System.String"));

            string sSql = "select * from MED09_0000" +
                          " where mo_code ='" + sMoCode + "'" +
                          "   and mac_code='" + sMacCode + "'" +
                          "   and usr_code='" + sPerCode + "'" +
                          "  order by ins_date desc,ins_time desc";
            var dtTmp = comm.Get_DataTable(sSql);

            int i;
            for (i = 0; i < dtTmp.Rows.Count; i++)
            {
                DataRow drow = dtDat.NewRow();
                drow["usr_code"] = dtTmp.Rows[i]["usr_code"];
                drow["usr_name"] = comm.Get_QueryData("BDP08_0000", dtTmp.Rows[i]["usr_code"].ToString(), "usr_code", "usr_name");
                drow["mo_time"] = dtTmp.Rows[i]["ins_date"].ToString() + "　" + dtTmp.Rows[i]["ins_time"].ToString();
                drow["mo_qty"] = dtTmp.Rows[i]["pro_qty"];
                dtDat.Rows.Add(drow);
            }
            return dtDat;
        }



        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sPerCode = iWork.Get_UserCodeByMacCode(sMacCode);
            string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
            string sWrkCode = iWork.Get_WrkCodeByMacCode(sMacCode);

            //寫入生產資料
            MED09_0000 med09_0000 = new MED09_0000();
            comm.Set_ModelValue(med09_0000, form);
            med09_0000.mo_code = sMoCode;
            med09_0000.wrk_code = sWrkCode;
            med09_0000.mac_code = comm.sGetString(sMacCode);
            med09_0000.pro_code = comm.Get_QueryData("MET01_0000", sMoCode, "mo_code", "pro_code");
            med09_0000.pro_lot_no = "";
            med09_0000.pro_qty = comm.sGetDouble(form["pro_qty"].ToString());
            med09_0000.ins_date = comm.Get_Date();
            med09_0000.ins_time = comm.Get_Time();
            med09_0000.usr_code = sPerCode;
            med09_0000.des_memo = "";
            med09_0000.is_ng = "N";
            med09_0000.is_end = "N";
            med09_0000.end_memo = "";
            med09_0000.end_date = "";
            med09_0000.end_time = "";
            med09_0000.end_usr_code = "";
            med09_0000.InsertData(med09_0000);

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
            //    case "pro_qty":
            //        if ( pValue > 20)
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

            //設定檢查點
            //if (comm.Get_Data("BDP08_0000", form["per_code"], "usr_code", "usr_code") == "")
            //{
            //    sStatus = "N";
            //    sMessage = "無此人員代號";
            //}

            //回傳格式
            sReturn = sStatus + "|" + sMessage;
            return sReturn;
        }
    }
}