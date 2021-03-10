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
    public class MEM04_0000
    {
        [Key]
        [DisplayName("識別碼")]
        public int mem04_0000 { get; set; }

        [DisplayName("機器代碼")]
        public string mac_code { get; set; }

        [DisplayName("站別代號")]
        public string station_code { get; set; }

        [DisplayName("工站IP")]
        public string station_ip { get; set; }

        [DisplayName("工站MAC")]
        public string station_mac { get; set; }

        public string ip { get; set; }

        /// <summary>
        /// 傳入一個DTO，存檔，一次存檔一筆
        /// </summary>
        public void InsertData(MEM04_0000 MEM04_0000)
        {
            Comm comm = new Comm();
            string sSql = " INSERT INTO " +
                          " MEM04_0000 (  mac_code,  station_code,  station_ip,  station_mac ) " +

                          "     VALUES ( @mac_code, @station_code, @station_ip, @station_mac ) ";
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, MEM04_0000);
            }
        }

        /// <summary>
        /// 傳入一個DTO，修改，一次修改一筆
        /// </summary>
        public void UpdateData(MEM04_0000 MEM04_0000)
        {
            Comm comm = new Comm();
            string sSql = " UPDATE MEM04_0000" +
                          "    SET mac_code     = @mac_code,     " +
                          "        station_code = @station_code, " +
                          "        station_ip   = @station_ip,   " +
                          "        station_mac  = @station_mac   " +
                          "  WHERE mem04_0000   = @mem04_0000    " ;
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, MEM04_0000);
            }
        }
    }
}





