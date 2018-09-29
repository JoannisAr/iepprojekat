using IepProject0.Models;
using log4net;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace IepProject0.Controllers
{
    public class UserController : Controller
    {
        private IepTestContext dbcontext = new IepTestContext();
        private static log4net.ILog Log { get; set; }
        ILog log = log4net.LogManager.GetLogger(typeof(UserController));

        // GET: User
        public ActionResult Index()
        {
            return View();
        }
        private String Hash(String password)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            Byte[] originalBytes = System.Text.ASCIIEncoding.Default.GetBytes(password);
            Byte[] encodedBytes = md5.ComputeHash(originalBytes);

            return BitConverter.ToString(encodedBytes);
        }
        public ActionResult Login()
        {      
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            model.Password = Hash(model.Password);
            try
            {
                var user = dbcontext.User.First(x => x.Password.Equals(model.Password) && x.Email.Equals(model.Email));
                Session["User"] = user;
                if (user.Email.Equals("admin@admin.com"))
                    Session["isAdmin"] = true;
                else
                    Session["isAdmin"] = false;
                log.Info("Logged User : " + user.Id);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                log.Info("Failed to log for  User : " +model.Email);
                ViewBag.ErrorMessage = "Incorrect password or email.";
                return View();
            }
        }

        public ActionResult Logout()
        {
            if(((User)Session["User"])!=null){
                try
               {
                    Session.Clear();
                    log.Info("Logged off : " + ((User)Session["User"]).Id);
                }
                catch(Exception e)
            {

              }
            }
            return RedirectToAction("Index", "Home");
        }
        public ActionResult Register()
        {
            if (Session["User"] != null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Exclude = "Id,TokensNumber,Bids,TokenOders")]User user)
        {
            if (ModelState.IsValid)
            {
                if (!dbcontext.User.Any(x => x.Email.Equals(user.Email)))
                {
                    user.TokensNumber = 0;
                    user.Password = Hash(user.Password);
                    dbcontext.User.Add(user);
                    log.Info("Register User : " + user.Email);
                    dbcontext.SaveChanges();
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.ErrorMessage = "This e-mail is already taken.";
                    log.Info("Failed Register User : " + user.Email);
                    return View("Register");
                }
            }
            return View();
        }

        public ActionResult Edit()
        {
            if (Session["User"] != null && Session["isAdmin"].Equals(false))
                return View(Session["User"]);
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FirstName,LastName,Email")] User user)
        {
            if (Session["User"] != null && Session["isAdmin"].Equals(false))
            {
                var mail = user.Email;
                var id = ((User)Session["User"]).Id;
                if (!dbcontext.User.Any(x => x.Email == user.Email && x.Id != id))
                {
                    user.Id = ((User)Session["User"]).Id;
                    user.Password = ((User)Session["User"]).Password;
                    user.TokenOders = ((User)Session["User"]).TokenOders;
                    user.TokensNumber = ((User)Session["User"]).TokensNumber;
                    dbcontext.Entry(user).State = EntityState.Modified;
                    dbcontext.SaveChanges();
                    ((User)Session["User"]).Email = user.Email;
                    ((User)Session["User"]).FirstName = user.FirstName;
                    ((User)Session["User"]).LastName = user.LastName;
                    log.Info("Edited User : " + id + " " + user.Email + " " +user.FirstName + " "+ user.LastName);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                   log.Info("Failed Edit User "+ id);
                   ViewBag.ErrorMessage = "User with this mail exists!";
                   return View(Session["User"]);
                }
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public ActionResult PasswordChange()
        {
            if (Session["User"] != null && Session["isAdmin"].Equals(false))
                return View();
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PasswordChange([Bind(Include = "OldPassword,NewPassword,ConfirmNewPassword")]ChangePassViewModel passdata)
        {
            if (Session["User"] != null && Session["isAdmin"].Equals(false))
            {
                if (((User)Session["User"]).Password.Equals(Hash(passdata.OldPassword)))
                {
                    var user = dbcontext.User.Find(((User)Session["User"]).Id);

                    user.Password = Hash(passdata.NewPassword);
                    ((User)Session["User"]).Password = user.Password;
                    dbcontext.SaveChanges();
                    log.Info("Password Changed User : " + user.Id);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    log.Info("Password Changed User Failed : " + ((User)Session["User"]).Id);
                    ViewBag.ErrorMessage = "The password you have entered is invalid.";
                    return View();
                }
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        //TODOJANISE MORACES DA URADIS NE BAD REQUEST NEGO STRANICA ZA LOGIN.
        public ActionResult TokenOrders(int? page)
        {
            if (Session["User"] != null && Session["isAdmin"].Equals(false))
            {
                //NEKI UPID OVDE.
                var id = ((User)Session["User"]).Id;
                var orders = dbcontext.TokenOders.Where(o => o.User.Id == id).ToList();
                log.Info("Token Orders User : " + id);
                int pageSize = dbcontext.SystemParameter.FirstOrDefault().DefaultNumPageAuctions;
                int pageNumber = (page ?? 1);
                return View(orders.ToPagedList(pageNumber, pageSize));
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        }

        public ActionResult EditSystemParameters()
        {
            if (Session["User"] != null && Session["isAdmin"].Equals(true))
            {
                SystemParameter parameter = dbcontext.SystemParameter.FirstOrDefault();

                return View(parameter);
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSystemParameters([Bind(Include = "DefaultNumPageAuctions,DefaultAuctionTime,SilverPackage,GoldPackage,PlatinumPackage,Currency,PriceOfToken")]SystemParameter parameters)
        {
            if (Session["User"] != null && Session["isAdmin"].Equals(true))
            {
                if (ModelState.IsValid)
                {
                    parameters.Id = 1;
                    dbcontext.Entry(parameters).State = EntityState.Modified;
                    log.Info("Edit System Parameters");
                    dbcontext.SaveChanges();
                }
                return RedirectToAction("Index", "Home");
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Bid(int idAuction, int tokensNum)
        {
            using (var transaction = dbcontext.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    Bids bid = new Bids()
                    {
                        Bidder = ((User)Session["User"]).Id,
                        User = dbcontext.User.Find(((User)Session["User"]).Id),
                        Auction = idAuction,
                        Auction1 = dbcontext.Auction.Find(idAuction),
                        BidOn = DateTime.UtcNow,
                        Amount = tokensNum,
                        Currency = "RSD"
                    };
                    ViewBag.auctionBidId = idAuction;
                    Auction auct = dbcontext.Auction.Find(idAuction);
                    if(((DateTime)auct.OpenedOn).AddSeconds(auct.AuctionTime)<= DateTime.UtcNow)
                        return RedirectToAction("Index", "Auction");
                    Bids previousBid = null;
                    if (auct.Bids.Count != 0)
                    {
                        previousBid = auct.Bids.OrderByDescending(x => x.BidOn).First();
                    }

                    if ((previousBid != null && bid.Amount <= previousBid.Amount) || bid.Amount <= (double)bid.Auction1.StartPrice)
                    {
                        ViewBag.message = "Invalid Bid";
                        log.Info("Bid Fail User" + bid.User.Id);
                        return RedirectToAction("Index", "Auction");
                    }
                    if (previousBid != null)
                    {
                        if (previousBid.User.Id.Equals(((User)Session["User"]).Id))
                        {   //just take the extra.
                            if (bid.Amount - previousBid.Amount < bid.User.TokensNumber)
                                bid.User.TokensNumber -= bid.Amount - previousBid.Amount;
                            else
                            {
                                ViewBag.message = "You don't have anough Tokens!";
                                log.Info("Bid Fail User" + bid.User.Id);
                                return RedirectToAction("Index", "Auction");
                            }
                        }
                        else
                        {
                            if (bid.User.TokensNumber < tokensNum)
                            {
                                ViewBag.message = "You don't have anough Tokens!";
                                log.Info("Bid Fail User" + bid.User.Id);
                                return RedirectToAction("Index", "Auction");
                            }
                            else
                            {
                                previousBid.User.TokensNumber += previousBid.Amount;
                                bid.User.TokensNumber -= tokensNum;
                            }
                        }
                    }
                    else
                    {
                        if (bid.User.TokensNumber < tokensNum)
                        {
                            ViewBag.message = "You don't have anough Tokens!";
                            log.Info("Bid Fail User"+bid.User.Id);
                            return RedirectToAction("Index", "Auction");
                        }
                        else
                            bid.User.TokensNumber -= tokensNum;
                    }
                    if (ModelState.IsValid)
                    {
                        ((User)Session["User"]).TokensNumber = bid.User.TokensNumber;
                        auct.CurrentPrice = tokensNum;
                        auct.CurUser = ((User)Session["User"]).Id;
                        auct.FullName = bid.User.FirstName + " " + bid.User.LastName;
                        dbcontext.Bids.Add(bid);
                        await dbcontext.SaveChangesAsync();
                        transaction.Commit();
                        log.Info("Successful bid  by " + bid.User.Id + " on " + bid.Auction);
                        Hubs.AuctionHub.AuctionUpdate(idAuction, tokensNum, bid.User.FirstName + " " + bid.User.LastName);
                        return RedirectToAction("Index", "Auction");
                    }
                    else
                        return View();
                }
                catch (Exception)
                {
                    log.Info("Roll back bid fail");
                    transaction.Rollback();
                    return RedirectToAction("Index", "Auction");
                }
            }
        }
    }
}