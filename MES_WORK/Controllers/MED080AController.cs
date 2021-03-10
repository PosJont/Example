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
    public class MED080AController : Controller
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

        //public string pubMacCode()
        //{
        //    return iWork.Get_MacCodeByMacAddress();
        //}
        //public string pubPerCode()
        //{
        //    return iWork.Get_UserCodeByMacCode(pubMacCode());
        //}
        //public string pubMoCode()
        //{
        //    return iWork.Get_MoCodeByMacCode(pubMacCode());
        //}
        //public string pubWorkCode()
        //{
        //    return iWork.Get_WrkCodeByMacCode(pubMacCode());
        //}

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

            /**/string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sPerCode = iWork.Get_UserCodeByMacCode(sMacCode);
            string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
            string sWrkCode = iWork.Get_WrkCodeByMacCode(sMacCode);
            string sNgCode = comm.sGetString(form["ng_code"]);
            //寫入生產資料
            


            return RedirectToAction("Index");
        }
        public void post_ng_code(FormCollection form)
        {
            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sPerCode = iWork.Get_UserCodeByMacCode(sMacCode);
            string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
            string sWrkCode = iWork.Get_WrkCodeByMacCode(sMacCode);
            //object data = new object();
            string user_field_01 = form["user_field_01"];
            string user_field_02 = form["user_field_02"];
            string user_field_03 = form["user_field_03"];
            string user_field_04 = form["user_field_04"];
            string user_field_05 = form["user_field_05"];
            string user_field_06 = form["user_field_06"];
            string user_field_07 = form["user_field_07"];
            string user_field_08 = form["user_field_08"];
            string user_field_09 = form["user_field_09"];
            string user_field_10 = form["user_field_10"];
            string sNgCode = comm.sGetString(form["ng_code"]);
            string pro_Code = comm.Get_QueryData("MET01_0000", sMoCode, "mo_code", "pro_code");
            string ng_qty = comm.sGetString(form["pro_qty"]);
            if (form["user_field_01"] == null) user_field_01 = "";
            if (form["user_field_02"] == null) user_field_02 = "";
            if (form["user_field_03"] == null) user_field_03 = "";
            if (form["user_field_04"] == null) user_field_04 = "";
            if (form["user_field_05"] == null) user_field_05 = "";
            if (form["user_field_06"] == null) user_field_06 = "";
            if (form["user_field_07"] == null) user_field_07 = "";
            if (form["user_field_08"] == null) user_field_08 = "";
            if (form["user_field_09"] == null) user_field_09 = "";
            if (form["user_field_10"] == null) user_field_10 = "";

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("mo_code", sMoCode);
            data.Add("wrk_code", sWrkCode);
            data.Add("mac_code", sMacCode);
            data.Add("pro_code", pro_Code);
            data.Add("per_code", sPerCode);
            data.Add("ng_code", sNgCode);
            data.Add("ng_qty", ng_qty);
            //string user_field_01 = null;
            data.Add("user_field_01", user_field_01);
            data.Add("user_field_02", user_field_02);
            data.Add("user_field_03", user_field_03);
            data.Add("user_field_04", user_field_04);
            data.Add("user_field_05", user_field_05);
            data.Add("user_field_06", user_field_06);
            data.Add("user_field_07", user_field_07);
            data.Add("user_field_08", user_field_08);
            data.Add("user_field_09", user_field_09);
            data.Add("user_field_10", user_field_10);



            string dataStr = JsonConvert.SerializeObject(data);
            string json = MesApi.Ins_MoNgData(pubToken, dataStr);
            JObject obj_ins = JsonConvert.DeserializeObject<JObject>(json);
            ViewBag.message = obj_ins["Message"].ToString();  ////錯誤訊息


            //MED03_0000 data = new MED03_0000();
            //data.mo_code = pubMoCode();
            //data.wrk_code = pubWorkCode();
            //data.work_code = "";
            //data.station_code = "";
            //data.mac_code = pubMacCode();
            //data.pro_code = pro_Code;
            //data.ng_code = sNgCode;
            //data.pro_lot_no = "";
            //data.ng_qty =  comm.sGetfloat(ng_qty);
            //data.ins_date = DateTime.Now.ToString("yyyy/MM/dd");
            //data.ins_time = DateTime.Now.ToString("HH:mm:ss");
            //data.usr_code = pubPerCode();
            //data.des_memo = "";
            //data.is_ng = "N";
            //data.is_end = "Y";
            //data.end_memo = "";
            //data.end_date = "";
            //data.end_time = "";
            //data.end_usr_code = "";
            //data.InsertData(data);

            //data = new
            //{
            //    mo_code = pubMoCode(),
            //    wrk_code = pubWorkCode(),
            //    mac_code = pubMacCode(),
            //    pro_code = pro_Code,
            //    ng_code = sNgCode,
            //    pro_lot_no = "",
            //    ng_qty = ng_qty,
            //    ins_date = DateTime.Now.ToString("yyyy/MM/dd"),
            //    ins_time = DateTime.Now.ToString("HH:mm:ss"),
            //    usr_code = pubPerCode(),
            //    des_memo = "",
            //    is_ng = "Y",
            //    is_end = "N",
            //    end_memo = "",
            //    end_date = "",
            //    end_time = "",
            //    end_usr_code = "",
            //};                  
            //DT.InsertData("MED03_0000", data);
            //float now_ng = comm.sGetfloat(ng_qty);
            //string sSql = "select ng_qty " +
            //             "from MEM01_0000  " +
            //             " where mo_code = '" + pubMoCode() + "' " +
            //             "       and mac_code ='" + pubMacCode() + "' ";
            //var dtDat = comm.Get_DataTable(sSql);
            //float tmp_ng = comm.sGetfloat(dtDat.Rows[0]["ng_qty"].ToString());
            //string uSql = "update MEM01_0000 " +
            //            "  set ng_qty = '" + (tmp_ng + now_ng) + "'" +
            //            " where mo_code = '" + pubMoCode() + "' " +
            //            " and mac_code ='" + pubMacCode() + "' ";
            //comm.Connect_DB(uSql);


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