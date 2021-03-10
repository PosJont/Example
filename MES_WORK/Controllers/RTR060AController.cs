﻿using MES_WORK.Models;
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
    public class RTR060AController : Controller
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

        // GET: MED010A
        public ActionResult Index()
        {
            DataTable sText = comm.Get_DataTable(comm.Get_strSQL("MEB48_0000", "s", "table_code", "MED06_0000"));
            ViewBag.Tabel = sText;
            ViewData["wrk_code"] = ViewBag.pro_name;
            ViewData["lot_no"] = ViewBag.lot_no;
            ViewData["pro_qty"] = ViewBag.pro_qty;
            //string sMacCode = iWork.Get_MacCodeByMacAddress();
            //string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
            //string sProCode = comm.Get_QueryData("MET01_0000", sMoCode, "mo_code", "pro_code");
            //string sProName = comm.Get_QueryData("MEB20_0000", sProCode, "pro_code", "pro_name");
            //ViewBag.mo_code = sMoCode;
            //ViewBag.pro_name = sProCode + " " + sProName;
            //ViewBag.mo_qty = comm.Get_QueryData<decimal>("MET01_0000", sMoCode, "mo_code", "plan_qty");
            //ViewBag.res_qty = iWork.Get_ProQtyByMocode(sMoCode);
            Get_Data();
            return View();
        }

        //private DataTable Get_PalletContent(string pPalletCode)
        //{
        //    string sSql = "";

        //    sSql = "select * from MED09_0000 " +
        //           " where pallet_code='" + pPalletCode + "'" +
        //           " order by ins_date desc,ins_time desc ";
        //    DataTable dtFun = comm.Get_DataTable(sSql);
        //    return dtFun;
        //}

        public DataTable Get_Data()
        {
            //在前端設定需要看到的欄位
            //這裡只負責抓資料

            string sMacCode = iWork.Get_MacCodeByMacAddress();
            string sPerCode = iWork.Get_UserCodeByMacCode(sMacCode);
            string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
            DataTable dtTmp = new DataTable();

            DataTable dtDat = new DataTable();
            dtDat.Columns.Add("scr_no", System.Type.GetType("System.String"));
            dtDat.Columns.Add("mo_code", System.Type.GetType("System.String"));
            dtDat.Columns.Add("pro_code", System.Type.GetType("System.String"));
            dtDat.Columns.Add("pro_name", System.Type.GetType("System.String"));
            dtDat.Columns.Add("pro_qty", System.Type.GetType("System.String"));
            dtDat.Columns.Add("res_qty", System.Type.GetType("System.String"));
            dtDat.Columns.Add("inventory", System.Type.GetType("System.String"));
            try
            {
                //從WEBAPI取值
                //string jsontmp = MesApi.Get_PreparList(pubToken, "2020/07/20", sMacCode);
                string jsontmp = MesApi.Get_PreparList(pubToken, comm.Get_Date(), sMacCode);
                JObject obj = JsonConvert.DeserializeObject<JObject>(jsontmp);
                if (obj["Result"].ToString() == "OK")
                {
                    dtTmp = JsonConvert.DeserializeObject<DataTable>(obj["Data"].ToString());
                }

                int i;
                for (i = 0; i < dtTmp.Rows.Count; i++)
                {
                    DataRow drow = dtDat.NewRow();
                    drow["scr_no"] = comm.sGetString(dtTmp.Rows[i]["scr_no"].ToString());
                    drow["mo_code"] = comm.sGetString(dtTmp.Rows[i]["mo_code"].ToString());
                    drow["pro_code"] = comm.sGetString(dtTmp.Rows[i]["pro_code"].ToString());
                    drow["pro_name"] = comm.sGetString(dtTmp.Rows[i]["pro_name"].ToString());
                    drow["pro_qty"] = comm.sGetString(dtTmp.Rows[i]["pro_qty"].ToString());
                    drow["res_qty"] = comm.sGetString(dtTmp.Rows[i]["res_qty"].ToString());
                    drow["inventory"] = comm.sGetString(dtTmp.Rows[i]["inventory"].ToString());
                    dtDat.Rows.Add(drow);
                }
            }
            catch(Exception e)
            {
                return dtDat;
            }

            return dtDat;
        }


        /// <summary>
        /// 前端存檔動作
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            
            try {
                string sMacCode = iWork.Get_MacCodeByMacAddress();
                string sPerCode = iWork.Get_UserCodeByMacCode(sMacCode);
                string sMoCode = iWork.Get_MoCodeByMacCode(sMacCode);
                //SET 容器值
                string pallet_code = ""; //容器代碼
                if (form.AllKeys.Contains("pallet_code")) pallet_code = form["pallet_code"];

                DataTable dtTmp = new DataTable();
                DataTable dtTmp1 = new DataTable();

                //如果輸入的是料號條碼則拆解QRCODE
                string sProCode = comm.sGetString(form["pro_code"].ToString());
                string pro_code = "", lot_no="" , pro_qty = "0";

                //MED06_0000 med06_0000 = new MED06_0000();
                //comm.Set_ModelValue(med06_0000, form);
                //med06_0000.mo_code = sMoCode;
                //med06_0000.mac_code = sMacCode;
                //med06_0000.InsertData(med06_0000);

                if (!string.IsNullOrEmpty(sProCode))
                {
                    //拆解QRcode
                    string jsontmp = MesApi.Get_SplitQrCode(sProCode);
                    JObject obj = JsonConvert.DeserializeObject<JObject>(jsontmp);
                    if (obj["Result"].ToString() == "OK") dtTmp = JsonConvert.DeserializeObject<DataTable>(obj["Data"].ToString());
                    pro_code = comm.sGetString(dtTmp.Rows[0]["pro_code"].ToString());
                    lot_no = comm.sGetString(dtTmp.Rows[0]["lot_no"].ToString());
                    pro_qty = comm.sGetString(dtTmp.Rows[0]["pro_qty"].ToString());
                }

                //foreach (var key in form.AllKeys)
                //{
                //    if (key == "pallet_code") pallet_code = form["pallet_code"].ToString();
                //}

                //只輸入容器
                if (!string.IsNullOrEmpty(pallet_code))
                {
                    dtTmp = MesApi.Get_DT_PalletContent(pallet_code);
                    pro_code = comm.sGetString(dtTmp.Rows[0]["pro_code"].ToString());
                    lot_no = comm.sGetString(dtTmp.Rows[0]["pro_lot_no"].ToString());
                    pro_qty = comm.sGetString(dtTmp.Rows[0]["pro_qty"].ToString());
                }

                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("pro_code", pro_code);
                data.Add("lot_no", lot_no);
                data.Add("mac_code", sMacCode);
                data.Add("per_code", sPerCode);
                data.Add("loc_code", "");
                data.Add("pro_qty", pro_qty);
                data.Add("pallet_code", pallet_code);

                string dataStr = JsonConvert.SerializeObject(data);
                //寫入上料記錄
                string json = MesApi.Ins_PreparData(pubToken, dataStr,form);
                JObject obj_ins = JsonConvert.DeserializeObject<JObject>(json);
                ViewBag.message = obj_ins["Message"].ToString();  ////錯誤訊息
                return View();


            }
            catch (Exception e)
            {

                ViewBag.message = "存檔失敗，請在重新輸入";
                return View();

            }
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

            string lot_no = "";
            string pro_qty = "";
            string pro_name = "";

            switch (pID)
            {
                case "pro_code2":
                    if (pValue.Length > 20)
                    {
                        sStatus = "N";
                        sMessage = "代號過長";
                        sIsSelect = true;
                    }
                    break;
                case "pro_code":

                    string[] tmpPro_Code = pValue.Split('%');
                    lot_no = tmpPro_Code[1].ToString();
                    pro_qty = tmpPro_Code[6].ToString();
                    pro_name = Get_ProName(tmpPro_Code[0].ToString());
                    sReturn = sStatus + "|" + sMessage + "|" + sIsSelect.ToString()+"|"+ lot_no+","+ pro_qty+"," + pro_name;
                    return sReturn;

                case "pallet_code":
                    string sPalletCode = pValue;
                    DataTable dtTmp = MesApi.Get_DT_PalletContent(sPalletCode);
                    lot_no = comm.sGetString(dtTmp.Rows[0]["pro_lot_no"].ToString());
                    pro_qty = comm.sGetString(dtTmp.Rows[0]["pro_qty"].ToString());
                    pro_name = Get_ProName(comm.sGetString(dtTmp.Rows[0]["pro_code"].ToString()));
                    sReturn = sStatus + "|" + sMessage + "|" + sIsSelect.ToString() + "|" + lot_no + "," + pro_qty + "," + pro_name;
                    return sReturn;
            }
            sReturn = sStatus + "|" + sMessage + "|" + sIsSelect.ToString();
            return sReturn;
            //回傳格式

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
            if (comm.Get_Data("MEB15_0000", form["pro_code"], "pro_code", "pro_name") == "")
            {
                sStatus = "N";
                sMessage = "無此料件代號";
            }

            //回傳格式
            sReturn = sStatus + "|" + sMessage;
            return sReturn;
        }
        public string Get_ProName(string sProCode) {
            string pro_name = "";
            string pro_code = sProCode;
            pro_name = comm.Get_Data("MEB20_0000", pro_code, "pro_code", "pro_name");
            return pro_name;
        }

        }
}