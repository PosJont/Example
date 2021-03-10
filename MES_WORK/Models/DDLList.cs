using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace MVC.Models
{
    public class DDLList
    {
        [DisplayName("選項值")]
        public string field_code { get; set; }

        [DisplayName("選項名稱")]
        public string field_name { get; set; }

        [DisplayName("呈現方式")]
        public string show_type { get; set; }

    }
}