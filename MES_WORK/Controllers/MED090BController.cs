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
    public class MED090BController : Controller
    {
        Comm comm = new Comm();
        Work iWork = new Work();
        GetData GD = new GetData();
        MesApi MesApi = new MesApi();
        public string pubToken = "b0daf10bb4df45b9a58c20ead5a94d98";

        // GET: MED010A
        public ActionResult Index()
        {
            //string sMacCode = iWork.Get_MacCodeByMacAddress();
            //string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
            //string sProCode = comm.sGetString(comm.Get_QueryData("MET01_0000", sMoCode, "mo_code", "pro_code"));
            //string sProName = comm.sGetString(comm.Get_QueryData("MEB20_0000", sProCode, "pro_code", "pro_name"));
            //ViewBag.mo_code = sMoCode;
            //ViewBag.pro_name =sProCode +"  "+sProName ;
            //ViewBag.mo_qty = comm.Get_QueryData<decimal>("MET01_0000", sMoCode, "mo_code", "plan_qty");
            //ViewBag.res_qty = iWork.Get_ProQtyByMocode(sMoCode);

            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sPerCode = iWork.Get_UserCodeByMacCode(sMacCode);
            string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
            string sWrkCode = iWork.Get_WrkCodeByMacCode(sMacCode);
            string pro_code= comm.Get_QueryData("WMT07_0000", sWrkCode, "wrk_code", "pro_code");
            string pro_name = comm.Get_Data("MEB20_0000", pro_code, "pro_code", "pro_name");
            string plan_qty = GD.Get_Data("WMT07_0000", sWrkCode, "wrk_code", "pro_qty");

            DataTable dtTmp = new DataTable();
            dtTmp.Columns.Add("pro_qty", System.Type.GetType("System.String"));
            //從WEBAPI取值
            string jsontmp = MesApi.Get_IOTProQty(pubToken, sMacCode);
            JObject obj = JsonConvert.DeserializeObject<JObject>(jsontmp);
            if (obj["Result"].ToString() == "OK")
            {
                dtTmp = JsonConvert.DeserializeObject<DataTable>(obj["Data"].ToString());
                string pro_qty = comm.sGetString(dtTmp.Rows[0]["pro_qty"].ToString());
                ViewBag.pro_qty = pro_qty;
                ViewBag.pro_code = pro_code;
                ViewBag.pro_name = pro_code +" "+pro_name;
                ViewBag.plan_qty = plan_qty;
            }
            ViewBag.mo_code = sWrkCode;





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

            //string sMacCode = iWork.Get_MacCodeByMacAddress();
            //string sPerCode = iWork.Get_UserCodeByMacCode(sMacCode);
            //string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);

            DataTable dtDat = new DataTable();
            //dtDat.Columns.Add("usr_code", System.Type.GetType("System.String"));
            //dtDat.Columns.Add("usr_name", System.Type.GetType("System.String"));
            //dtDat.Columns.Add("mo_time", System.Type.GetType("System.String"));
            //dtDat.Columns.Add("mo_qty", System.Type.GetType("System.String"));
            //dtDat.Columns.Add("lot_no", System.Type.GetType("System.String"));

            //string sSql = "select * from MED09_0000" +
            //              " where mo_code ='" + sMoCode + "'" +
            //              "   and mac_code='" + sMacCode + "'" +
            //              "   and usr_code='" + sPerCode + "'" +
            //              "   and usr_code!='' " +
            //              "  order by ins_date desc,ins_time desc";
            //var dtTmp = comm.Get_DataTable(sSql);

            //int i;
            //for (i = 0; i < dtTmp.Rows.Count; i++)
            //{
            //    DataRow drow = dtDat.NewRow();
            //    drow["usr_code"] = dtTmp.Rows[i]["usr_code"];
            //    drow["usr_name"] = comm.Get_QueryData("BDP08_0000", dtTmp.Rows[i]["usr_code"].ToString(), "usr_code", "usr_name");
            //    drow["mo_time"] = dtTmp.Rows[i]["ins_date"].ToString() + "　" + dtTmp.Rows[i]["ins_time"].ToString();
            //    drow["mo_qty"] = dtTmp.Rows[i]["pro_qty"];
            //    drow["lot_no"] = dtTmp.Rows[i]["pro_lot_no"];
            //    dtDat.Rows.Add(drow);
            //}
            return dtDat;
        }



        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            string sMsg = "";
            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sPerCode = iWork.Get_UserCodeByMacCode(sMacCode);
            string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
            string sWrkCode = iWork.Get_WrkCodeByMacCode(sMacCode);
            string pro_qty = comm.sGetString(form["pro_qty"].ToString());
            string ng_qty = comm.sGetString(form["ng_qty"].ToString());
            string pro_code = comm.sGetString(form["pro_code"].ToString());
            //更新工單生產數量
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("mo_code", sMoCode);
            data.Add("wrk_code", sWrkCode);
            data.Add("mac_code", sMacCode);
            data.Add("per_code", sPerCode);
            data.Add("pro_qty", pro_qty);

            //更新生產數量
            string dataStr = JsonConvert.SerializeObject(data);
            string json = MesApi.Ins_ProQtyData_Api(pubToken, dataStr);
            JObject obj_ins = JsonConvert.DeserializeObject<JObject>(json);
            sMsg = sMsg+"生產數量訊息"+ obj_ins["Message"].ToString();  ////錯誤訊息

            //更新不良品數量
            data.Clear();
            json = "";
            dataStr = "";
            obj_ins.Values("");
            data.Add("mo_code", sMoCode);
            data.Add("wrk_code", sWrkCode);
            data.Add("mac_code", sMacCode);
            data.Add("pro_code", pro_code);
            data.Add("per_code", sPerCode);
            data.Add("ng_code", "");
            data.Add("ng_qty", ng_qty);
            dataStr = JsonConvert.SerializeObject(data);
            json = MesApi.Ins_MoNgData(pubToken, dataStr);
            obj_ins = JsonConvert.DeserializeObject<JObject>(json);
            sMsg = sMsg + " 不良數量：" + obj_ins["Message"].ToString();  ////錯誤訊息



            //更新工單狀態
            data.Clear();
            json = "";
            dataStr = "";
            obj_ins.Values("");
            data.Add("mo_code", sMoCode);
            data.Add("wrk_code", sWrkCode);
            data.Add("mac_code", sMacCode);
            data.Add("per_code", sPerCode);
            data.Add("mo_status", "END");
            dataStr = JsonConvert.SerializeObject(data);
            json = MesApi.Upd_MoStatus(pubToken, dataStr);
            obj_ins = JsonConvert.DeserializeObject<JObject>(json);
            sMsg = sMsg + " 更新工單：" + obj_ins["Message"].ToString();  ////錯誤訊息


            //string sMacCode = iWork.Get_MacCodeByMacAddress();
            //string sPerCode = iWork.Get_UserCodeByMacCode(sMacCode);
            //string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
            //string sWrkCode = iWork.Get_WrkCodeByMacCode(sMacCode);
            //float now_ok_qty = comm.sGetfloat(form["pro_qty"].ToString());
            //string slot_no = comm.sGetString(form["lot_no"].ToString());
            ////寫入生產資料
            //MED09_0000 med09_0000 = new MED09_0000();
            //comm.Set_ModelValue(med09_0000, form);
            //med09_0000.mo_code = sMoCode;
            //med09_0000.wrk_code = sWrkCode;
            //med09_0000.mac_code = comm.sGetString(sMacCode);
            //med09_0000.pro_code = comm.Get_QueryData("MET01_0000", sMoCode, "mo_code", "pro_code");
            //med09_0000.pro_lot_no = slot_no;
            //med09_0000.pro_qty = comm.sGetDouble(form["pro_qty"].ToString());
            //med09_0000.ins_date = comm.Get_Date();
            //med09_0000.ins_time = comm.Get_Time();
            //med09_0000.usr_code = sPerCode;
            //med09_0000.des_memo = "";
            //med09_0000.is_ng = "N";
            //med09_0000.is_end = "N";
            //med09_0000.end_memo = "";
            //med09_0000.end_date = "";
            //med09_0000.end_time = "";
            //med09_0000.end_usr_code = "";
            //med09_0000.InsertData(med09_0000);
            //string sSql = "select ok_qty " +
            //             " from  MEM01_0000  " +
            //             " where mo_code = '" + sMoCode + "' " +
            //             "       and mac_code ='" + sMacCode + "' ";
            //var dtDat = comm.Get_DataTable(sSql);
            //float tmp_ok = comm.sGetfloat(dtDat.Rows[0]["ok_qty"].ToString());
            //string uSql = "update MEM01_0000 " +
            //            "  set ok_qty = '" + (tmp_ok + now_ok_qty) + "'" +
            //            "  where mo_code = '" + sMoCode + "' " +
            //            "       and mac_code ='" + sMacCode + "' ";
            //comm.Connect_DB(uSql);
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