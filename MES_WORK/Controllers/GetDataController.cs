using MES_WORK.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MES_WORK.Controllers
{
    public class GetDataController : Controller
    {
        Comm comm = new Comm();
        GetData GD = new GetData();
        DynamicTable DT = new DynamicTable();
        CheckData CD = new CheckData();

        // GET: GetData
        public ActionResult Index()
        {
            return View();
        }




        /// <summary>
        /// 取得資訊
        /// </summary>
        /// <param name="T">資料庫</param>
        /// <param name="K">鍵值</param>
        /// <param name="KF">鍵值欄位</param>
        /// <param name="F">欄位</param>
        /// <returns></returns>
        public string Get_Data(string T, string K, string KF, string F)
        {
            return GD.Get_Data(T, K, KF, F);
        }

        /// <summary>
        /// 取得停機總時間
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public string Get_OnStopTime(string pTable)
        {
            Double time = 0;
            string sData = "";
            string sSql = "select top(1) * from " + pTable +
                          " where date_e = ''" +                          
                          "  order by date_s,time_s";
            var dtTmp = comm.Get_DataTable(sSql);
            if (dtTmp.Rows.Count > 0)
            {
                time = new TimeSpan(DateTime.Now.Ticks - DateTime.Parse(dtTmp.Rows[0]["time_s"].ToString()).Ticks).TotalSeconds;
            }
            sData += Convert.ToInt32(Math.Floor(time / 3600)) + " 小時 " + Convert.ToInt32(Math.Floor((time % 3600) / 60)) + " 分 " + Convert.ToInt32(Math.Floor((time % 3600) % 60)) + " 秒 ";
            return sData;
        }

    }
}