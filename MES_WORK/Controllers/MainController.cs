using MES_WORK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
namespace MES_WORK.Controllers
{
    public class MainController : Controller
    {
        Comm comm = new Comm();
        Work iWork = new Work();
        // GET: Main
        public ActionResult Index()
        {
            if (Request.Cookies["MacCode"] == null)
            {
                HttpCookie Cookie = new HttpCookie("MacCode");
                Cookie.Expires = DateTime.Now.AddDays(1);
                HttpContext.Response.Cookies.Add(Cookie);
                Cookie.Value = "";
            }
            //GET MAC-CODE
            string sMacCode = iWork.Get_MacCodeByMacAddress();

            string sSql =
             "   SELECT MEB30_0400.prg_code                                                            " +
             "   FROM      MEB30_0000 INNER JOIN                                                       " +
             "       MEB30_0100 ON MEB30_0000.work_code = MEB30_0100.work_code INNER JOIN              " +
             "       MEB30_0400 ON MEB30_0000.work_code = MEB30_0400.work_code INNER JOIN              " +
             "       MEB29_0000 ON MEB30_0100.station_code = MEB29_0000.station_code INNER JOIN        " +
             "       MEB15_0000 ON MEB29_0000.mac_code = MEB15_0000.mac_code                           " +
             "   where MEB15_0000.mac_code='" + sMacCode + "'                                          " +
             "   order by src_no asc                                                                   ";

            //GET PRG STRING
            DataTable dTmp = comm.Get_DataTable(sSql);
            ViewBag.PrgCode = "RTR000A|";
            if (dTmp.Rows.Count > 0)
            {
                foreach (DataRow drTmp in dTmp.Rows)
                {
                    ViewBag.PrgCode += drTmp[0].ToString() + "|";
                }
            }
            ViewBag.PrgCode += "Setting";

            return View();
        }
        public string pubMacCode()
        {
            return iWork.Get_MacCodeByMacAddress();
        }


        private string Get_MacPermission(string pMacCode) {
            //取得目前上工人員的功能權限
            string sPermission = "";
            string sSql = "select * from MEM01_0000 " +
                    " where mac_code = '" + Request.Cookies["MacCode"].Value + "'" +
                    "   and status = 'Y' ";
            var dtTmp = comm.Get_DataTable(sSql);

            for (int i = 0; i < dtTmp.Rows.Count; i++) {
                sSql = "select * from CNB06_0000 " +
                       " where usr_code = '" + dtTmp.Rows[i]["per_code"] + "'";
                var dtTmp2 = comm.Get_DataTable(sSql);
                if (dtTmp2.Rows[i]["sup_code"].ToString() == "A3210")
                {
                    sPermission = "G004";
                    if (dtTmp2.Rows[i]["is_rec"].ToString() == "Y") sPermission = "S002";
                }
                else
                {
                    sPermission = "S001";
                }
            }
            return sPermission;
        }



        /// <summary>
        /// 取得停機資料，若沒有停機則為空值
        /// </summary>
        /// <returns></returns>
        public string Get_OnData(string pShowField)
        {
            string val = "";
            string sSql = "";
            if (pShowField == "stop_code") {
                 sSql = "select top 1 * from MED04_0000" +
                          " where mac_code = '" + pubMacCode() + "'" +
                          "   and date_e = ''";
            }
            if (pShowField == "except_code") {
                sSql="select top 1 * from MED05_0000" +
                          " where mac_code = '" + pubMacCode() + "'" +
                          "   and date_e = ''";
            }
            var dtTmp = comm.Get_DataTable(sSql);
            if (dtTmp.Rows.Count > 0)
            {
                val = dtTmp.Rows[0][pShowField].ToString();
            }
            return val;
        }



    }
}