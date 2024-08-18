using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using mini_E_commerce_Aldakkaneh.Models;

namespace mini_E_commerce_Aldakkaneh.Controllers
{
    public class CustomersController : Controller
    {
        private AldakkanehEntities db = new AldakkanehEntities();

      

      
        public ActionResult Welcomepage()
        {
            return View();
        }
      
        public ActionResult Profile()
        {
            int customerId = (int)Session["CustomerId"]; 
            var customer = db.Customers.Find(customerId);

            if (customer == null)
            {
                return HttpNotFound();
            }

            return View(customer);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(Customer updatedCustomer)
        {
            if (ModelState.IsValid)
            {
                // الحصول على معرف العميل من الجلسة
                int customerId = (int)Session["CustomerId"];

                // البحث عن العميل في قاعدة البيانات
                var customer = db.Customers.Find(customerId);

                if (customer == null)
                {
                    return HttpNotFound();
                }

                // تحديث البيانات
                customer.first_name = updatedCustomer.first_name;
                customer.last_name = updatedCustomer.last_name;
                customer.email = updatedCustomer.email;
                customer.address = updatedCustomer.address;
                customer.phone_number = updatedCustomer.phone_number;

                // حفظ التغييرات في قاعدة البيانات
                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "تم تحديث الملف الشخصي بنجاح!";
                return RedirectToAction("Profile");
            }

            // إذا كان هناك خطأ في النموذج، إعادة عرض النموذج مع الأخطاء
            return View(updatedCustomer);
        }

        // GET: Customers/Create
        public ActionResult registration()
        {
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
       
        public ActionResult registration([Bind(Include = "customer_id,first_name,last_name,email,password,address,phone_number")] Customer customer)
        {
            // شرط للتحقق من صحة البريد الإلكتروني
            if (!customer.email.Contains("@") || !customer.email.Contains("."))
            {
                TempData["ErrorMessage"] = "البريد الإلكتروني غير صحيح!";
                return RedirectToAction("registration");
            }

            // شرط للتحقق من طول كلمة المرور
            if (customer.password.Length < 8)
            {
                TempData["ErrorMessage"] = "كلمة المرور يجب أن تكون أطول من 8 حروف!";
                return RedirectToAction("registration");
            }

            // شرط للتحقق من رقم الهاتف
            if (!(customer.phone_number.StartsWith("077") || customer.phone_number.StartsWith("079") || customer.phone_number.StartsWith("078"))
                || customer.phone_number.Length != 10)
            {
                TempData["ErrorMessage"] = "رقم الهاتف يجب أن يكون من 10 أرقام ويبدأ بـ 077 أو 079 أو 078!";
                return RedirectToAction("registration");
            }

            if (ModelState.IsValid)
            {
                customer.password = BCrypt.Net.BCrypt.HashPassword(customer.password);
                db.Customers.Add(customer);
                db.SaveChanges();

                TempData["SuccessMessage"] = "تم إنشاء الحساب بنجاح!";
                return RedirectToAction("Login");
            }

            TempData["ErrorMessage"] = "حدث خطأ أثناء التسجيل. الرجاء التحقق من المدخلات!";
            return View(customer);
        }
        public ActionResult Logout()
        {
            // إزالة البيانات المحددة من الجلسة
            Session["CustomerName"] = null;
            Session["CustomerId"] = null;

            // إزالة كافة البيانات من الجلسة إذا رغبت بذلك
            Session.Clear();

            // إلغاء المصادقة
            FormsAuthentication.SignOut();

            // إعادة التوجيه إلى صفحة تسجيل الدخول
            return RedirectToAction("Login", "Customers");
        }



        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email,string password)
        {
            if (string.IsNullOrEmpty(email)|| string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "البريد الالكتروني و كلمه المرور مطلوبان");
                return View();
            }
            var customer=db.Customers.FirstOrDefault(c=>c.email == email);
            if (customer !=null && BCrypt.Net.BCrypt.Verify(password,customer.password))
            {
                FormsAuthentication.SetAuthCookie(email, false);

                // تخزين بيانات المستخدم في الجلسة
                Session["CustomerName"] = customer.first_name; // مثال، تخزين اسم المستخدم
                Session["CustomerId"] = customer.customer_id;
                TempData["SuccessMessage"] = "تم إنشاء الحساب بنجاح!";

                return RedirectToAction("prodect", "Products");
            }
            else
            {
                ModelState.AddModelError("","البريد الالكتروني او كلمه المرور غير صحيحه");
           
            return View();
            
            }
        }



    }
}
