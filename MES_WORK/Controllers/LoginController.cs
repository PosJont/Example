using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MES_WORK.Models;
using System.Data;
using System.Web.Security;
using System.Text.RegularExpressions;

namespace MES_WORK.Controllers
{
    public class LoginController : Controller
    {
        // GET: LoginTimeOut
        public ActionResult Index(string usr_code, string usr_pass)
        {
            //ViewBag.prj_name = comm.Get_QueryData("BDP00_0000", "prj_name", "par_name", "par_value");
            //ViewBag.com_name = comm.Get_QueryData("BDP00_0000", "com_name", "par_name", "par_value");
            //ViewBag.Message = tk_code;
            return View();

        }

        [HttpPost]
        public ActionResult Index(FormCollection post)
        {
            //var obj = Server.CreateObject("Snfun1.dll");

            // 根據Login Page輸入的組織代碼選擇DB連線

            Comm comm = new Comm();

            string sDbName = comm.Get_QueryData("BDP00_0000", "db_name_list", "par_name", "par_value");
            if (sDbName.IndexOf(post["db"].ToString().ToUpper()) >= 0)
            {
                //合法的DB
                Session["ChosenDB"] = post["db"].ToString().ToUpper();
            }
            else
            {
                ViewBag.Message = "登入失敗，請檢查您的組織代號";
                return View();
            }

            //ViewBag.prj_name = comm.Get_QueryData("BDP00_0000", "prj_name", "par_name", "par_value");
            //ViewBag.com_name = comm.Get_QueryData("BDP00_0000", "com_name", "par_name", "par_value");

            string usr_code = post["usr_code"];
            string usr_pass = post["usr_pass"];
            string usr_name = comm.Get_QueryData("BDP08_0000", usr_code, "usr_code", "usr_name");
            string grp_code = comm.Get_QueryData("BDP08_0000", usr_code, "usr_code", "grp_code");

            if (comm.Chk_Login(usr_code, usr_pass))
            {
                //Session["usr_name"] = usr_name;
                //Session["usr_code"] = usr_code;
                //登入成功 轉頁
                var ticket = new FormsAuthenticationTicket(
                version: 1,
                name: usr_code.ToString(), //可以放使用者Id
                issueDate: DateTime.UtcNow,//現在UTC時間
                expiration: DateTime.UtcNow.AddMinutes(30),//Cookie有效時間=現在時間往後+30分鐘
                isPersistent: true,// 是否要記住我 true or false
                userData: grp_code, //可以放使用者角色名稱
                cookiePath: FormsAuthentication.FormsCookiePath);

                var encryptedTicket = FormsAuthentication.Encrypt(ticket); //把驗證的表單加密
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                Response.Cookies.Add(cookie);


                // 儲存資訊到BDP20_0000
                comm.Ins_BDP20_0000(usr_code, "Login", "Login", "登入時間: " + comm.Get_Time());

                FormsAuthentication.RedirectFromLoginPage(usr_code, false);

                return RedirectToAction("Index", "Main", null);
                //return RedirectToAction("Index", "Blank", null);
            }
            ViewBag.Message = "登入失敗，請檢查帳號密碼";
            return View();
        }
    }
}