using Blog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using System.Configuration;
using System.Net.Mail;

namespace Blog.Controllers
{
    public class HomeController : Controller
    {
        mvcblogDB db = new mvcblogDB();


        public ActionResult Index(int Page = 1)
        {
            var makale = db.Makales.OrderByDescending(i => i.MakaleId).ToPagedList(Page, 5);
            return View(makale);
        }

        public ActionResult BlogAra(string Ara = null)
        {
            var aranan = db.Makales.Where(m => m.Baslik.Contains(Ara)).ToList();

            return View(aranan.OrderByDescending(m => m.Tarih));
        }

        public ActionResult PopulerMakaleler()
        {

            return View(db.Makales.OrderByDescending(m => m.Okunma).Take(5));
        }

        public ActionResult SonYorumlar()
        {

            return View(db.Yorums.OrderByDescending(y => y.YorumId).Take(5));
        }
        public ActionResult KategoriMakale(int id)
        {
            var makaleler = db.Makales.Where(m => m.Kategori.KategoriId == id).ToList();
            return View(makaleler);
        }


        public ActionResult MakaleDetay(int id)
        {
            var makale = db.Makales.Where(m => m.MakaleId == id).SingleOrDefault();
            if (makale == null)
            {
                return HttpNotFound();
            }
            return View(makale);
        }
        public ActionResult Hakkimizda()
        {
            return View();
        }

        public ActionResult Iletisim()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Iletisim(Iletisim model)
        {
            mvcblogDB db = new mvcblogDB();
            if (model.AdSoyad != null && model.Email != null && model.Mesaj != null && model.Konu != null)
            {
                db.Iletisims.Add(model);
                db.SaveChanges();
            }

            string server = ConfigurationManager.AppSettings["server"];
            int port = int.Parse(ConfigurationManager.AppSettings["port"]);
            bool ssl = ConfigurationManager.AppSettings["ssl"].ToString() == "1" ? true : false;
            string from = ConfigurationManager.AppSettings["from"];
            string password = ConfigurationManager.AppSettings["password"];
            string fromname = ConfigurationManager.AppSettings["fromname"];
            string to = ConfigurationManager.AppSettings["to"];

            var client = new SmtpClient();
            client.Host = server;
            client.Port = port;
            client.EnableSsl = ssl;
            client.UseDefaultCredentials = true;
            client.Credentials = new System.Net.NetworkCredential(from, password);

            var email = new MailMessage();
            email.From = new MailAddress(from, fromname);
            email.To.Add(to);

            email.Subject = model.Konu;
            email.IsBodyHtml = true;
            email.Body = $"ad soyad : { model.AdSoyad} <br /> konu : {model.Konu} <br /> mesaj : {model.Mesaj} <br /> email : {model.Email}";

            try
            {
                client.Send(email);
                ViewData["result"] = true;
            }
            catch (Exception)
            {
                ViewData["result"] = false;
            }

            return View();
        }

        public ActionResult KategoriPartial()
        {
            return View(db.Kategoris.ToList());
        }

        public JsonResult YorumYap(string yorum, int Makaleid)
        {
            var uyeid = Session["uyeid"];
            if (yorum == null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            db.Yorums.Add(new Yorum { UyeId = Convert.ToInt32(uyeid), MakaleId = Makaleid, Icerik = yorum, Tarih = DateTime.Now });

            db.SaveChanges();

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult YorumSil(int id)
        {
            var uyeid = Session["uyeid"];
            var yorum = db.Yorums.Where(y => y.YorumId == id).SingleOrDefault();
            var makale = db.Makales.Where(m => m.MakaleId == yorum.MakaleId).SingleOrDefault();
            if (yorum.UyeId == Convert.ToUInt32(uyeid))
            {
                db.Yorums.Remove(yorum);
                db.SaveChanges();
                return RedirectToAction("MakaleDetay", "Home", new { id = makale.MakaleId });
            }
            else
            {
                return HttpNotFound();
            }
        }

        public ActionResult OkunmaArttir(int Makaleid)
        {
            var makale = db.Makales.Where(m => m.MakaleId == Makaleid).SingleOrDefault();
            if (makale.Okunma == null)
            {
                makale.Okunma = 0;
            }
            makale.Okunma += 1;
            db.SaveChanges();
            return View();
        }

    }
}