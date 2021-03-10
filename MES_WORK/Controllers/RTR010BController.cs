using MES_WORK.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MES_WORK.Controllers
{
    public class RTR010BController : Controller
    {
        Comm comm = new Comm();
        Work iWork = new Work();

        // GET: MED010A
        public ActionResult Index()
        {
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


            //string sMacCode = "";
            //if (Request.Cookies["MacCode"] == null)
            //{
            //    sMacCode = "";
            //}
            //else {
            //    sMacCode = Request.Cookies["MacCode"].Value;
            //}

            DataTable dtDat = new DataTable();
            dtDat.Columns.Add("usr_code", System.Type.GetType("System.String"));
            dtDat.Columns.Add("usr_name", System.Type.GetType("System.String"));
            dtDat.Columns.Add("login_time", System.Type.GetType("System.String"));
            dtDat.Columns.Add("duration", System.Type.GetType("System.String"));

            string sSql = "select * from MED01_0000" +
                          " where ins_date ='" + comm.Get_Date() + "'" +
                          "   and mac_code='" + comm.sGetString(sMacCode) + "'" +
                          "  order by ins_date desc,ins_time desc";
            var dtTmp = comm.Get_DataTable(sSql);

            int i;
            for (i = 0; i < dtTmp.Rows.Count; i++)
            {
                DataRow drow = dtDat.NewRow();
                drow["usr_code"] = dtTmp.Rows[i]["usr_code"];
                drow["usr_name"] = comm.Get_QueryData("BDP08_0000", dtTmp.Rows[i]["usr_code"].ToString(), "usr_code", "usr_name");
                drow["login_time"] = dtTmp.Rows[i]["ins_date"].ToString() + "　" + dtTmp.Rows[i]["ins_time"].ToString();
                drow["duration"] = "";  //2筆資料的差異時間
                dtDat.Rows.Add(drow);
            }

            return dtDat;
        }



        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            string sMacCode = "";
            if (Request.Cookies["MacCode"] != null)
            {
                sMacCode = Request.Cookies["MacCode"].Value;
            }

            //寫入報工歷程檔 RowData
            MED01_0000 med01_0000 = new MED01_0000();
            comm.Set_ModelValue(med01_0000, form);
            med01_0000.mo_code = "";
            med01_0000.wrk_code = "";
            med01_0000.mac_code = comm.sGetString(sMacCode);
            med01_0000.ins_date = comm.Get_Date();
            med01_0000.ins_time = comm.Get_Time();
            med01_0000.usr_code = comm.sGetString(form["per_code"]);
            med01_0000.login_status = "O";
            med01_0000.InsertData(med01_0000);

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

            switch (pID)
            {
                case "per_code":
                    if (pValue.Length > 20)
                    {
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