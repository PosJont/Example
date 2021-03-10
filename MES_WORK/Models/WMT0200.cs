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
    public class WMT0200
    {
        public string wmt0200 { get; set; }
        public string lot_no { get; set; }
        public string rel_type { get; set; }
        public string rel_code { get; set; }
        public string ins_type { get; set; }
        public DateTime sto_date { get; set; }
        public string pro_code { get; set; }
        public decimal pro_qty { get; set; }
        public string sto_code { get; set; }
        public string loc_code { get; set; }
        public string scr_no { get; set; }
        public string cus_code { get; set; }
        public int wmt0100 { get; set; }
        public string container { get; set; }
        public string barcode { get; set; }
        public string sor_no { get; set; }
        public string tra_code { get; set; }
        public string identifier { get; set; }
        public string ins_user { get; set; }


        /// <summary>
        /// 傳入一個DTO，存檔，一次存檔一筆
        /// </summary>
        public void InsertData(WMT0200 WMT0200)
        {
            Comm comm = new Comm();
            string sSql = " INSERT INTO " +
                          " WMT0200 ( wmt0200, lot_no,  rel_type,  rel_code,  ins_type,  sto_date,  pro_code,  pro_qty,  sto_code,  loc_code,  scr_no,  cus_code, " +
                          "               wmt0100,  container,  barcode,  sor_no,  tra_code,  identifier,  ins_user ) " +

                          "     VALUES  ( @wmt0200,  @lot_no,  @rel_type,  @rel_code,  @ins_type,  @sto_date,  @pro_code,    @pro_qty,  @sto_code,  @loc_code,  @scr_no,  @cus_code, " +
                          "                @wmt0100, @container, @barcode,   @sor_no,    @tra_code,  @identifier,  @ins_user ) ";

            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, WMT0200);
            }
        }

        public void Ins_WMT02(string pWrkCode, string pLotNo, string pProCode, string pProQty
            , string pLocCode, string pRelType, string pInsType, string pPerCode)
        {
            Comm comm = new Comm();
            string wmt0200 = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
            DateTime sDateTime = DateTime.Parse(comm.Get_ServerDateTime("yyyy/MM/dd HH:mm:ss"));
            WMT0200 data_T0200 = new WMT0200()
            {
                wmt0200 = wmt0200,
                lot_no = pLotNo,
                rel_type = pRelType,
                rel_code = pWrkCode,
                ins_type = pInsType,
                sto_date = sDateTime,
                pro_code = pProCode,
                pro_qty = pProQty != "" ? decimal.Parse(pProQty) : 0,
                sto_code = "",
                loc_code = pLocCode,
                scr_no = "",
                cus_code = "",
                wmt0100 = 0,
                container = "",
                barcode = "",
                sor_no = "",
                tra_code = "",
                identifier = "",
                ins_user = pPerCode,
            };
            InsertData(data_T0200);
        }

        /// <summary>
        /// 傳入一個DTO，修改，一次修改一筆
        /// </summary>
        public void UpdateData(WMT0200 WMT0200)
        {
            Comm comm = new Comm();
            string sSql = " UPDATE WMT0200 " +
                          "    SET lot_no     =  @lot_no    ,  " +
                          "        rel_type   =  @rel_type  ,  " +
                          "        rel_code   =  @rel_code  ,  " +
                          "        ins_type   =  @ins_type  ,  " +
                          "        sto_date   =  @sto_date  ,  " +
                          "        pro_code   =  @pro_code  ,  " +
                          "        pro_qty    =  @pro_qty   ,  " +
                          "        sto_code   =  @sto_code  ,  " +
                          "        loc_code   =  @loc_code  ,  " +
                          "        scr_no     =  @scr_no    ,  " +
                          "        cus_code   =  @cus_code  ,  " +
                          "        wmt0100    =  @wmt0100   ,  " +
                          "        container  =  @container ,  " +
                          "        barcode    =  @barcode   ,  " +
                          "        sor_no     =  @sor_no    ,  " +
                          "        tra_code   =  @tra_code  ,  " +
                          "        identifier =  @identifier,  " +
                          "        ins_user   =  @ins_user     " +
                          "  WHERE wmt0200    =  @wmt0200     ";
            using (SqlConnection con_db = comm.Set_DBConnection())
            {
                con_db.Execute(sSql, WMT0200);
            }
        }
    }
}