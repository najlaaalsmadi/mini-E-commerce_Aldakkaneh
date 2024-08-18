using mini_E_commerce_Aldakkaneh.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using MailKit.Net.Smtp;
using MimeKit;

namespace mini_E_commerce_Aldakkaneh.Controllers
{
    public class HomeController : Controller
    {
        private AldakkanehEntities db = new AldakkanehEntities();

        public ActionResult Index()
        {
            TempData["SuccessMessage"] = "تم تسجيل الدخول بنجاح!";

            var products = db.Products.Include(p => p.Category).ToList();

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
            return View();

        }
        [HttpPost]
        public ActionResult Contact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                string fromEmail = "election2024jordan@gmail.com";
                string fromName = "دعم دكانة عجلون";
                string subjectText = contact.Subject;
                string messageText = $@"
                <html>
                <body dir='rtl'>
                    <h2>مرحباَ {contact.Name}!</h2>
                    <p>شكراً لتواصلك مع محل الدكانة:</p>
                    <p>{contact.Message}</p>
                    <p>إذا كانت لديك أي أسئلة أو تحتاج إلى مساعدة إضافية، لا تتردد في الاتصال بفريق الدعم لدينا.</p>
                    <p>مع أطيب التحيات,<br>فريق الدعم</p>
                </body>
                </html>";

                string toEmail = contact.Email;
                string smtpServer = "smtp.gmail.com";
                int smtpPort = 465; // Port 465 for SSL

                string smtpUsername = "election2024jordan@gmail.com";
                string smtpPassword = "zwht jwiz ivfr viyt"; // تأكد من استخدام كلمة مرور صحيحة

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subjectText;
                message.Body = new TextPart("html") { Text = messageText };

                using (var client = new SmtpClient())
                {
                    client.Connect(smtpServer, smtpPort, true); // Use SSL
                    client.Authenticate(smtpUsername, smtpPassword);
                    client.Send(message);
                    client.Disconnect(true);
                }

                TempData["SuccessMessage"] = "تم إرسال الرسالة بنجاح!";
            }
            else
            {
                TempData["ErrorMessage"] = "يرجى ملء جميع الحقول المطلوبة.";
            }

            return RedirectToAction("Index");
        }
    }
}
