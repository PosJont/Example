using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Data;
using Dapper;

namespace MES_WORK.Models
{
    public class MED02_0000
    {
        [Key]
        [DisplayName("識別碼")]
        public int med02_0000 { get; set; }

        [DisplayName("工單編號")]
        public string mo_code { get; set; }

        [DisplayName("工單號碼")]
        public string wrk_code { get; set; }

        [DisplayName("機台代碼")]
        public string mac_code { get; set; }

        [DisplayName("機台名稱")]
        public string mac_name { get; set; }

        [DisplayName("工單狀態")]
        public string mo_status_wrk { get; set; }

        [DisplayName("工單狀態名稱")]
        public string mo_status_wrk_name { get; set; }

        [DisplayName("建立日期")]
        public string ins_date { get; set; }

        [DisplayName("建立時間")]
        public string ins_time { get; set; }

        [DisplayName("使用者編號")]
        public string usr_code { get; set; }

        [DisplayName("使用者名稱")]
        public string usr_name { get; set; }

        [DisplayName("資料說明")]
        public string des_memo { get; set; }

        [DisplayName("是否異常")]
        public string is_ng { get; set; }

        [DisplayName("強制結案")]
        public string is_end { get; set; }

        [DisplayName("結案說明")]
        public string end_memo { get; set; }

        [DisplayName("結案日期")]
        public string end_date { get; set; }

        [DisplayName("結案時間")]
        public string end_time { get; set; }

        [DisplayName("結案人員")]
        public string end_usr_code { get; set; }


        [DisplayName("是否能刪除(控制用)")]
        public string can_delete { get; set; }

        [DisplayName("是否能修改(控制用)")]
        public string can_update { get; set; }

        public string user_field_01 { get; set; }
        public string user_field_02 { get; set; }
        public string user_field_03 { get; set; }
        public string user_field_04 { get; set; }
        public string user_field_05 { get; set; }
        public string user_field_06 { get; set; }
        public string user_field_07 { get; set; }
        public string user_field_08 { get; set; }
        public string user_field_09 { get; set; }
        public string user_field_10 { get; set; }

        /// <summary>
        /// 傳入一個DTO，存檔，一次存檔一筆
        /// </summary>
        public void InsertData(MED02_0000 MED02_0000)
        {
            Comm comm = new Comm();
            string sSql = " INSERT INTO " +
                          " MED02_0000 (  mo_code,  wrk_code,  mac_code,  mo_status_wrk,  ins_date,  ins_time,  usr_code, " +
                          "               des_memo,  is_ng,  is_end,  end_memo,  end_date,  end_time,  end_usr_code,  " +
                          "               user_field_01, user_field_02, user_field_03, user_field_04, user_field_05,    " +
                          "               user_field_06, user_field_07, user_field_08, user_field_09, user_field_10 )    " +

                          "     VALUES ( @mo_code, @wrk_code, @mac_code, @mo_status_wrk, @ins_date, @ins_time, @usr_code, " +
                          "              @des_memo, @is_ng, @is_end, @end_memo, @end_date, @end_time, @end_usr_code,       "+
                          "             @user_field_01, @user_field_02, @user_field_03, @user_field_04, @user_field_05,    " +
                          "             @user_field_06, @user_field_07, @user_field_08, @user_field_09, @user_field_10 )    ";
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, MED02_0000);
            }
        }

        /// <summary>
        /// 傳入一個DTO，修改，一次修改一筆
        /// </summary>
        public void UpdateData(MED02_0000 MED02_0000)
        {
            Comm comm = new Comm();
            string sSql = " UPDATE MED02_0000 " +
                          "    SET mo_code       =  @mo_code,       " +
                          "        wrk_code      =  @wrk_code,      " +
                          "        mac_code      =  @mac_code,      " +
                          "        mo_status_wrk =  @mo_status_wrk, " +
                          "        ins_date      =  @ins_date,      " +
                          "        ins_time      =  @ins_time,      " +
                          "        usr_code      =  @usr_code,      " +
                          "        des_memo      =  @des_memo,      " +
                          "        is_ng         =  @is_ng,         " +
                          "        is_end        =  @is_end,        " +
                          "        end_memo      =  @end_memo,      " +
                          "        end_date      =  @end_date,      " +
                          "        end_time      =  @end_time,      " +
                          "        end_usr_code  =  @end_usr_code   " +
                          "        user_field_01  = @user_field_01,   " +
                          "        user_field_02  = @user_field_02,   " +
                          "        user_field_03  = @user_field_03,   " +
                          "        user_field_04  = @user_field_04,   " +
                          "        user_field_05  = @user_field_05,   " +
                          "        user_field_06  = @user_field_06,   " +
                          "        user_field_07  = @user_field_07,   " +
                          "        user_field_08  = @user_field_08,   " +
                          "        user_field_09  = @user_field_09,   " +
                          "        user_field_10  = @user_field_10   " +
                          "  WHERE med02_0000    =  @med02_0000     ";
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, MED02_0000);
            }
        }
        /// <summary>
        /// 更新MET01_0000開工時間
        /// </summary>
        /// <param name="pMoCode"></param>
        public void Upd_MET01_0000_MoStartDate(string pMoCode)
        {
            Comm comm = new Comm();
            DateTime NowDate = DateTime.Now;
            string sSql = "UPDATE MET01_0000" +
                          "   SET mo_start_date='" + NowDate.ToString("yyyy/MM/dd") + "'" +
                          "   WHERE mo_code='" + pMoCode + "'";
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql);
            }
        }

        /// <summary>
        /// 更新MET01_0000完工時間
        /// </summary>
        /// <param name="pMoCode"></param>
        public void Upd_MET01_0000_MoEndDate(string pMoCode)
        {
            Comm comm = new Comm();
            DateTime NowDate = DateTime.Now;
            string sSql = "UPDATE MET01_0000" +
                          "   SET mo_end_date='" + NowDate.ToString("yyyy/MM/dd") + "'" +
                          "   WHERE mo_code='" + pMoCode + "'";
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql);
            }
        }
        /// <summary>
        /// 取得工單中，最後的派工單號
        /// </summary>
        /// <param name="pMoCode"></param>
        /// <returns></returns>
        public string Get_LastWrkCode(string pMoCode)
        {
            Comm comm = new Comm();
            string sSql = "Select top 1 wrk_code from MET03_0000 " +
                         " where mo_code ='" + pMoCode + "'" +
                         " order by  wrk_code desc";
            DataTable dtTmp = comm.Get_DataTable(sSql);
            if (dtTmp.Rows.Count > 0)
            {
                string sWrkCode = dtTmp.Rows[0]["wrk_code"].ToString();
                return sWrkCode;
            }
            return "";
        }
    }
}