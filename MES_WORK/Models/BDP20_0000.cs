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
    public class BDP20_0000
    {
        [DisplayName("資料代號")]
        public Int32 bdp20_0000 { get; set; }

        [DisplayName("使用者代號")]
        public string usr_code { get; set; }


        [DisplayName("程式代號")]
        public string prg_code { get; set; }

        [DisplayName("使用日期")]
        public string usr_date { get; set; }

        [DisplayName("使用時間")]
        public string usr_time { get; set; }

        [DisplayName("使用類型")]
        public string usr_type { get; set; }

        [DisplayName("使用備註")]
        public string cmemo { get; set; }

        [DisplayName("參數值JSON")]
        public string params_json { get; set; }

        [DisplayName("是否能刪除(控制用)")]
        public string can_delete { get; set; }

        [DisplayName("是否能修改(控制用)")]
        public string can_update { get; set; }
    }
}