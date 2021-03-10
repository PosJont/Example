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


    public class MED01_0100
    {
        [Key]
        [DisplayName("識別碼")]
        public int med01_0100 { get; set; }

        [DisplayName("工單編號")]
        public string mo_code { get; set; }

        [DisplayName("工單號碼")]
        public string wrk_code { get; set; }

        [DisplayName("機台代碼")]
        public string mac_code { get; set; }

        [DisplayName("機台名稱")]
        public string mac_name { get; set; }

        [DisplayName("使用者編號")]
        public string usr_code { get; set; }

        [DisplayName("上工日期")]
        public string date_s { get; set; }

        [DisplayName("上工時間")]
        public string time_s { get; set; }

        [DisplayName("下工日期")]
        public string date_e { get; set; }

        [DisplayName("下工時間")]
        public string time_e { get; set; }

        [DisplayName("狀態")]
        public string status { get; set; }


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
        public void InsertData(MED01_0100 MED01_0100)
        {
            Comm comm = new Comm();
            string sSql = " INSERT INTO " +
                          " MED01_0100 (  mo_code,  wrk_code,  mac_code,  usr_code,  date_s,  time_s,  date_e,  time_e , status ,"         +
                          "               user_field_01, user_field_02, user_field_03, user_field_04, user_field_05,    " +
                          "               user_field_06, user_field_07, user_field_08, user_field_09, user_field_10 )    " +
                          "     VALUES ( @mo_code, @wrk_code, @mac_code, @usr_code, @date_s, @time_s, @date_e, @time_e , @status ,     "  +
                           "             @user_field_01, @user_field_02, @user_field_03, @user_field_04, @user_field_05,    " +
                           "             @user_field_06, @user_field_07, @user_field_08, @user_field_09, @user_field_10 )    "; ; 
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, MED01_0100);
            }
        }

        /// <summary>
        /// 傳入一個DTO，修改，一次修改一筆
        /// </summary>
        public void UpdateData(MED01_0100 MED01_0100)
        {
            Comm comm = new Comm();
            string sSql = " UPDATE MED01_0100 " +
                          "    SET mo_code    =  @mo_code,   " +
                          "        wrk_code   =  @wrk_code,  " +
                          "        mac_code   =  @mac_code,  " +
                          "        usr_code   =  @usr_code,  " +
                          "        date_s     =  @date_s,    " +
                          "        time_s     =  @time_s,    " +
                          "        date_e     =  @date_e,    " +
                          "        time_e     =  @time_e,    " +
                          "        status     =  @status     " +                      
                          "  WHERE med01_0100 =  @med01_0100 ";
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, MED01_0100);
            }
        }
    }
    
    //string sSql = "INSERT INTO " +
    //              " MED01_0100 (  mo_code,  wrk_code,  mac_code,   time_e,  ins_date,  ins_time,  usr_code,  login_status )   " +
    //              "     VALUES ( @mo_code, @wrk_code, @mac_code,  @ins_date, @ins_time, @usr_code, @login_status )   ";
    
}
