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


    public class MIX01_0000
    {

        [DisplayName("SKU")]
        public string sku_code { get; set; }

        [DisplayName("調配量")]
        public string amount_value { get; set; }

        [DisplayName("物料編號")]
        public string item_no { get; set; }

        [DisplayName("批號編號")]
        public string item_lot_no { get; set; }

        [DisplayName("建立日期")]
        public string ins_date { get; set; }

        [DisplayName("建立時間")]
        public string ins_time { get; set; }

        [DisplayName("使用者編號")]
        public string usr_code { get; set; }

        [DisplayName("狀態")]
        public string login_status { get; set; }

        [DisplayName("狀態名稱")]
        public string login_status_name { get; set; }

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

        [DisplayName("是否能刪除(控制用)")]
        public string can_delete { get; set; }

        [DisplayName("是否能修改(控制用)")]
        public string can_update { get; set; }



        /// <summary>
        /// 傳入一個DTO，存檔，一次存檔一筆
        /// </summary>
        public void InsertData(MIX01_0000 MIX01_0000)
        {
            Comm comm = new Comm();
            string sSql = " INSERT INTO " +
                          " MIX01_0000 (  mo_code,  wrk_code,  mac_code,  ins_date,  ins_time,  usr_code,  login_status, " +
                          "               des_memo,  is_ng,  is_end,  end_memo,  end_date,  end_time,  end_usr_code , " +
                          "               user_field_01, user_field_02, user_field_03, user_field_04, user_field_05,    " +
                          "               user_field_06, user_field_07, user_field_08, user_field_09, user_field_10 )    " +

                          "     VALUES ( @mo_code, @wrk_code, @mac_code, @ins_date, @ins_time, @usr_code, @login_status, " +
                          "              @des_memo, @is_ng, @is_end, @end_memo, @end_date, @end_time, @end_usr_code,  " +
                           "             @user_field_01, @user_field_02, @user_field_03, @user_field_04, @user_field_05,    " +
                           "             @user_field_06, @user_field_07, @user_field_08, @user_field_09, @user_field_10 )    ";
                        
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, MIX01_0000);
            }
        }

        /// <summary>
        /// 傳入一個DTO，修改，一次修改一筆
        /// </summary>
        public void UpdateData(MIX01_0000 MIX01_0000)
        {
            Comm comm = new Comm();
            string sSql = " UPDATE MIX01_0000 " +
                          "    SET mo_code      =  @mo_code,      " +
                          "        wrk_code     =  @wrk_code,     " +
                          "        mac_code     =  @mac_code,     " +
                          "        ins_date     =  @ins_date,     " +
                          "        ins_time     =  @ins_time,     " +
                          "        usr_code     =  @usr_code,     " +
                          "        login_status =  @login_status, " +
                          "        des_memo     =  @des_memo,     " +
                          "        is_ng        =  @is_ng,        " +
                          "        is_end       =  @is_end,       " +
                          "        end_memo     =  @end_memo,     " +
                          "        end_date     =  @end_date,     " +
                          "        end_time     =  @end_time,     " +
                          "        end_usr_code =  @end_usr_code,  " +
                          "        user_field_01  = @user_field_01,   " +
                          "        user_field_02  = @user_field_02,   " +
                          "        user_field_03  = @user_field_03,   " +
                          "        user_field_04  = @user_field_04,   " +
                          "        user_field_05  = @user_field_05,   " +
                          "        user_field_06  = @user_field_06,   " +
                          "        user_field_07  = @user_field_07,   " +
                          "        user_field_08  = @user_field_08,   " +
                          "        user_field_09  = @user_field_09,   " +
                          "        user_field_10  = @user_field_10   "+                                                              
                          "  WHERE med01_0000   =  @med01_0000    ";
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, MIX01_0000);
            }
        }

        /// <summary>
        /// 修改MEM01_0000人員上下工
        /// </summary>
        /// <param name="pMacCode"></param>
        /// <param name="pUsrCode"></param>
        /// <param name="pMoCode"></param>
        public void UPD_MEM01_UsrCode(string pMacCode, string pUsrCode, string pMoCode)
        {
            Comm comm = new Comm();
            string sSql = " UPDATE MEM01_0000 " +
                          " SET usr_code ='" + pUsrCode + "'" +
                          " WHERE mac_code='" + pMacCode + "'" +
                          "   AND mo_code='" + pMoCode + "'";

            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql);
            }
        }

    }
    
    //string sSql = "INSERT INTO " +
    //              " MIX01_0000 (  mo_code,  wrk_code,  mac_code,   time_e,  ins_date,  ins_time,  usr_code,  login_status )   " +
    //              "     VALUES ( @mo_code, @wrk_code, @mac_code,  @ins_date, @ins_time, @usr_code, @login_status )   ";
    
}
