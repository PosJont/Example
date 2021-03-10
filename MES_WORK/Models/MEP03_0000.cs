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
    public class MEP03_0000
    {
        Comm comm = new Comm();

        [DisplayName("識別碼")]
        public int mep03_0000 { get; set; }

        [DisplayName("製令號碼")]
        public string mo_code { get; set; }

        [DisplayName("工單號碼")]
        public string wrk_code { get; set; }

        [DisplayName("人員代號")]
        public string usr_code { get; set; }

        [DisplayName("人員名稱")]
        public string usr_name { get; set; }

        [DisplayName("工作時間(秒)")]
        public decimal work_second { get; set; }

        /// <summary>
        /// 傳入一個MEP03_0000的DTO，存檔，一次存檔一筆
        /// </summary>
        /// <param name="MEP03_0000">DTO</param>
        public void InsertData(MEP03_0000 MEP03_0000)
        {
            string sSql = "INSERT INTO " +
                          " MEP03_0000 (  mo_code,  wrk_code,  usr_code,  work_second ) " +
                          "     VALUES ( @mo_code, @wrk_code, @usr_code, @work_second ) ";

            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, MEP03_0000);
            }
        }
    }
}