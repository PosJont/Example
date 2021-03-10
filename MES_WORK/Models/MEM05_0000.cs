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
    public class MEM05_0000
    {
        [Key]
        [DisplayName("識別碼")]
        public int mem05_0000 { get; set; }

        [DisplayName("工單編號")]
        public string mo_code { get; set; }

        [DisplayName("途程代碼")]
        public string work_code { get; set; }

        [DisplayName("站別代號")]
        public string station_code { get; set; }

        [DisplayName("機台代號")]
        public string mac_code { get; set; }

        [DisplayName("上料時間")]
        public string in_time { get; set; }

        [DisplayName("退料時間")]
        public string out_time { get; set; }

        [DisplayName("上料料號")]
        public string pro_code { get; set; }

        [DisplayName("上料批號")]
        public string lot_no { get; set; }

        [DisplayName("上料量")]
        public decimal in_qty { get; set; }

        [DisplayName("上料單位")]
        public string in_unit { get; set; }

        [DisplayName("退料量")]
        public decimal out_qty { get; set; }

        [DisplayName("退料單位")]
        public string out_unit { get; set; }



        [DisplayName("是否能刪除(控制用)")]
        public string can_delete { get; set; }

        [DisplayName("是否能修改(控制用)")]
        public string can_update { get; set; }

        /// <summary>
        /// 傳入一個DTO，存檔，一次存檔一筆
        /// </summary>
        public void InsertData(MEM05_0000 MEM05_0000)
        {
            Comm comm = new Comm();
            string sSql = " INSERT INTO " +
                          " MEM05_0000 (   mo_code,  work_code,  station_code,  mac_code,  in_time,       " +
                          "    out_time,  pro_code,    lot_no,   in_qty,     in_unit,       out_qty ,  out_unit )     " +

                          " VALUES (      @mo_code,  @work_code,  @station_code,  @mac_code,  @in_time,  " +
                          "  @out_time,  @pro_code,    @lot_no,   @in_qty,     @in_unit,       @out_qty ,  @out_unit ) ";

            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, MEM05_0000);
            }
        }

    }
}