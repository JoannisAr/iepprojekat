using IepProject0.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Net.Mail;
using log4net;

namespace IepProject0.Controllers
{
    public class OrderController : Controller
    {
        private static log4net.ILog Log { get; set; }
        ILog log = log4net.LogManager.GetLogger(typeof(OrderController));

        private IepTestContext dbcontext = new IepTestContext();
        public ActionResult OrderTokens()
        {
            return View();
        }
        public ActionResult CreateOrder(string package)
        {
            int tokenAmount = 0;
            SystemParameter parameter = dbcontext.SystemParameter.FirstOrDefault();
            if (package == "Silver")
                tokenAmount = parameter.SilverPackage;
            else if (package == "Gold")
                tokenAmount = parameter.GoldPackage;
            else if (package == "Platinum")
                tokenAmount = parameter.PlatinumPackage;
            int price = (int)(tokenAmount * parameter.PriceOfToken);
            TokenOder tokenOrder = new TokenOder();
            if (ModelState.IsValid)
            {
                tokenOrder.Buyer = ((User)Session["User"]).Id;
                tokenOrder.Currency = parameter.Currency;
                tokenOrder.Price = price;
                tokenOrder.User = dbcontext.User.Find(tokenOrder.Buyer);
                tokenOrder.TokensAmount = tokenAmount;
                tokenOrder.Status = "SUBMITTED";
                dbcontext.TokenOders.Add(tokenOrder);
                dbcontext.SaveChanges();
            }
            log.Info("Create Order-SUBMITTED User" + tokenOrder.Buyer);
            return Redirect("http://stage.centili.com/payment/widget?apikey=3e298f9e6f38648e1fbd052ea42dae99&country=rs&reference=" + tokenOrder.Id);
        }
        public ActionResult Centili(string clientid, string status)
        {
            TokenOder order = dbcontext.TokenOders.Find(Int32.Parse(clientid));
            if (order == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (status == "canceled" || status == "failed")
            {
                order.Status = "CANCELED";
                log.Info("Create Order-CANCELED User" + order.Buyer);
            }
            else
            {
                order.Status = "COMPLETED";
                using (var transaction = dbcontext.Database.BeginTransaction())
                {
                    try
                    {
                        User user = dbcontext.User.Find(order.Buyer);
                        if (user == null)
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                        user.TokensNumber += order.TokensAmount;
                        //((User)Session["User"]).TokensNumber = user.TokensNumber;
                        dbcontext.Entry(user).State = EntityState.Modified;
                        dbcontext.SaveChanges();
                        transaction.Commit();
                        sendMail(user.Email);
                        log.Info("Token order succeeded " + order.User.Email + "num " + order.TokensAmount);
                    }
                    catch (Exception)
                    {
                        log.Info("Roll back token order failed " + order.User.Email + "num " + order.TokensAmount);
                        transaction.Rollback();
                        return RedirectToAction("Index", "Auction");
                    }
                }
            }
            dbcontext.Entry(order).State = EntityState.Modified;
            dbcontext.SaveChanges();
            return new HttpStatusCodeResult(200);
        }

        private void sendMail(string email)
        {
            MailMessage mm = new MailMessage("projekatiep0061@gmail.com", email);
            {
                mm.Subject = "Payment Successful!";
                string body = "Thank you for your purchase!xx";
                mm.Body = body;
                mm.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                NetworkCredential NetworkCred = new NetworkCredential("projekatiep0061", "projekatiep1!");
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = 587;
                smtp.Send(mm);
            }
        }
    }
}