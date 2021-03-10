using MES_WORK.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.NetworkInformation;
using Newtonsoft.Json;

namespace MES_WORK.Controllers
{
    public class RTR000AController : JsonNetController
    {
        Comm comm = new Comm();
        Work iwork = new Work();
        GetData GD = new GetData();
        DynamicTable DT = new DynamicTable();
        CheckData CD = new CheckData();
        MesApi MesApi = new MesApi();

        public string PrgCode()
        {
            return ControllerContext.RouteData.Values["controller"].ToString();
        }

        // GET: MED010A
        public ActionResult Index()
        {            
            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            string sMacCode = iwork.Get_MacCodeByMacAddress();
            string sAddress = iwork.Get_MacAddress();

            if (sMacCode != "")
            {
                //曾經綁定過，改update 機台代號
                iwork.Upd_MacAddress(comm.sGetString(form["mac_code"]), sAddress);
            }
            else
            {
                //全新的工站
                //MEM04_0000 mem04_0000 = new MEM04_0000();
                //mem04_0000.mac_code = comm.sGetString(form["mac_code"]);
                //mem04_0000.station_code = comm.Get_QueryData("MEB30_0100", mem04_0000.mac_code,"station_code","work_code");
                //mem04_0000.station_ip = iwork.Get_MacAddress();
                //mem04_0000.station_mac = iwork.Get_MacAddress();
                //mem04_0000.InsertData(mem04_0000);
                iwork.Upd_MacAddress(comm.sGetString(form["mac_code"]), sAddress);

            }

            return RedirectToAction("Index","Main");
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
                case "mac_code":
                    if (pValue.Length > 20)
                    {
                        sStatus = "N";
                        sMessage = "代號過長";
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
            if (comm.Get_Data("MEB15_0000", form["mac_code"], "mac_code", "mac_code") == "")
            {
                sStatus = "N";
                sMessage = "無此機台代號";
            }

            //回傳格式
            sReturn = sStatus + "|" + sMessage;
            return sReturn;
        }






    }
}