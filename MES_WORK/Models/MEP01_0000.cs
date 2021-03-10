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
    public class MEP01_0000
    {
        Comm comm = new Comm();

        [DisplayName("識別碼")]
        public int mep01_0000 { get; set; }

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

        [DisplayName("IOT良品量")]
        public decimal iot_ok_qty { get; set; }

        [DisplayName("良品量")]
        public decimal ok_qty { get; set; }

        [DisplayName("良品單位")]
        public string ok_unit { get; set; }

        [DisplayName("IOT不良品量")]
        public decimal iot_ng_qty { get; set; }

        [DisplayName("不良品量")]
        public decimal ng_qty { get; set; }

        [DisplayName("不良品單位")]
        public string ng_unit { get; set; }



        /// <summary>
        /// 傳入一個MEP01_0000的DTO，存檔，一次存檔一筆
        /// </summary>
        /// <param name="MEP01_0000">DTO</param>
        public void InsertData(MEP01_0000 MEP01_0000)
        {
            string sSql = "INSERT INTO " +
                          " MEP01_0000 (  mo_code,  wrk_code,  work_code,  station_code,  mac_code,  usr_code,  pro_code, " +
                          "               pro_lot_no,  iot_ok_qty,  ok_qty,  ok_unit,  iot_ng_qty,  ng_qty,  ng_unit ) " +
                          "     VALUES ( @mo_code, @wrk_code, @work_code, @station_code, @mac_code, @usr_code, @pro_code, " +
                          "              @pro_lot_no, @iot_ok_qty, @ok_qty, @ok_unit, @iot_ng_qty, @ng_qty, @ng_unit )";

            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, MEP01_0000);
            }
        }


    }
}