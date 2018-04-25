using Blog.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class UyeController : Controller
    {
        mvcblogDB db = new mvcblogDB();

        // GET: Uye
        public ActionResult Index(int id)
        {
            var uye = db.Uyes.Where(u => u.UyeId == id).SingleOrDefault();
            if (Convert.ToInt32(Session["uyeid"]) != uye.UyeId)
            {
                return HttpNotFound();
            }

            return View(uye);
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Uye uye)
        {
            //var md5pass = Crypto.Hash(Sifre, "MD5");

            var login = db.Uyes.Where(u => u.Email == uye.Email).SingleOrDefault();
            if (login == null)
            {
                ViewBag.Uyarı = "Email Adresinizi veya Şifrenizi Kontrol Ediniz!!!";
                return View("Login");
            }
            else
            {
                if (login.Email == uye.Email && login.Sifre == uye.Sifre)
                {
                    Session["uyeid"] = login.UyeId;
                    Session["kullaniciadi"] = login.KullaniciAdi;
                    Session["yetkiid"] = login.YetkiId;
                    Session["profilFoto"] = login.Foto;

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Uyarı = "Email Adresinizi veya Şifrenizi Kontrol Ediniz!!!";
                    return View();
                }
            }


        }
        public ActionResult Duzenle()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Duzenle(Uye uyes)
        {
            var uye = db.Uyes.Where(x => x.Email == uyes.Email).SingleOrDefault();
            if (uye == null)
            {
                ViewBag.Uyari = "Sisteme kayıtlı böyle bir email adresi bulunamadı!!!";
                return View("Duzenle");
            }
            uye.Sifre = "123";
            db.SaveChanges();
            string konu = "Şifre Değişikliği";
            string mesaj = "Güvenliğiniz için şifrenizi değiştirmenizi öneririz. Size gönderdiğimiz şifreyle giriş yapınız ve profil ekranına gelerek şifrenizi güncelleyiniz." + "<br/>" + "yeni şifreniz: 123";
            try
            {
                SmtpClient client = new SmtpClient("mail.aziztalhadurgun.com", 587);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.EnableSsl = false;
                client.Credentials = new NetworkCredential
                {
                    UserName = "info@aziztalhadurgun.com",
                    Password = "Talha123."
                };
                MailAddress from = new MailAddress("info@aziztalhadurgun.com");
                MailAddress to = new MailAddress(uye.Email);

                MailMessage mm = new MailMessage(from, to);
                mm.Subject = konu;
                mm.Body = mesaj;
                mm.IsBodyHtml = true;
                client.Send(mm);
                
                return RedirectToAction("Duzenle");
            }
            catch (Exception e)
            {
                return RedirectToAction("Duzenle");
            }

        }

        public ActionResult Logout()
        {
            Session["uyeid"] = null;
            Session.Abandon();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Uye uye, HttpPostedFileBase Foto)
        {
            //var md5pass = Sifre;
            if (ModelState.IsValid)
            {
                var emailKontrol = db.Uyes.Where(e => e.Email == uye.Email).SingleOrDefault();
                if (emailKontrol == null)
                {
                    if (Foto != null)
                    {
                        WebImage img = new WebImage(Foto.InputStream);
                        FileInfo fotoinfo = new FileInfo(Foto.FileName);

                        string newfoto = Guid.NewGuid().ToString() + fotoinfo.Extension;
                        img.Resize(1000, 1000);
                        img.Save("~/Uploads/UyeFoto/" + newfoto);
                        uye.Foto = "/Uploads/UyeFoto/" + newfoto;
                        uye.YetkiId = 2;
                        //uye.Sifre = Crypto.Hash(md5pass, "MD5");
                        db.Uyes.Add(uye);
                        db.SaveChanges();

                        Session["uyeid"] = uye.UyeId;
                        Session["kullaniciadi"] = uye.KullaniciAdi;
                        Session["profilFoto"] = uye.Foto;

                        return RedirectToAction("Index", "Home");

                    }
                    else
                    {
                        ModelState.AddModelError("Fotoğraf", "Fotoğraf Seçiniz");
                    }
                }
                else
                {
                    ViewBag.HataMesaji = "Bu email adresi daha önce kullanılmış";
                }


            }

            return View(uye);
        }

        public ActionResult Edit(int id)
        {
            var uye = db.Uyes.Where(u => u.UyeId == id).SingleOrDefault();
            if (Convert.ToInt32(Session["uyeid"]) != uye.UyeId)
            {
                return HttpNotFound();
            }

            return View(uye);
        }

        [HttpPost]
        public ActionResult Edit(Uye uye, int id, HttpPostedFileBase Foto)
        {
            if (ModelState.IsValid)
            {
                //var md5pass = Sifre;
                var uyes = db.Uyes.Where(u => u.UyeId == id).SingleOrDefault();
                if (Foto != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(uyes.Foto)))
                    {
                        System.IO.File.Delete(Server.MapPath(uyes.Foto));
                    }
                    WebImage img = new WebImage(Foto.InputStream);
                    FileInfo fotoinfo = new FileInfo(Foto.FileName);

                    string newfoto = Guid.NewGuid().ToString() + fotoinfo.Extension;
                    img.Resize(1000, 1000);
                    img.Save("~/Uploads/UyeFoto/" + newfoto);
                    uyes.Foto = "/Uploads/UyeFoto/" + newfoto;
                }

                uyes.AdSoyad = uye.AdSoyad;
                uyes.KullaniciAdi = uye.KullaniciAdi;
                //uyes.Sifre = Crypto.Hash(md5pass,"MD5");
                uyes.Sifre = uye.Sifre;
                uyes.Email = uye.Email;

                db.SaveChanges();
                Session["kullaniciadi"] = uye.KullaniciAdi;

                return RedirectToAction("Index", "Home", new { id = uyes.UyeId });

            }
            return View();
        }

        public ActionResult UyeProfil(int id)
        {
            var uye = db.Uyes.Where(u => u.UyeId == id).SingleOrDefault();
            return View(uye);
        }

    }
}