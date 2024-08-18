using mini_E_commerce_Aldakkaneh.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity; // تأكد من تضمين هذا

namespace mini_E_commerce_Aldakkaneh.Controllers
{
    public class HomeController : Controller
    {
        private AldakkanehEntities db = new AldakkanehEntities();

        public ActionResult Index()
        {
            // تعيين رسالة نجاح تسجيل الدخول
            TempData["SuccessMessage"] = "تم تسجيل الدخول بنجاح!";

            // استيراد المنتجات مع تصنيفها
            var products = db.Products.Include(p => p.Category).ToList();

            // تحقق من البيانات وتأكد من عدم كونها فارغة
            if (products == null || !products.Any())
            {
                ViewBag.NoProductsMessage = "لا توجد منتجات لعرضها.";
            }

            return View(products);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
