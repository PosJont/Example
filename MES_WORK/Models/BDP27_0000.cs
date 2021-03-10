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
    public class BDP27_0000
    {
        public string fun_code { get; set; }
        public string search_paras { get; set; }
        public string result { get; set; }
        public string data { get; set; }
        public string message { get; set; }
        public string ins_date { get; set; }
        public string ins_time { get; set; }
        public string usr_code { get; set; }


        /// <summary>
        /// 傳入一個DTO，存檔，一次存檔一筆
        /// </summary>
        public void InsertData(BDP27_0000 BDP27_0000)
        {
            Comm comm = new Comm();
            string sSql = "INSERT INTO " +
              " BDP27_0000 (  fun_code,  search_paras,  result,  data , message, ins_date, ins_time , usr_code)  " +
              "     VALUES (  @fun_code,  @search_paras,  @result,  @data , @message, @ins_date, @ins_time , @usr_code)  ";
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, BDP27_0000);
            }
        }

        ///// <summary>
        ///// 傳入一個DTO，修改，一次修改一筆
        ///// </summary>
        //public void UpdateData(MED00_0000 MED04_0000)
        //{
        //    Comm comm = new Comm();
        //    string sSql = " UPDATE MED04_0000 " +
        //                  "    SET mo_code       =  @mo_code,       " +
        //                  "        wrk_code      =  @wrk_code,      " +
        //                  "        mac_code      =  @mac_code,      " +
        //                  "        stop_code     =  @stop_code,     " +
        //                  "        date_s        =  @date_s,        " +
        //                  "        time_s        =  @time_s,        " +
        //                  "        date_e        =  @date_e,        " +
        //                  "        time_e        =  @time_e,        " +
        //                  "        ins_date      =  @ins_date,      " +
        //                  "        ins_time      =  @ins_time,      " +
        //                  "        usr_code      =  @usr_code,      " +
        //                  "        des_memo      =  @des_memo,      " +
        //                  "        is_ng         =  @is_ng,         " +
        //                  "        is_end        =  @is_end,        " +
        //                  "        end_memo      =  @end_memo,      " +
        //                  "        end_date      =  @end_date,      " +
        //                  "        end_time      =  @end_time,      " +
        //                  "        end_usr_code  =  @end_usr_code   " +
        //                  "  WHERE med04_0000    =  @med04_0000     ";
        //    using (SqlConnection con_db = comm.Set_DBConnection())
        //    {
        //        con_db.Execute(sSql, MED04_0000);
        //    }
        //}


    }
}