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
    public class SiparisController : Controller
    {
        private ETicaretEntities db = new ETicaretEntities();

        // GET: Siparis
        public ActionResult Index()
        {
            var siparis = db.Siparis.ToList();
            return View(siparis.ToList());
        }

       public ActionResult SiparisDetay(int siparisID)
        {
            var SiparisDetay = db.SiparisKalem.Where(a => a.RefSiparisID == siparisID).ToList();
            return View(SiparisDetay.ToList());
        }
        public ActionResult SiparisTamamla()
        {
            string userID = User.Identity.GetUserId();

            IEnumerable<Sepet> sepetUrunleri = db.Sepet.Where(a => a.RefAspNetUserID == userID).ToList();

            string Clientid = "100300000"; // bankadan alınan mağaza kodu
            string Amount = sepetUrunleri.Sum(a => a.ToplamTutar).ToString();//sepetteki ürünlerin toplamı
            string Oid = string.Format("{0:yyyyMMddHHmmss}", DateTime.Now);//sipariş id oluşturuluyor
            string OnayUrl = "http://localhost:11111/Siparis/Tamamlandi";//Ödeme tamamlandığında verilerin geleceği adres
            string HataUrl = "http://localhost:11111/Siparis/Hatali";
            string RDN = "SDADA";//HSH karşılaştırması için eklnen rastgele dize
            string StoreKey = "123456"; // bankanın sanal pos sayfasından alınıyor

            string TransActionType = "Auth";//sabit değişmiyor
            string Isntalment = "";
            string HshStr = Clientid + Oid + Amount + OnayUrl + HataUrl + TransActionType + Isntalment + RDN + StoreKey;

            System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] HashBytes = System.Text.Encoding.GetEncoding("ISO-8859-9").GetBytes(HshStr);
            byte[] InputBytes = sha.ComputeHash(HashBytes);
            string Hash = Convert.ToBase64String(InputBytes);

            ViewBag.ClientId = Clientid;
            ViewBag.Oid = Oid;
            ViewBag.okURL = OnayUrl;
            ViewBag.failURL = HataUrl;
            ViewBag.TransActionType = TransActionType;
            ViewBag.RDN = RDN;
            ViewBag.Hash = Hash;
            ViewBag.Amount = Amount;
            ViewBag.StoreType = "3d_pay_hosting";//ödeme modelimiz biz buna göre anlatıyoruz
            ViewBag.Description = "";
            ViewBag.XID = "";
            ViewBag.Lang = "tr";
            ViewBag.EMail = "destek@destek.com";
            ViewBag.UserID = "mdurmaz";//bu id yi bankanın sanal pos ekranında biz oluşturuyoruz
            ViewBag.PostURL = "https://entegrasyon.asseco-see.com.tr/fim/est3Dgate";

            return View();



        }
        public ActionResult Tamamlandi()
        {
            string userID = User.Identity.GetUserId();

            Siparis siparis = new Siparis();

            siparis.Ad = Request.Form.Get("Ad");
            siparis.Soyad = Request.Form.Get("Soyad");
            siparis.Adres = Request.Form.Get("Adres");
            siparis.Tarih = DateTime.Now;
            siparis.TcKimlikNo = Request.Form.Get("TcKimlikNo");
            siparis.Eposta = Request.Form.Get("Eposta");
            siparis.RefAspNetUserID = userID;

            IEnumerable<Sepet> sepettekiUrunler = db.Sepet.Where(a => a.RefAspNetUserID == userID).ToList();

            foreach(Sepet sepetUrunu in sepettekiUrunler)
            {
                SiparisKalem yeniKalem = new SiparisKalem()
                {
                    Adet = sepetUrunu.Adet,
                    ToplamTutar = sepetUrunu.ToplamTutar,
                    RefUrunID = sepetUrunu.RefUrunID
                };
                siparis.SiparisKalem.Add(yeniKalem);
                db.Sepet.Remove(sepetUrunu);
            }
            db.Siparis.Add(siparis);
            db.SaveChanges();
            return View();
        }
        public ActionResult Hatali()
        {
            ViewBag.Hata = Request.Form;
            return View();
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
