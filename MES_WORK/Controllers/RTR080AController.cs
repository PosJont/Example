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
    public class RTR080AController : Controller
    {
        Comm comm = new Comm();
        Work iWork = new Work();
        DynamicTable DT = new DynamicTable();
        GetData GD = new GetData();
        MesApi MesApi = new MesApi();
        public string pubToken = "b0daf10bb4df45b9a58c20ead5a94d98";

        // GET: MED010A
        public string PrgCode()
        {
            return ControllerContext.RouteData.Values["controller"].ToString();
        }

        public ActionResult Index()
        {
            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
            string sWrkCode = iWork.Get_WrkCodeByMacCode(sMacCode);
            string sProCode = comm.sGetString(comm.Get_QueryData("WMT07_0000", sWrkCode, "wrk_code", "pro_code"));
            string sProName = comm.sGetString(comm.Get_QueryData("MEB20_0000", sProCode, "pro_code", "pro_name"));
            string sMacName = comm.sGetString(comm.Get_QueryData("MEB15_0000", sMacCode,"mac_code","mac_name"));
            if (sWrkCode != "") {
                ViewBag.mac_code = sMacCode + "  " + sMacName;
                ViewBag.wrk_code = sWrkCode;
                ViewBag.pro_name = sProCode + "  " + sProName;
                ViewBag.urs_code = iWork.Get_UserCodeByMacCode(sMacCode);
            }
            DataTable sText = comm.Get_DataTable(comm.Get_strSQL("MEB48_0000", "s", "table_code", "MED06_0000"));
            ViewBag.Tabel = sText;
            ViewData["mac_code"] = ViewBag.mac_code;
            ViewData["wrk_code"] = ViewBag.wrk_code;
            ViewData["pro_name"] = ViewBag.pro_name;
            ViewData["urs_code"] = ViewBag.urs_code;
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
            string sWrkCode = iWork.Get_WrkCodeByMacCode(sMacCode);

            DataTable dtDat = new DataTable();
            dtDat.Columns.Add("pro_code", System.Type.GetType("System.String"));
            dtDat.Columns.Add("usr_name", System.Type.GetType("System.String"));
            dtDat.Columns.Add("ins_date", System.Type.GetType("System.String"));
            dtDat.Columns.Add("ng_qty", System.Type.GetType("System.String"));
            dtDat.Columns.Add("ng_code", System.Type.GetType("System.String"));
            string sSql = "Select ins_date, ins_time, pro_code, ng_qty,ng_code,end_usr_code " +
                          " From MED03_0000 " +
                          " where  wrk_code ='" + sWrkCode + "'" +
                          "        and mac_code='" + sMacCode + "'" +
                          "        and usr_code='" + sPerCode + "'" +
                          "  order by ins_date desc,ins_time desc";
            var dtTmp = comm.Get_DataTable(sSql);

            int i;
            for (i = 0; i < dtTmp.Rows.Count; i++)
            {
                DataRow drow = dtDat.NewRow();
                drow["pro_code"] = dtTmp.Rows[i]["pro_code"];
                drow["ins_date"] = dtTmp.Rows[i]["ins_date"].ToString() + "　" + dtTmp.Rows[i]["ins_time"].ToString();
                drow["ng_code"] = comm.Get_QueryData("MEB37_0000", dtTmp.Rows[i]["ng_code"].ToString(), "ng_code", "ng_name");
                drow["ng_qty"] = dtTmp.Rows[i]["ng_qty"];
                dtDat.Rows.Add(drow);
            }
            return dtDat;
        }



        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            //post_ng_code(form);
            //string dataStr = JsonConvert.SerializeObject(form);
            //string json = MesApi.Ins_MoNgData(pubToken, dataStr, form);
            //JObject obj_ins = JsonConvert.DeserializeObject<JObject>(json);
            //ViewBag.message = obj_ins["Message"].ToString();  ////錯誤訊息

            return RedirectToAction("Index");
        }
        public void post_ng_code(FormCollection form)
        {
            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sPerCode = iWork.Get_UserCodeByMacCode(sMacCode);
            string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
            string sWrkCode = iWork.Get_WrkCodeByMacCode(sMacCode);
            //object data = new object();

            string sNgCode = comm.sGetString(form["ng_code"]);
            string pro_Code = comm.Get_QueryData("MET01_0000", sMoCode, "mo_code", "pro_code");
            string ng_qty = comm.sGetString(form["pro_qty"]);


            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("mo_code", sMoCode);
            data.Add("wrk_code", sWrkCode);
            data.Add("mac_code", sMacCode);
            data.Add("pro_code", pro_Code);
            data.Add("per_code", sPerCode);
            data.Add("ng_code", sNgCode);
            data.Add("ng_qty", ng_qty);



            string dataStr = JsonConvert.SerializeObject(data);
            string json = MesApi.Ins_MoNgData(pubToken, dataStr,form);
            JObject obj_ins = JsonConvert.DeserializeObject<JObject>(json);
            ViewBag.message = obj_ins["Message"].ToString();  ////錯誤訊息



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


            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sPerCode = iWork.Get_UserCodeByMacCode(sMacCode);
            string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
            string sWrkCode = iWork.Get_WrkCodeByMacCode(sMacCode);
            //設定檢查點
            //if (comm.Get_Data("BDP08_0000", form["per_code"], "usr_code", "usr_code") == "")
            //{
            //    sStatus = "N";
            //    sMessage = "無此人員代號";
            //}
            if (sWrkCode == "")
            {
                sStatus = "N";
                sMessage = "尚未輸入派工單號";
            }

            //回傳格式
            sReturn = sStatus + "|" + sMessage;
            return sReturn;
        }
        public DataTable Get_ngCode()
        {
            //string sSql = "select * from MEB37_0000";
            //var dtTmp = comm.Get_DataTable(sSql);
            //return dtTmp;
            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sPerCode = iWork.Get_UserCodeByMacCode(sMacCode);
            string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
            DataTable dtTmp = new DataTable();

            DataTable dtDat = new DataTable();
            dtDat.Columns.Add("ng_code", System.Type.GetType("System.String"));
            dtDat.Columns.Add("ng_name", System.Type.GetType("System.String"));


            //從WEBAPI取值
            //string jsontmp = MesApi.Get_PreparList(pubToken, "2020/07/20", sMacCode);
            if (sMacCode == "")
            {
                return dtDat;
            }
            string jsontmp = MesApi.Get_NgCodeList(pubToken, sMacCode);
            JObject obj = JsonConvert.DeserializeObject<JObject>(jsontmp);
            if (obj["Result"].ToString() == "OK")
            {
                dtTmp = JsonConvert.DeserializeObject<DataTable>(obj["Data"].ToString());
            }

            int i;
            for (i = 0; i < dtTmp.Rows.Count; i++)
            {
                DataRow drow = dtDat.NewRow();
                drow["ng_code"] = comm.sGetString(dtTmp.Rows[i]["ng_code"].ToString());
                drow["ng_name"] = comm.sGetString(dtTmp.Rows[i]["ng_name"].ToString());

                dtDat.Rows.Add(drow);
            }

            return dtDat;

        }
        public bool Chk_ID()
        {
            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string tmpID = iWork.Get_UserCodeByMacCode(sMacCode);
            if (tmpID == "") { return false; }
            return true;
        }
    }
}