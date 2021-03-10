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


    public class MED09_0100
    {
        [Key]
        [DisplayName("識別碼")]
        public int med09_0000 { get; set; }

        [DisplayName("工單編號")]
        public string mo_code { get; set; }

        [DisplayName("工單號碼")]
        public string wrk_code { get; set; }

        [DisplayName("機台代碼")]
        public string mac_code { get; set; }

        [DisplayName("機台名稱")]
        public string mac_name { get; set; }

        [DisplayName("產品代碼")]
        public string pro_code { get; set; }

        [DisplayName("產品名稱")]
        public string pro_name { get; set; }

        [DisplayName("生產批號")]
        public string pro_lot_no { get; set; }

        [DisplayName("生產數量")]
        public double pro_qty { get; set; }

        [DisplayName("建立日期")]
        public string ins_date { get; set; }

        [DisplayName("建立時間")]
        public string ins_time { get; set; }

        [DisplayName("使用者編號")]
        public string usr_code { get; set; }

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
        public void InsertData(MED09_0000 MED09_0000)
        {
            Comm comm = new Comm();
            string sSql = " INSERT INTO " +
                          " MED09_0000 (  mo_code,  wrk_code,  mac_code,  pro_code,  pro_lot_no,  pro_qty,  ins_date,  ins_time,  usr_code, " +
                          "               des_memo,  is_ng,  is_end,  end_memo,  end_date,  end_time,  end_usr_code ) " +

                          "     VALUES ( @mo_code, @wrk_code, @mac_code, @pro_code, @pro_lot_no, @pro_qty, @ins_date, @ins_time, @usr_code, " +
                          "              @des_memo, @is_ng, @is_end, @end_memo, @end_date, @end_time, @end_usr_code ) ";
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, MED09_0000);
            }
        }

        /// <summary>
        /// 傳入一個DTO，修改，一次修改一筆
        /// </summary>
        public void UpdateData(MED09_0000 MED09_0000)
        {
            Comm comm = new Comm();
            string sSql = " UPDATE MED09_0000 " +
                          "    SET mo_code      =  @mo_code,      " +
                          "        wrk_code     =  @wrk_code,     " +
                          "        mac_code     =  @mac_code,     " +
                          "        pro_code     =  @pro_code,     " +
                          "        pro_lot_no   =  @pro_lot_no,   " +
                          "        pro_qty      =  @pro_qty,      " +
                          "        ins_date     =  @ins_date,     " +
                          "        ins_time     =  @ins_time,     " +
                          "        usr_code     =  @usr_code,     " +
                          "        des_memo     =  @des_memo,     " +
                          "        is_ng        =  @is_ng,        " +
                          "        is_end       =  @is_end,       " +
                          "        end_memo     =  @end_memo,     " +
                          "        end_date     =  @end_date,     " +
                          "        end_time     =  @end_time,     " +
                          "        end_usr_code =  @end_usr_code  " +
                          "  WHERE med09_0000   =  @med09_0000    ";
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, MED09_0000);
            }
        }

    }
}
