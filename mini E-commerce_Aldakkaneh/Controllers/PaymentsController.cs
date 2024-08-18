using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using mini_E_commerce_Aldakkaneh.Models;

namespace mini_E_commerce_Aldakkaneh.Controllers
{
    public class PaymentsController : Controller
    {
        private AldakkanehEntities db = new AldakkanehEntities();
        private readonly string PayPalBaseUrl = "https://api.sandbox.paypal.com/";
        private readonly string ClientId = "AZdkr6v_1FZG68xLjacHS8bQeVRiMqCBFcaSEhyp_W8mYwjwG3hUkNkdpHoUaXiGd4VhtLdMCIW4zG_C";
        private readonly string Secret = "EOSNx_ek2CYg6N2WqM4i81m2bXAyZ72hdY-pcX23yepPpHbarU8eDLiKpBVkpZ4pFwPMOiZ2WX7FwTj3";

        public async Task<ActionResult> ProcessOrder(decimal? totalSum)
        {
            if (!totalSum.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid totalSum value");
            }

            ViewBag.TotalSum = totalSum.Value;

            try
            {
                var accessToken = await GetAccessToken();

                var paymentPayload = new
                {
                    intent = "sale",
                    payer = new
                    {
                        payment_method = "paypal"
                    },
                    transactions = new[]
                    {
                        new
                        {
                            amount = new
                            {
                                total = totalSum.Value.ToString("F2"), // Ensure format as a decimal with two decimal places
                                currency = "USD"
                            },
                            description = "Payment description"
                        }
                    },
                    redirect_urls = new
                    {
                        return_url = Url.Action("PaymentReturn", "Payments", null, Request.Url.Scheme),
                        cancel_url = Url.Action("PaymentCancel", "Payments", null, Request.Url.Scheme)
                    }
                };

                var paymentJson = Newtonsoft.Json.JsonConvert.SerializeObject(paymentPayload);

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(PayPalBaseUrl);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(paymentJson, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("/v1/payments/payment", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);

                        string approvalUrl = null;
                        var linksArray = responseObject.links as Newtonsoft.Json.Linq.JArray;
                        if (linksArray != null)
                        {
                            var approvalLink = linksArray.FirstOrDefault(l => (string)l["rel"] == "approval_url");
                            if (approvalLink != null)
                            {
                                approvalUrl = approvalLink["href"].ToString();
                            }
                        }

                        if (!string.IsNullOrEmpty(approvalUrl))
                        {
                            // Save payment information to the database
                            var payment = new Payment
                            {
                                payment_date = DateTime.Now,
                                payment_method = "paypal",
                                amount = totalSum.Value,
                                customer_id= (int)Session["CustomerId"]
                            };
                            db.Payments.Add(payment);
                            await db.SaveChangesAsync();

                            return Redirect(approvalUrl);
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Approval URL not found in PayPal response.";
                            return View("Error");
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Failed to initiate PayPal payment: " + response.ReasonPhrase;
                        return View("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred: " + ex.Message;
                return View("Error");
            }
        }

        private async Task<string> GetAccessToken()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(PayPalBaseUrl);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{ClientId}:{Secret}")));
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var requestData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                };

                var requestContent = new FormUrlEncodedContent(requestData);
                var response = await client.PostAsync("/v1/oauth2/token", requestContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return responseObject.access_token;
                }
                else
                {
                    throw new Exception("Failed to retrieve PayPal access token: " + response.ReasonPhrase);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // أكشنات أخرى في الـ Controller...
    }
}
