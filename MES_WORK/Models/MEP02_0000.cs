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

    public class MEP02_0000
    {
        Comm comm = new Comm();

        [DisplayName("識別碼")]
        public int mep02_0000 { get; set; }

        [DisplayName("製令號碼")]
        public string mo_code { get; set; }

        [DisplayName("工單號碼")]
        public string wrk_code { get; set; }

        [DisplayName("途程代碼")]
        public string work_code { get; set; }

        [DisplayName("途程名稱")]
        public string work_name { get; set; }

        [DisplayName("站別代號")]
        public string station_code { get; set; }

        [DisplayName("站別名稱")]
        public string station_name { get; set; }

        [DisplayName("機台代號")]
        public string mac_code { get; set; }

        [DisplayName("機台名稱")]
        public string mac_name { get; set; }

        [DisplayName("人員代號")]
        public string usr_code { get; set; }

        [DisplayName("人員名稱")]
        public string usr_name { get; set; }

        [DisplayName("產品代碼")]
        public string pro_code { get; set; }

        [DisplayName("產品名稱")]
        public string pro_name { get; set; }

        [DisplayName("批號")]
        public string pro_lot_no { get; set; }

        [DisplayName("IOT上料量")]
        public decimal iot_use_qty { get; set; }

        [DisplayName("上料量")]
        public decimal use_qty { get; set; }

        [DisplayName("上料單位")]
        public string use_unit { get; set; }

        [DisplayName("IOT退料量")]
        public decimal iot_rtn_qty { get; set; }

        [DisplayName("退料量")]
        public decimal rtn_qty { get; set; }

        [DisplayName("退料單位")]
        public string rtn_unit { get; set; }

        [DisplayName("IOT使用量")]
        public decimal iot_total_qty { get; set; }

        [DisplayName("使用量")]
        public decimal total_qty { get; set; }

        [DisplayName("使用單位")]
        public string total_unit { get; set; }


        /// <summary>
        /// 傳入一個MEP02_0000的DTO，存檔，一次存檔一筆
        /// </summary>
        /// <param name="MEP02_0000">DTO</param>
        public void InsertData(MEP02_0000 MEP02_0000)
        {
            string sSql = "INSERT INTO " +
                          " MEP02_0000 (  mo_code,  wrk_code,  work_code,  station_code,  mac_code,  usr_code,  pro_code, " +
                          "               pro_lot_no,  iot_use_qty,  use_qty,  use_unit,  iot_rtn_qty,  rtn_qty,  rtn_unit, " +
                          "               iot_total_qty,  total_qty,  total_unit ) " +
                          "     VALUES ( @mo_code, @wrk_code, @work_code, @station_code, @mac_code, @usr_code, @pro_code, " +
                          "              @pro_lot_no, @iot_use_qty, @use_qty, @use_unit, @iot_rtn_qty, @rtn_qty, @rtn_unit, " +
                          "              @iot_total_qty, @total_qty, @total_unit ) ";

            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, MEP02_0000);
            }
        }

    }
}