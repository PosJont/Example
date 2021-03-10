using MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Dapper;
using MES_WORK.Models;

namespace MVC.Models
{
    public class Mail
    {

        Comm comm = new Comm();
        //ModTable MT = new ModTable();
        /// <summary>
        /// 新增寄信排程
        /// </summary>
        /// <param name="pPrgCode">詢價單類型(寫死)</param>
        /// <param name="pTkCode">詢價單號</param>
        public void Ins_MailLog(string pPrgCode,string pTkCode)
        {
            string sSql = "";
            string sGuid = "";
            MailLog Data = new MailLog();
            switch (pPrgCode)
            {
                case "MFT02_0100": //詢價單mail to 供應商       
                    //string[] sMailUsr = Get_MailUsr(pTkCode).Split(',');               
                    //for (int i = 0;i < sMailUsr.Length;i++) {
                    //    sGuid = comm.Get_Guid();//一個人建一組32碼流水號
                    //    Data = new MailLog {                           
                    //        tk_code = pTkCode,//tk_code﹕詢價單號 pTkCode                            
                    //        email = Get_UsrData(sMailUsr[i],"usr_mail"),//收件人﹕詢價單的供應商連絡資訊 用供應商代號去BDP08_0000找usr_code的usr_mail                            
                    //        is_send = "N",//is_send : 固定N                            
                    //        sch_time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),//sch_time:當下時間                            
                    //        send_time = null,//send_time:空值                            
                    //        week_day = DateTime.Now.DayOfWeek.ToString("d"),//week_day:當下周間日                            
                    //        mail_title = "[I-Direct 詢價通知] 詢價單號: " + pTkCode + "  " + Get_UsrData(sMailUsr[i], "usr_name"),//mail_title﹕[I-Direct 詢價通知] + 詢價單號 + 廠商名稱
                    //        mail_body = "親愛的供應商，您好:<br><br>  我們有一張新的工單需要您的報價<br><br>  請在"+ comm.Get_QueryData("MFT02_0000", pTkCode,"inq_code","limit_date") + "前回到我們的供應鏈平台進行報價<br><br>  您可以立即點選下列網址<br><br>  進行詢價確認<br><br><a href='http://test.startnet.com.tw:232/MailLogin/?pToken=" + sGuid + "'>  供應鏈管理系統</a>",
                    //        usr_code = sMailUsr[i],//usr_code:收件者
                    //        token = sGuid,
                    //        mail_type = "A01",
                    //    };
                    //    sSql = "insert into " +
                    //                  "  mailLog ( tk_code, email, is_send, sch_time, send_time, week_day, mail_title, mail_body, usr_code, token, mail_type) " +
                    //                  "  values  (@tk_code,@email,@is_send,@sch_time,@send_time,@week_day,@mail_title,@mail_body,@usr_code,@token,@mail_type) ";
                    //    using (SqlConnection con_db = comm.Set_DBConnection())
                    //    { con_db.Execute(sSql, Data); }
                    //}                   
                    break;
                case "MFT01_0100":
                    //TkCode = 製令製程   
                    //完工的信是要寄給自己        
                    //sSql = "select * from MFT01_0100 " +
                    //       " where mod_code = '" + pTkCode + "'";
                    //var dtTmp = comm.Get_DataTable(sSql);
                    
                    ////檢查是否有下一個順序
                    //if (MT.Get_ModOrder(pTkCode,"U") != "") { 
                    //    sGuid = comm.Get_Guid();//一個人建一組32碼流水號
                    //    Data = new MailLog
                    //    {
                    //        tk_code = pTkCode,//tk_code﹕詢價單號 pTkCode                            
                    //        email = Get_UsrData(dtTmp.Rows[0]["sch_process"].ToString(), "usr_mail"),//收件人﹕詢價單的供應商連絡資訊 用供應商代號去BDP08_0000找usr_code的usr_mail                            
                    //        is_send = "N",//is_send : 固定N                            
                    //        sch_time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),//sch_time:當下時間                            
                    //        send_time = null,//send_time:空值                            
                    //        week_day = DateTime.Now.DayOfWeek.ToString("d"),//week_day:當下周間日                            
                    //        mail_title = "[I-Direct 通知] 製令製程: " + pTkCode + "  已完工  " + Get_UsrData(dtTmp.Rows[0]["sch_process"].ToString(), "usr_name"),//mail_title﹕[I-Direct 詢價通知] + 詢價單號 + 廠商名稱
                    //        mail_body = "親愛的供應商，您好:<br><br>  您的製令製程生產已完工<br><br>  請送貨至地點: <br><br>  出示以下QRCode<br><br>  <img src='https://chart.googleapis.com/chart?chs=180x180&cht=qr&chl="+ MT.Get_ModOrder(pTkCode,"U") +"&chld=L|4'>",
                    //        usr_code = dtTmp.Rows[0]["sch_process"].ToString(),//usr_code:收件者
                    //        token = sGuid,
                    //        mail_type = "A02",
                    //    };
                    //    sSql = "insert into " +
                    //                    "  mailLog ( tk_code, email, is_send, sch_time, send_time, week_day, mail_title, mail_body, usr_code, token, mail_type) " +
                    //                    "  values  (@tk_code,@email,@is_send,@sch_time,@send_time,@week_day,@mail_title,@mail_body,@usr_code,@token,@mail_type) ";
                    //    using (SqlConnection con_db = comm.Set_DBConnection())
                    //    { con_db.Execute(sSql, Data); }
                    //}
                    
                    break;
                //case "SUB020A": //供應商評鑑
                    //sKey = "EV" + DateTime.Now.ToString("yyyyMMdd");
                    //sNo = Get_AutoIntMax("SUB02_0000", "RIGHT(eva_code, 4)", "And eva_code LIKE '" + sKey + "%'") + 1;
                    //sCode = sKey + StrRigth("0000" + sNo.ToString(), 4);
                    //break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 利用詢價單取得廠商代號
        /// </summary>
        /// <param name="pSupCode">供應商代號</param>
        /// <param name="pFieldCode">欄位</param>
        /// <returns></returns>
        public string Get_MailUsr(string pTkCode)
        {
            string sSql = "select * " +
                          "  from MFT02_0100 " +
                          " where inq_code = '" + pTkCode + "' ";
            return DataFieldToStr(sSql, "TA006");
        }


        /// <summary>
        /// 取得使用者資訊
        /// </summary>
        /// <param name="pUsrCode">使用者代號</param>
        /// <param name="pFieldCode">欄位</param>
        /// <returns></returns>
        public string Get_UsrData(string pUsrCode, string pFieldCode)
        {
            string sSql = "select * from BDP08_0000 " +
                          " where usr_code = '" + pUsrCode + "' ";
            return DataFieldToStr(sSql, pFieldCode);
        }


        /// <summary>
        /// 利用SQL語法取得指定欄位的字串形式
        /// </summary>
        /// <param name="pFieldCode">指定欄位</param>
        /// <returns></returns>
        public string DataFieldToStr(string pSqlStr, string pFieldCode)
        {
            var dtTmp = comm.Get_DataTable(pSqlStr);
            string sValue = "";
            for (int i = 0; i < dtTmp.Rows.Count; i++)
            {
                if (i > 0) { sValue += ","; };
                sValue += dtTmp.Rows[i][pFieldCode].ToString();
            }
            return sValue;
        }


        private class MailLog
        {
            public string tk_code { get; set; }
            public string email { get; set; }
            public string is_send { get; set; }
            public string sch_time { get; set; }
            public string send_time { get; set; }
            public string week_day { get; set; }
            public string mail_title { get; set; }
            public string mail_body { get; set; }
            public string usr_code { get; set; }
            public string token { get; set; }
            public string mail_type { get; set; }
        }
    }
}