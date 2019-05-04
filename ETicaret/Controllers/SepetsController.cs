using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ETicaret.Models;
using Microsoft.AspNet.Identity;

namespace ETicaret.Controllers
{
    [Authorize]//Sadece giriş yapmış kullanıcıların sepete girebilmesi için
    public class SepetsController : Controller
    {
        private ETicaretEntities db = new ETicaretEntities();




        public ActionResult SepeteEkle(int? adet,int id)
        {
            string userID = User.Identity.GetUserId();// O anda hangi kullanıcının getirdiği bilgiisni getirir

            Sepet sepettekiUrun = db.Sepet.FirstOrDefault(a => a.RefUrunID == id && a.RefAspNetUserID == userID);

            Urunler urun = db.Urunler.Find(id);

            if(sepettekiUrun==null)//sepette ürün yoksa
            {
                Sepet YeniUrun = new Sepet()
                {
                    RefAspNetUserID = userID,
                    RefUrunID = id,
                    Adet = adet ?? 1,
                    ToplamTutar = (adet ?? 1) * urun.UrunFiyati
                };
                db.Sepet.Add(YeniUrun);
            }
            else
            {
                sepettekiUrun.Adet = sepettekiUrun.Adet + (adet ?? 1);
                sepettekiUrun.ToplamTutar = sepettekiUrun.Adet * sepettekiUrun.Urunler.UrunFiyati;
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult SepetGuncelle(int? adet,int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sepet sepet = db.Sepet.Find(id);
            if (sepet == null)
            {
                return HttpNotFound();
            }

            Urunler urun = db.Urunler.Find(sepet.RefUrunID);

            sepet.Adet = adet ?? 1;
            sepet.ToplamTutar = sepet.Adet * urun.UrunFiyati;
            db.SaveChanges();

            return RedirectToAction("Index");

        }

        public ActionResult Sil(int id)
        {
            Sepet sepet = db.Sepet.Find(id);
            db.Sepet.Remove(sepet);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        // GET: Sepets
        public ActionResult Index()
        {
            var sepet = db.Sepet.Include(s => s.Urunler).Include(s => s.AspNetUsers);
            return View(sepet.ToList());
        }
    



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
