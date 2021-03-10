using MES_WORK.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;


namespace MES_WORK.Controllers
{
    public class RTR090AController : Controller
    {
        Comm comm = new Comm();
        Work iWork = new Work();
        GetData GD = new GetData();
        MesApi MesApi = new MesApi();
        public string pubToken = "b0daf10bb4df45b9a58c20ead5a94d98";

        // GET: MED010A
        public ActionResult Index()
        {
            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
            string sWrkCode = iWork.Get_WrkCodeByMacCode(sMacCode);
            string sProCode = Get_ProCode(sWrkCode);
            string sProName = comm.sGetString(comm.Get_QueryData("MEB20_0000", sProCode, "pro_code", "pro_name"));
            ViewBag.mo_qty = comm.Get_QueryData<decimal>("MET01_0000", sMoCode, "mo_code", "plan_qty");

            //string sProCode = comm.sGetString(comm.Get_QueryData("MET03_0000", sWrkCode, "wrk_code", "pro_code"));
            //string sProCode = comm.sGetString(comm.Get_QueryData("WMT07_0000", sWrkCode, "wrk_code", "pro_code"));
            //string sProName = comm.sGetString(comm.Get_QueryData("MEB20_0000", sProCode, "pro_code", "pro_name"));
            DataTable sText = comm.Get_DataTable(comm.Get_strSQL("MEB48_0000", "s", "table_code","MED09_0000"));
            if (sWrkCode != "") {
                ViewBag.wrk_code = sWrkCode;
                ViewBag.pro_code = sProCode;
                ViewBag.pro_name = sProName;
                ViewBag.res_qty = iWork.Get_ProQtyByMocode(sMoCode, sWrkCode);
            }
            ViewBag.Tabel = sText;
            ViewBag.test = sMoCode;
            ViewData["wrk_code"] = ViewBag.wrk_code;
            ViewData["pro_code"] = ViewBag.pro_code;
            ViewData["pro_name"] = ViewBag.pro_name;
            ViewData["mo_qty"] = ViewBag.mo_qty;
            ViewData["res_qty"] = ViewBag.res_qty;
           
            return View();
        }
        public string Get_ProCode(string sWrkCode) {
            string sSql = "SELECT MET01_0100.pro_code FROM MET01_0100 " +
                        " LEFT JOIN MET03_0000 p on p.mo_code = MET01_0100.mo_code and p.work_code = MET01_0100.work_code " +
                        " WHERE p.wrk_code ='"+ sWrkCode + "'";

            DataTable dTmp = comm.Get_DataTable(sSql);
            if (dTmp.Rows.Count > 0)
            {
                return dTmp.Rows[0][0].ToString();
            }
            return "";
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
            string sWrkCode = iWork.Get_WrkCodeByMacCode(sMacCode);

            DataTable dtDat = new DataTable();
            dtDat.Columns.Add("usr_code", System.Type.GetType("System.String"));
            dtDat.Columns.Add("usr_name", System.Type.GetType("System.String"));
            dtDat.Columns.Add("mo_time", System.Type.GetType("System.String"));
            dtDat.Columns.Add("mo_qty", System.Type.GetType("System.String"));
            dtDat.Columns.Add("lot_no", System.Type.GetType("System.String"));
            //dtDat.Columns.Add("pallet_code", System.Type.GetType("System.String"));

            string sSql = "select * from MED09_0000" +
                          " where wrk_code ='" + sWrkCode + "'" +
                          "   and mac_code='" + sMacCode + "'" +
                          "   and usr_code='" + sPerCode + "'" +
                          "   and usr_code!='' " +
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
                drow["lot_no"] = dtTmp.Rows[i]["pro_lot_no"];
                //drow["pallet_code"] = dtTmp.Rows[i]["pallet_code"];
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
            string pro_qty = comm.sGetString(form["pro_qty"].ToString());
            string pro_lot_no = comm.sGetString(form["lot_no"].ToString());
            //string pallet_code = comm.sGetString(form["pallet_code"].ToString());

            //string pro_code = comm.sGetString(form["pro_code"].ToString());
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("mo_code", sMoCode);
            data.Add("wrk_code", sWrkCode);
            data.Add("mac_code", sMacCode);
            data.Add("per_code", sPerCode);
            data.Add("pro_qty", pro_qty);
            data.Add("pro_lot_no", pro_lot_no);
            //data.Add("pallet_code", pallet_code);

            //data.Add("pro_code", pro_code);

            string dataStr = JsonConvert.SerializeObject(data);
            //寫入上MED09_0000，call Ins_ProQtyData API
            string json = MesApi.Ins_ProQtyData(pubToken, dataStr, form);
            JObject obj_ins = JsonConvert.DeserializeObject<JObject>(json);
            
            //GET med09_0000 KEY AND UPDATE MED090A DATA
            string sSql = "select top(1) med09_0000 from MED09_0000  where 1 = 1  order by med09_0000 desc ";
            DataTable dtmp = comm.Get_DataTable(sSql);
            MED09_0000 med09_0000 = new MED09_0000();
            comm.Set_ModelValue(med09_0000, form);
            med09_0000.med09_0000 = int.Parse(dtmp.Rows[0]["med09_0000"].ToString());
            med09_0000.UpdateData(med09_0000);


            //錯誤訊息
            ViewBag.message = obj_ins["Message"].ToString();

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
        public bool Chk_ID()
        {
            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string tmpID = iWork.Get_UserCodeByMacCode(sMacCode);
            if (tmpID != "") { return true; }
            else return false;
        }
    }
}