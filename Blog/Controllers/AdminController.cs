using Blog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class AdminController : Controller
    {
        mvcblogDB db = new mvcblogDB();
        
        public ActionResult Index()
        {
            ViewBag.MakaleSayisi = db.Makales.Count();
            ViewBag.YorumSayisi = db.Yorums.Count();
            ViewBag.KategoriSayisi = db.Kategoris.Count();
            ViewBag.UyeSayisi = db.Uyes.Count();
            return View();
        }

        public ActionResult Logout()
        {
            Session["uyeid"] = null;
            Session.Abandon();

            return RedirectToAction("Index", "Home");
        }
    }
}