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
    public class RTR070AController : Controller
    {
        Comm comm = new Comm();
        GetData GD = new GetData();
        DynamicTable DT = new DynamicTable();
        CheckData CD = new CheckData();
        WebReference.WmsApi WA = new WebReference.WmsApi();
        Work iWork = new Work();
        MesApi MesApi = new MesApi();
        public string pubToken = "b0daf10bb4df45b9a58c20ead5a94d98";

        //表單欄位table
        public string pubFieldTable = "WRK02_0100";

        //索引鍵
        public string pubPKCode()
        {
            return DT.Get_Table_PKField(pubFieldTable);
        }

        public string PrgCode()
        {
            return ControllerContext.RouteData.Values["controller"].ToString();
        }

        // GET: MED010A
        public ActionResult Index()
        {
            DataTable sText = comm.Get_DataTable(comm.Get_strSQL("MEB48_0000", "s", "table_code", "MED06_0000"));
            ViewBag.Tabel = sText;
            ViewData["mo_code"] = ViewBag.mo_code;
            ViewData["pro_code"] = ViewBag.pro_code;
            Get_Data();
            return View();
        }
        public DataTable Get_Data()
        {
            //在前端設定需要看到的欄位
            //這裡只負責抓資料

            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sPerCode = iWork.Get_UserCodeByMacCode(sMacCode);
            string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);

            DataTable dtDat = new DataTable();
            DataTable dtTmp = new DataTable();
            try
            {
                dtDat.Columns.Add("mo_code", System.Type.GetType("System.String"));
                dtDat.Columns.Add("pro_code", System.Type.GetType("System.String"));
                dtDat.Columns.Add("pro_name", System.Type.GetType("System.String"));
                dtDat.Columns.Add("pro_qty", System.Type.GetType("System.String"));
                dtDat.Columns.Add("lot_no", System.Type.GetType("System.String"));
                dtDat.Columns.Add("ins_date", System.Type.GetType("System.String"));
                //dtDat.Columns.Add("in_time", System.Type.GetType("System.String"));
                //dtDat.Columns.Add("out_qty", System.Type.GetType("System.String"));
                //dtDat.Columns.Add("out_time", System.Type.GetType("System.String"));
                dtDat.Columns.Add("inventory", System.Type.GetType("System.String"));

                //從WEBAPI取值
                //string jsontmp = MesApi.Get_PreparList(pubToken, "2020/07/20", sMacCode);
                string jsontmp = MesApi.Get_PreparEdList(pubToken, sMacCode);
                JObject obj = JsonConvert.DeserializeObject<JObject>(jsontmp);
                if (obj["Result"].ToString() == "OK")
                {
                    dtTmp = JsonConvert.DeserializeObject<DataTable>(obj["Data"].ToString());
                }

                int i;
                for (i = 0; i < dtTmp.Rows.Count; i++)
                {
                    DataRow drow = dtDat.NewRow();
                    drow["mo_code"] = comm.sGetString(dtTmp.Rows[i]["mo_code"].ToString());
                    drow["pro_code"] = comm.sGetString(dtTmp.Rows[i]["pro_code"].ToString());
                    drow["pro_name"] = comm.sGetString(dtTmp.Rows[i]["pro_name"].ToString());
                    drow["pro_qty"] = comm.sGetString(dtTmp.Rows[i]["pro_qty"].ToString());
                    drow["lot_no"] = comm.sGetString(dtTmp.Rows[i]["lot_no"].ToString());
                    drow["ins_date"] = comm.sGetString(dtTmp.Rows[i]["ins_date"].ToString());
                    //drow["in_time"] = comm.sGetString(dtTmp.Rows[i]["in_time"].ToString());
                    //drow["out_time"] = comm.sGetString(dtTmp.Rows[i]["out_time"].ToString());
                    //drow["out_qty"] = comm.sGetString(dtTmp.Rows[i]["out_qty"].ToString());
                    drow["inventory"] = comm.sGetString(dtTmp.Rows[i]["inventory"].ToString());
                    dtDat.Rows.Add(drow);

                }

            }
            catch (Exception e)
            {

            }
            return dtDat;
        }


        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sPerCode = iWork.Get_UserCodeByMacCode(sMacCode);

            string sOutQty = comm.sGetString(form["out_qty"].ToString());
            string sProCode = comm.sGetString(form["pro_code"].ToString());
            string sLotNo = comm.sGetString(form["lot_no"].ToString());

            //MED07_0000 med07_0000 = new MED07_0000();
            //comm.Set_ModelValue(med07_0000, form);
            //med07_0000.InsertData(med07_0000);

            try
            {
                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("pro_code", sProCode);
                data.Add("lot_no", sLotNo);
                data.Add("loc_code", "");
                data.Add("mac_code", sMacCode);
                data.Add("per_code", sPerCode);
                data.Add("pro_qty", sOutQty);
                data.Add("pallet_code","");
                string dataStr = JsonConvert.SerializeObject(data);
                //寫入退料記錄
                string json = MesApi.Ins_PreparEdData(pubToken, dataStr, form);
                JObject obj_ins = JsonConvert.DeserializeObject<JObject>(json);
                ViewBag.message = obj_ins["Message"].ToString();  ////錯誤訊息
            }
            catch (Exception e)
            {

            }
            return View();
        }


        /// <summary>
        /// 將Sql語法製作成DataTable 並且調整裡面的值
        /// </summary>
        /// <returns></returns>
        public DataTable Table_DataTable()
        {
            string sSql = "select top 10 mac_name,pro_name,pro_qty,MED07_0000.pro_unit,loc_code,ins_date,ins_time,usr_code " +
                          "  from MED07_0000" +
                          "   left join MEB15_0000 on MED07_0000.mac_code = MEB15_0000.mac_code " +
                          "   left join MEB20_0000 on MEB20_0000.pro_code = MED07_0000.pro_code " +
                          " where ins_date = @ins_date" +
                          "  order by ins_date desc,ins_time desc";
            var dtTmp = comm.Get_DataTable(sSql, "ins_date", DateTime.Now.ToString("yyyy/MM/dd"));

            for (int i = 0; i < dtTmp.Rows.Count; i++)
            {
                DataRow Row = dtTmp.Rows[i];
                string TableField = DT.Get_SqlField(dtTmp);
                for (int u = 0; u < TableField.Split(',').Length; u++)
                {
                    string sField = TableField.Split(',')[u]; //欄位
                    string sValue = Row[sField].ToString();  //欄位的值

                    switch (sField)
                    {
                        case "pro_qty":
                            Row[sField] = decimal.Parse(sValue).ToString("G29");
                            break;
                    }
                }
            }
            return dtTmp;
        }

        /// <summary>
        /// 取得欄位名稱
        /// </summary>
        /// <param name="pFieldCode"></param>
        /// <returns></returns>
        public string Get_FieldName(string pFieldCode)
        {
            string sValue = "";
            switch (pFieldCode)
            {
                case "mo_code":
                    sValue = "工單編號";
                    break;
                case "wrk_code":
                    sValue = "派工單號";
                    break;
                case "mac_code":
                    sValue = "機台代碼";
                    break;
                case "mac_name":
                    sValue = "機台名稱";
                    break;
                case "ins_date":
                    sValue = "建立日期";
                    break;
                case "ins_time":
                    sValue = "建立時間";
                    break;
                case "usr_code":
                    sValue = "使用者編號";
                    break;
                case "pro_name":
                    sValue = "產品名稱";
                    break;
                case "lot_no":
                    sValue = "批號";
                    break;
                case "pro_qty":
                    sValue = "數量";
                    break;
                case "pro_unit":
                    sValue = "單位";
                    break;
                case "loc_code":
                    sValue = "上料位置";
                    break;

                default:
                    sValue = pFieldCode;
                    break;
            }
            return sValue;
        }



        /// <summary>
        /// 檢查資料
        /// </summary>
        /// <returns></returns>
        public string Chk_Data(string Id, string Value)
        {
            string sFieldCode = GD.Get_Data("WRK02_0100", Id, "wrk02_0100", "field_code");

            string sValue = "";
            //string sSql = "select * from WRK02_0100 where 1=1";
            //var dtTmp = comm.Get_DataTable(sSql);
            //if (dtTmp.Rows.Count > 0)
            //{

            //}
            //switch (sFieldCode) {
            //    case "mo_code":
            //        break;
            //    case "mac_code":
            //        sValue = "機台代號錯誤";
            //        break;
            //}
            return sValue;
        }



        public ActionResult Insert(MED07_0000 model)
        {
            MED07_0000 data = new MED07_0000();

            //在取完畫面上的值後，如果有一些別名欄位要變更值，可以在這邊2次加工
            data.mo_code = "mo_code";
            data.wrk_code = "wrk_code";
            data.mac_code = "mac_code";
            data.pro_code = "pro_code";
            data.lot_no = "lot_no";
            data.pro_qty = 1;
            data.pro_unit = "cm";
            data.loc_code = "loc_code";
            data.ins_date = DateTime.Now.ToString("yyyy/MM/dd");
            data.ins_time = DateTime.Now.ToString("HH:mm:ss");
            data.usr_code = "Derrick";
            data.des_memo = "des_memo";
            data.is_ng = "Y";
            data.is_end = "Y";
            data.end_memo = "end_memo";
            data.end_date = DateTime.Now.ToString("yyyy/MM/dd");
            data.end_time = DateTime.Now.ToString("HH:mm:ss");
            data.end_usr_code = "Derrick";
            //執行存檔
            data.InsertData(data);
            //存完檔回到主頁，如果不跳回主頁要在這裡做修改
            return RedirectToAction("Index");
        }

        public ActionResult Update(MED07_0000 model)
        {
            MED07_0000 data = new MED07_0000();

            //在取完畫面上的值後，如果有一些別名欄位要變更值，可以在這邊2次加工
            int t = Int32.Parse(GetDataTable("select MAX(med07_0000) as med07_0000 from MED07_0000"));
            if (t != 0)
            {
                data.med07_0000 = t;
                data.mo_code = "mo_code2";
                data.wrk_code = "wrk_code2";
                data.mac_code = "mac_code2";
                data.pro_code = "pro_code2";
                data.lot_no = "lot_no2";
                data.pro_qty = 2;
                data.pro_unit = "m";
                data.loc_code = "loc_code2";
                data.ins_date = DateTime.Now.ToString("yyyy/MM/dd");
                data.ins_time = DateTime.Now.ToString("HH:mm:ss");
                data.usr_code = "Derrick2";
                data.des_memo = "des_memo2";
                data.is_ng = "N";
                data.is_end = "N";
                data.end_memo = "end_memo2";
                data.end_date = DateTime.Now.ToString("yyyy/MM/dd");
                data.end_time = DateTime.Now.ToString("HH:mm:ss");
                data.end_usr_code = "Derrick2";
                data.UpdateData(data);
            }
            //存完檔回到主頁，如果不跳回主頁要在這裡做修改
            return RedirectToAction("Index");
        }

        public string GetDataTable(string sSql)
        {
            string i = "0";
            var dtTmp = comm.Get_DataTable(sSql);
            if (dtTmp.Rows.Count >= 1)
            {
                //取得欄位代號後，執行Sql語法
                i = dtTmp.Rows[0]["med07_0000"].ToString();
            }
            return (i);
        }
        public bool Chk_ID()
        {
            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string tmpID = iWork.Get_UserCodeByMacCode(sMacCode);
            if (tmpID == "") { return false; }
            return true;
        }
        public string Chk_Lot_No(string pro_code,string lot_no , string out_qty)
        {
            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
            string no_pro_code = comm.Get_QueryData("MET01_0000", sMoCode, "mo_code", "pro_code");
            string[] slot_no =  lot_no.Split('%');

            string num= out_qty;
            if (slot_no.Length > 6) { pro_code = slot_no[0]; num = slot_no[6]; }

            if (no_pro_code != pro_code.Trim())
            {
                return "請確認料號是否吻合";
            }
            if (comm.sGetfloat(num) <= 0)
            {
                return "請確認數量";
            }
            return "";
        }

    }
}