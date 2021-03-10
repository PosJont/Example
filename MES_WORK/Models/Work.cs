using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using System.Reflection;
using System.ComponentModel;
using System.Web.Mvc;

using System.Web.Helpers;
using MVC.Models;
using System.Net.NetworkInformation;


namespace MES_WORK.Models
{
    public class Work
    {
        Comm comm = new Comm();

        ///// <summary>
        ///// 檢查單一人員的上工狀態
        ///// </summary>
        ///// <param name="pPerCode"></param>
        ///// <returns></returns>
        //public string Chk_PerStaus01(string pPerCode)
        //{
        //    return "";
        //    //string sSql = "";
        //    //sSql = "select * from MED01_0000 " +
        //    //       " where usr_code='" + pPerCode + "'" +
        //    //       "   and "
        //}

        /// <summary>
        /// 取得單一人員在一機台的的上機狀態(限定當天) 
        /// </summary>
        /// <param name="pPerCode">人員編號</param>
        /// <param name="pMacCode">機台編號</param>
        /// <returns>I:已上工 / O:已下工(等待上工)</returns>
        public string Get_PerLoginStatus(string pPerCode,string pMacCode)
        {
            //取得目前人員是否有在該機台的任何資料
            string sReturn = "";
            string sSql = "";
            sSql = "select top 1 * from MED01_0100 " +
                   " where usr_code='" + pPerCode + "'" +
                   "   and mac_code='" + pMacCode + "'" +
                   "   and date_s='" + comm.Get_Date() + "'" +
                   " order by date_s desc , time_s desc ";
            DataTable dtTmp = comm.Get_DataTable(sSql);

            if (dtTmp.Rows.Count > 0)
            {
                //有上工記錄則要判斷是否為I，如果為I則
                sReturn = dtTmp.Rows[0]["status"].ToString().Trim();
            }
            else
            {
                //完全沒有上工記錄則回傳目前要上工
                sReturn = "O";
            }
            return sReturn;
        }

        public string Get_med01_0100(string pPerCode, string pMacCode)
        {
            //取得目前人員是否有在該機台的任何資料
            string sReturn = "";
            string sSql = "";
            sSql = "select top 1 * from MED01_0100 " +
                   " where usr_code='" + pPerCode + "'" +
                   "   and mac_code='" + pMacCode + "'" +
                   "   and date_s='" + comm.Get_Date() + "'" +
                   "   and status='I'";
            DataTable dtTmp = comm.Get_DataTable(sSql);

            if (dtTmp.Rows.Count > 0)
            {
                sReturn = dtTmp.Rows[0]["med01_0100"].ToString().Trim();
            }
            else
            {
                sReturn = "0";
            }
            return sReturn;
        }

        public string Get_MoCodeByMacCode(string pMacCode)
        {
            //取得目前機台正在生產的工單
            string sReturn = "";
            string sSql = "";
            sSql = "select top 1 * from MED02_0000 " +
                   " where mac_code='" + pMacCode + "'" +
                   "   and mo_status_wrk='IN'" +
                   " order by ins_date desc , ins_time desc";
            DataTable dtTmp = comm.Get_DataTable(sSql);

            if (dtTmp.Rows.Count > 0)
            {
                sReturn = dtTmp.Rows[0]["mo_code"].ToString().Trim();
            }
            else
            {
                sReturn = "";
            }
            return sReturn;
        }

        public string Get_WrkCodeByMacCode(string pMacCode)
        {
            //取得目前機台正在生產的工單
            string sReturn = "";
            string sSql = "";
            sSql = "select top 1 * from MED02_0000 " +
                   " where mac_code='" + pMacCode + "'" +
                //   "   and mo_status_wrk='IN'" +
                   " order by ins_date desc , ins_time desc";
            DataTable dtTmp = comm.Get_DataTable(sSql);

            if (dtTmp.Rows.Count > 0)
            {
                if (dtTmp.Rows[0]["mo_status_wrk"].ToString().Trim() == "IN")
                {
                    sReturn = dtTmp.Rows[0]["wrk_code"].ToString().Trim();

                }
                else {
                    sReturn = "";
                }
            }
            else
            {
                sReturn = "";
            }
            return sReturn;
        }

        public string Get_ProQtyByMocode(string pMoCode,string pWrkCode)
        {
            //取得單一工單目前已生產量
            string sReturn = "";
            string sSql = "";
            sSql = "select sum(pro_qty) as sum_qty from MED09_0000 " +
                   " where mo_code='" + pMoCode + "'" +
                   "   and wrk_code='" + pWrkCode + "'" +
                   " group by mo_code ";
            DataTable dtTmp = comm.Get_DataTable(sSql);

            if (dtTmp.Rows.Count > 0)
            {
                sReturn = dtTmp.Rows[0]["sum_qty"].ToString().Trim();
            }
            else
            {
                sReturn = "0";
            }
            return sReturn;
        }

        public string Get_StationCodeByMacCode(string pMacCode)
        {
            //依據機台編號取得站別編號
            string sReturn = "";
            string sSql = "";
            sSql = "select station_code  from MEB29_0200 " +
                   " where mac_code='" + pMacCode + "'";
            DataTable dtTmp = comm.Get_DataTable(sSql);

            if (dtTmp.Rows.Count > 0)
            {
                sReturn = dtTmp.Rows[0]["station_code"].ToString().Trim();
            }
            else
            {
                sReturn = "0";
            }
            return sReturn;
        }
        public string Get_WorkCodeByStationCode(string pStationCode)
        {
            //取得單一工單目前已生產量
            string sReturn = "";
            string sSql = "";
            sSql = "select work_code  from MEB30_0100 " +
                   " where station_code='" + pStationCode + "'";
            DataTable dtTmp = comm.Get_DataTable(sSql);

            if (dtTmp.Rows.Count > 0)
            {
                sReturn = dtTmp.Rows[0]["work_code"].ToString().Trim();
            }
            else
            {
                sReturn = "0";
            }
            return sReturn;
        }


        //public string GetPerCodeByMacCode(string pMacCode)
        //{
        //    //取得目前機台正在上工的人員
        //    string sReturn = "";
        //    string sSql = "";
        //    sSql = "select top 1 * from MED01_0100 " +
        //           " where mac_code='" + pMacCode + "'" +
        //           "   and date_s='" + comm.Get_Date() + "'" +
        //           "   and status='I'" +
        //           " order by date_s desc , time_s desc ";
        //    DataTable dtTmp = comm.Get_DataTable(sSql);

        //    if (dtTmp.Rows.Count > 0)
        //    {
        //        sReturn = dtTmp.Rows[0]["usr_code"].ToString().Trim();
        //    }
        //    else
        //    {
        //        sReturn = "";
        //    }
        //    return sReturn;
        //}


        public string Get_UserCodeByMacCode(string pMacCode)
        {
            //取得目前人員是否有在該機台的任何資料
            string sReturn = "";
            string sSql = "";
            sSql = "select top 1 * from MED01_0100 " +
                   " where mac_code='" + pMacCode + "'" +
                   "   and date_s='" + comm.Get_Date() + "'" +
                   "   and status='I'";
            DataTable dtTmp = comm.Get_DataTable(sSql);

            if (dtTmp.Rows.Count > 0)
            {
                sReturn = dtTmp.Rows[0]["usr_code"].ToString().Trim();
            }
            else
            {
                sReturn = "";
            }
            return sReturn;
        }

        public void Upd_PerLogOut(string pMED01_0100)
        {
            string sSql = "";

            sSql = "update MED01_0100 set " +
                   " date_e='" + comm.Get_Date() + "'," +
                   " time_e='" + comm.Get_Time() + "'," +
                   " status='O'" +
                   " where med01_0100=" + pMED01_0100 + "";
            comm.Connect_DB(sSql);
        }

        /// <summary>
        /// 更新派工單狀態
        /// </summary>
        /// <param name="pWrkCcode">派工單號</param>
        /// <param name="pStatus">狀態 STOP:暫停 / END:結束 / NONE:尚未開始 / IN:生產中</param>
        public void Upd_WrkStatus(string pWrkCcode, string pStatus)
        {
            string sSql = "";

            sSql = "update MET03_0000 set mo_status='" + pStatus + "' where wrk_code='" + pWrkCcode + "'";
            comm.Connect_DB(sSql);
        }

        public void Upd_MEM01Data(string pMoCode,string pWorkCode,string pField,string pValue)
        {
            string sSql = "";

            sSql = "update MEM01_0000 set " + pField + "='" + pValue + "' where work_code='" + pWorkCode + "' and mo_code='" + pMoCode + "'";
            comm.Connect_DB(sSql);
        }

        //public DataTable Get_LoginData(string pMacCode)
        //{
        //    string sSql = "select * from MED01_0000" +
        //                  " where ins_date ='" + comm.Get_Date() + "'" +
        //                  "   and mac_code='" + pMacCode + "'" +
        //                  "  order by ins_date desc,ins_time desc";
        //    DataTable dtTmp = comm.Get_DataTable(sSql);
        //    return dtTmp;
        //}



        //---------------------------
        /// <summary>
        /// 傳入一個設備的mac adress取得目前上工機台
        /// </summary>
        /// <returns></returns>
        public string Get_MacCodeByMacAddress()
        {
            string sMacAddress = Get_MacAddress();
            string sReturn = "";
            string sSql = "";

            sSql = "select mac_code from MEM04_0000 where station_mac='" + sMacAddress + "'";
            DataTable dtTmp = comm.Get_DataTable(sSql);
            if (dtTmp.Rows.Count > 0)
            {
                sReturn = dtTmp.Rows[0]["mac_code"].ToString().Trim();
            }
            return sReturn;
        }

        /// <summary>
        /// 取得設備第一張網卡的mac address
        /// </summary>
        /// <returns></returns>
        public string Get_MacAddress()
        {
            string sReturn = "";
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            //List<string> macList = new List<string>();
            foreach (var nic in nics)
            {
                // 因為電腦中可能有很多的網卡(包含虛擬的網卡)，
                // 我只需要 Ethernet 網卡的 MAC
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    //sReturn = nic.GetPhysicalAddress().ToString();
                    sReturn= System.Web.HttpContext.Current.Request.UserHostAddress;
                    //macList.Add(nic.GetPhysicalAddress().ToString());
                }
            }

            return sReturn;
        }

        /// <summary>
        /// 更新機台使用MacAddress
        /// </summary>
        /// <param name="pMacCcode">機台代碼</param>
        /// <param name="pMacAddress">機台MAC</param>
        public void Upd_MacAddress(string pMacCcode,string pMacAddress)
        {
            string sSql = "";
            string sStationCode = comm.Get_QueryData("MEB30_0100", pMacCcode, "station_code", "work_code");

            //sSql = "update MEM04_0000 set mac_code='" + pMacCcode + "' where station_mac='" + pMacAddress + "'";
            sSql = " update MEM04_0000 " +
                   "    set station_mac=''," +
                   "    station_ip=''" +
                   "   where station_mac='" + pMacAddress + "'";
            comm.Connect_DB(sSql);
            Ins_MacAddress(pMacCcode, pMacAddress);
        }
        public void Ins_MacAddress(string pMacCcode, string pMacAddress)
        {
            string sSql = "";
            string sStationCode = comm.Get_QueryData("MEB30_0100", pMacCcode, "station_code", "work_code");

            //sSql = "update MEM04_0000 set mac_code='" + pMacCcode + "' where station_mac='" + pMacAddress + "'";
            //確認是否有機台
            sSql="Select * From MEM04_0000  where mac_code='"+ pMacCcode+"'";
            DataTable tmpData =new DataTable();
            tmpData= comm.Get_DataTable(sSql);
            if (tmpData.Rows.Count == 0) {
                sSql = " INSERT INTO " +
                          " MEM04_0000 (  mac_code,  station_code,  station_ip,  station_mac ) " +
                          "     VALUES ('"+ pMacCcode + "','"+ sStationCode + "','"+ pMacAddress + "','"+ pMacAddress + "') "; ;
                comm.Connect_DB(sSql);
            }
            else{

                sSql = "update MEM04_0000 set station_mac='" + pMacAddress + "' ,station_ip='" + pMacAddress + "' where mac_code='" + pMacCcode + "'";
                comm.Connect_DB(sSql);
            }
        }
        ///// <summary>
        ///// 更新機台使用MacAddress
        ///// </summary>
        ///// <param name="pMacCcode">機台代碼</param>
        ///// <param name="pMacAddress">機台MAC</param>
        //public void Del_MacAddress(string pMacCcode)
        //{
        //    string sSql = "";

        //    sSql = "Delete MEM04_0000 where  mac_code='" + pMacCcode + "''";
        //    comm.Connect_DB(sSql);
        //}

    }
}
