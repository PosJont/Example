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


    public class MEM01_0000
    {
        [Key]
        [DisplayName("資料識別碼")]
        public int mem01_0000 { get; set; }

        [DisplayName("製令號碼")]
        public string mo_code { get; set; }

        [DisplayName("途程代碼")]
        public string work_code { get; set; }

        [DisplayName("站別代號")]
        public string station_code { get; set; }

        [DisplayName("機台代號")]
        public string mac_code { get; set; }

        [DisplayName("工作開始時間")]
        public string work_time_s { get; set; }

        [DisplayName("工作結束時間")]
        public string work_time_e { get; set; }

        [DisplayName("良品量")]
        public float ok_qty { get; set; }

        [DisplayName("良品單位")]
        public string ok_unit { get; set; }

        [DisplayName("不良品量")]
        public float ng_qty { get; set; }

        [DisplayName("不良品單位")]
        public string ng_unit { get; set; }

        [DisplayName("工時")]
        public int work_sec { get; set; }

        [DisplayName("人員代號")]
        public string usr_code { get; set; }



        /// <summary>
        /// 傳入一個DTO，存檔，一次存檔一筆
        /// </summary>
        public void InsertData(MEM01_0000 MEM01_0000)
        {
            Comm comm = new Comm();
            string sSql = " INSERT INTO " +
                          " MEM01_0000 (  mo_code,  work_code , station_code , mac_code ,"+
                          "               work_time_s , work_time_e ,  ok_qty,  ok_unit , "+
                          "               ng_qty,  ng_unit,  work_sec ) " +
                          "     VALUES (  @mo_code, @work_code,@station_code,@mac_code, "+
                          "               @work_time_s, @work_time_e, @ok_qty, @ok_unit, "+
                          "               @ng_qty, @ng_unit, @work_sec ) ";
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, MEM01_0000);
            }
        }

        /// <summary>
        /// 傳入一個DTO，修改，一次修改一筆
        /// </summary>
        public void UpdateData(MEM01_0000 MEM01_0000)
        {
            Comm comm = new Comm();
            string sSql = " UPDATE MEM01_0000 " +
                          "    SET mo_code       =  @mo_code,      " +
                          "        work_code     =  @work_code,     " +
                          "        station_code  =  @station_code,     " +
                          "        mac_code      =  @mac_code,     " +
                          "        work_time_s   =  @work_time_s,     " +                          
                          "        work_time_e   =  @work_time_e,     " +
                          "        ok_qty        =  @ok_qty,   " +
                          "        ok_unit       =  @ok_unit,      " +
                          "        ng_qty        =  @ng_qty,     " +
                          "        ng_unit       =  @ng_unit,     " +
                          "        work_sec      =  @work_sec,     " +

                          "  WHERE MEM01_0000   =  @mem01_0000    ";
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, MEM01_0000);
            }
        }

        /// <summary>
        /// 傳入一個DTO，修改，一次修改一筆
        /// </summary>
        public void Update_NgData(MEM01_0000 MEM01_0000)
        {
            Comm comm = new Comm();
            string sSql = " UPDATE MEM01_0000 " +
                          "     SET   ng_qty    =  @ng_qty      " +
                          "     WHERE mo_code   =  @mo_code     " +
                          "     AND  work_code  =  @work_code   ";
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, MEM01_0000);
            }
        }

    }
}
