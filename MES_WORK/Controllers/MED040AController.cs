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
    public class MED040AController : Controller
    {
        Comm comm = new Comm();
        GetData GD = new GetData();
        DynamicTable DT = new DynamicTable();
        CheckData CD = new CheckData();
        Work iWork = new Work();
        MesApi MesApi = new MesApi();
        public string pubToken = "b0daf10bb4df45b9a58c20ead5a94d98";

        public string PrgCode()
        {
            return ControllerContext.RouteData.Values["controller"].ToString();
        }

        public string pubMacCode() {
            return iWork.Get_MacCodeByMacAddress();
        }
        public string pubPerCode()
        {
            return iWork.Get_UserCodeByMacCode(pubMacCode());
        }
        public string pubMoCode()
        {
            return iWork.Get_MoCodeByMacCode(pubMacCode());
        }
        public string pubWorkCode()
        {
            return iWork.Get_WrkCodeByMacCode(pubMacCode());
        }

        // GET: MED010A
        public ActionResult Index()
        {
            ViewBag.Tabel = comm.Get_DataTable(comm.Get_strSQL("MEB48_0000", "s", "table_code", "MED04_0000"));
            return View();
        }



        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            Response.Cookies["MacCode"].Value = comm.sGetString(form["mac_code"]);
            return RedirectToAction("Index","Main");
        }


        public DataTable Get_StopCode()
        {
            //string sSql = "select * from MEB45_0000";
            //var dtTmp = comm.Get_DataTable(sSql);
            //return dtTmp;



            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sPerCode = iWork.Get_UserCodeByMacCode(sMacCode);
            string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
            DataTable dtTmp = new DataTable();

            DataTable dtDat = new DataTable();

            try
            {
                string jsontmp = MesApi.Get_StopCodeList(pubToken, sMacCode);
                JObject obj = JsonConvert.DeserializeObject<JObject>(jsontmp);
                if (obj["Result"].ToString() == "OK")
                {
                    dtDat.Columns.Add("stop_code", System.Type.GetType("System.String"));
                    dtDat.Columns.Add("stop_name", System.Type.GetType("System.String"));

                    dtTmp = JsonConvert.DeserializeObject<DataTable>(obj["Data"].ToString());
                    int i;
                    for (i = 0; i < dtTmp.Rows.Count; i++)
                    {
                        DataRow drow = dtDat.NewRow();
                        drow["stop_code"] = comm.sGetString(dtTmp.Rows[i]["stop_code"].ToString());
                        drow["stop_name"] = comm.sGetString(dtTmp.Rows[i]["stop_name"].ToString());

                        dtDat.Rows.Add(drow);
                    }
                }
            }catch(Exception e)
            {

            }
            //從WEBAPI取值
            



            return dtDat;

        }

        public bool Chk_IsOnStop() {
            bool val = false;
            string sSql = "select * from MED04_0000" +
                          " where mac_code = '" + pubMacCode() + "'" +
                          "   and date_e = ''";
            var dtTmp = comm.Get_DataTable(sSql);
            if (dtTmp.Rows.Count > 0) {
                val = true;
            }
            return val;
        }

        /// <summary>
        /// 取得停機資料，若沒有停機則為空值
        /// </summary>
        /// <returns></returns>
        public string Get_OnStopData(string pShowField) {
            string val = "";
            string sSql = "select top 1 * from MED04_0000" +
                          " where mac_code = '" + pubMacCode() + "'" +
                          "   and date_e = ''";
            var dtTmp = comm.Get_DataTable(sSql);
            if (dtTmp.Rows.Count > 0)
            {
                val = dtTmp.Rows[0][pShowField].ToString();
            }
            return val;
        }


        public void Stop_Start(FormCollection form)
        {
            object data = new object();
            string sStopCode = comm.sGetString(form["stop_code"]);
            for(int i =1; i<10; i++)
                if (form["user_field_0"+i] == "undefined") form["user_field_0"+i]="";
            if (form["user_field_10"] == "undefined") form["user_field_10"] = "";
            MED04_0000 med04_0000 = new MED04_0000();
            comm.Set_ModelValue(med04_0000, form);
            //data = new
            //{
            //    mo_code = pubMoCode(),
            //    wrk_code = pubWorkCode(),
            //    mac_code = pubMacCode(),
            //    stop_code = sStopCode,
            //    date_s = DateTime.Now.ToString("yyyy/MM/dd"),
            //    time_s = DateTime.Now.ToString("HH:mm:ss"),
            //    ins_date = DateTime.Now.ToString("yyyy/MM/dd"),
            //    ins_time = DateTime.Now.ToString("HH:mm:ss"),
            //    usr_code = pubPerCode(),
            //    is_ng = "N",
            //    is_end = "N",
            //};
            med04_0000.mo_code = pubMoCode();
            med04_0000.wrk_code = pubWorkCode();
            med04_0000.mac_code = pubMacCode();
            med04_0000.stop_code = sStopCode;
            med04_0000.date_s = DateTime.Now.ToString("yyyy/MM/dd");
            med04_0000.time_s = DateTime.Now.ToString("HH:mm:ss");
            med04_0000.ins_date = DateTime.Now.ToString("yyyy/MM/dd");
            med04_0000.ins_time = DateTime.Now.ToString("HH:mm:ss");
            med04_0000.usr_code = pubPerCode();
            med04_0000.is_ng = "N";
            med04_0000.is_end = "N";
            DT.InsertData("MED04_0000", med04_0000);


            //med04_0000 = new MED04_0000
            //{
            //    mo_code = pubMacCode(),
            //    wrk_code = pubWorkCode(),
            //    mac_code = pubMacCode(),
            //    stop_code = sStopCode,
            //    date_s = DateTime.Now.ToString("yyyy/MM/dd"),
            //    time_s = DateTime.Now.ToString("HH:mm:ss"),
            //    ins_date = DateTime.Now.ToString("yyyy/MM/dd"),
            //    ins_time = DateTime.Now.ToString("HH:mm:ss"),
            //    usr_code = pubPerCode(),
            //    is_ng = "N",
            //    is_end = "N",
            //};
            ////med04_0000.InsertData(med04_0000);
            //DT.InsertData("MED04_0000", med04_0000);
        }

        public void Stop_End(FormCollection form)
        {
            string sSql = "update MED04_0000 " +
                          "   set date_e = '" + DateTime.Now.ToString("yyyy/MM/dd") + "'" +
                          "      ,time_e = '" + DateTime.Now.ToString("HH:mm:ss") + "'" +
                          " where mac_code = '" + pubMacCode() + "'" +
                          "   and date_e = ''";
            comm.Connect_DB(sSql);
        }


        public string Get_Timer(FormCollection form)
        {
            Double time = 0;
            string sData = "";
            string sSql = "select top 1 * from MED04_0000 " +
                          " where mac_code = @mac_code" +
                          "   and date_e = ''";
            var dtTmp = comm.Get_DataTable(sSql, "mac_code", pubMacCode());
            if (dtTmp.Rows.Count > 0)
            {
                string sDateS = dtTmp.Rows[0]["date_s"].ToString();
                string sTimeS = dtTmp.Rows[0]["time_s"].ToString();
                time = new TimeSpan(DateTime.Now.Ticks - DateTime.Parse(sDateS + " " + sTimeS).Ticks).TotalSeconds;
            }
            sData += Convert.ToInt32(Math.Floor(time / 3600)) + " 小時 " + Convert.ToInt32(Math.Floor((time % 3600) / 60)) + " 分 " + Convert.ToInt32(Math.Floor((time % 3600) % 60)) + " 秒 ";

            return sData;
        }


        /// <summary>
        /// 檢查資料
        /// </summary>
        /// <returns></returns>
        public string Chk_Data(string Id, string Value)
        {
            //string sFieldCode = GD.Get_Data("WRK02_0100", Id, "wrk02_0100", "field_code");

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



       


    }
}