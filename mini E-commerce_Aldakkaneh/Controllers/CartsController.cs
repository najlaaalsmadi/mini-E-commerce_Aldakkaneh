using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using mini_E_commerce_Aldakkaneh.Models;


namespace mini_E_commerce_Aldakkaneh.Controllers
{
    public class CartsController : Controller
    {
        private readonly string PayPalBaseUrl = "https://api.sandbox.paypal.com/";
        private readonly string ClientId = "AZdkr6v_1FZG68xLjacHS8bQeVRiMqCBFcaSEhyp_W8mYwjwG3hUkNkdpHoUaXiGd4VhtLdMCIW4zG_C";
        private readonly string Secret = "EOSNx_ek2CYg6N2WqM4i81m2bXAyZ72hdY-pcX23yepPpHbarU8eDLiKpBVkpZ4pFwPMOiZ2WX7FwTj3";

        private AldakkanehEntities db = new AldakkanehEntities();

        // GET: Carts
        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity)
        {
            var userId = User.Identity.GetUserId(); // Adjust according to your authentication method
            int customerId = (int)Session["CustomerId"];

            using (var db = new AldakkanehEntities())
            {
                // Check if the product exists
                var product = db.Products.Find(productId);
                if (product == null)
                {
                    return HttpNotFound();
                }

                // Create new cart item
                var cartItem = new Cart
                {
                    description = product.description,
                    imgprodect = product.imgprodect,
                    quantity = quantity,
                    customer_id = customerId,
                    product_id = productId
                };

                // Add the item to the cart
                db.Carts.Add(cartItem);

                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    // Log the error
                    Console.WriteLine(ex.Message);
                    // Handle the error (e.g., show a user-friendly message)
                }
            }

            return RedirectToAction("Index");
        }

     
        // In your CartController
        public ActionResult Checkout(decimal totalSum)
        {
            Session["TotalSum"] = totalSum;

            return View();
        }

        // GET: Carts
        public ActionResult Index()
        {
            // Get the customer ID from the session
            int customerId = (int)Session["CustomerId"];

            // Fetch cart items for the current customer
            var cartItems = db.Carts
                .Include(c => c.Customer)
                .Include(c => c.Product)
                .Where(c => c.customer_id == customerId)
                .ToList();

            return View(cartItems);
        }



        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cart cart = db.Carts.Find(id);
            if (cart == null)
            {
                return HttpNotFound();
            }
            ViewBag.customer_id = new SelectList(db.Customers, "customer_id", "first_name", cart.customer_id);
            ViewBag.product_id = new SelectList(db.Products, "product_id", "SKU", cart.product_id);
            return View(cart);
        }

        // POST: Carts1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "cart_id,quantity,customer_id,description,imgprodect,product_id")] Cart cart)
        {
            var cartnew  = db.Carts.Where(x=> x.cart_id == cart.cart_id).FirstOrDefault();

            if (ModelState.IsValid)
            {
                db.Entry(cartnew).State = EntityState.Detached; // remove effect of breious obj 

                cart.customer_id = cartnew.customer_id;
                cart.product_id = cartnew.product_id;
                cart.imgprodect = cartnew.imgprodect;
                db.Entry(cart).State = EntityState.Modified; // modified on the card obj from bind 
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.customer_id = new SelectList(db.Customers, "customer_id", "first_name", cart.customer_id);
            ViewBag.product_id = new SelectList(db.Products, "product_id", "SKU", cart.product_id);
            return View(cart);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cart cart = db.Carts.Find(id);
            if (cart == null)
            {
                return HttpNotFound();
            }
            return View(cart);
        }

        // POST: Carts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Cart cart = db.Carts.Find(id);
            db.Carts.Remove(cart);
            db.SaveChanges();
            return RedirectToAction("Index");
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
