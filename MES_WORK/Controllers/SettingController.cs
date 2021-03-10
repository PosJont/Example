using MES_WORK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MES_WORK.Controllers
{
    public class SettingController : Controller
    {
        Comm comm = new Comm();
        Work iWork = new Work();
        // GET: Main
        public ActionResult Index()
        {
            return View();
        }
        public string pubMacCode()
        {
            return iWork.Get_MacCodeByMacAddress();
        }

        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            return View();
        }

    }
}