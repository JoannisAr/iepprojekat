using IepProject0.Models;
using log4net;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace IepProject0.Controllers
{
    public class AuctionController : Controller
    {
        private IepTestContext dbcontext = new IepTestContext();
        ILog log = log4net.LogManager.GetLogger(typeof(AuctionController));

        public ActionResult Index([Bind(Include = "Name, LowPrice, HighPrice,List,Page")] AuctionViewModelForm auctionViewModel)
        {
            ViewBag.CurrentFilter = auctionViewModel;
            var auctions = dbcontext.Auction.OrderByDescending(x => x.CreatedOn).ToList();

            if (auctionViewModel.Name != null)
                auctions = auctions.Where(x => x.Name.Contains(auctionViewModel.Name)).ToList();

            if (auctionViewModel.LowPrice != null)
                auctions = auctions.Where(s => s.CurrentPrice != null && s.StartPrice >= auctionViewModel.LowPrice).ToList();

            if (auctionViewModel.HighPrice != null)
                auctions = auctions.Where(s => s.CurrentPrice != null && s.StartPrice <= auctionViewModel.HighPrice).ToList();

            if (!String.IsNullOrEmpty(auctionViewModel.List))
                auctions = auctions.Where(s => s.Status == auctionViewModel.List).ToList();
          
            int pageSize = dbcontext.SystemParameter.FirstOrDefault().DefaultNumPageAuctions;
            int pageNumber = (auctionViewModel.Page ?? 1);
            auctionViewModel.Auctions = auctions.ToPagedList(pageNumber, pageSize);
            log.Info("Search Name : " + auctionViewModel.Name+" Status : "+ auctionViewModel.List);
            return View(auctionViewModel);
        }

        public ActionResult Create()
        {
            if (Session["User"] != null && Session["isAdmin"].Equals(false))
                return View();
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,AuctionTime,StartPrice,IMG")] AuctionCreateViewModelForm auct)
        {

            if (Session["User"] != null && Session["isAdmin"].Equals(false))
            {
                var parameters = dbcontext.SystemParameter.First();
                Auction auction = new Auction();
                if (ModelState.IsValid)
                {
                    if (auct.AuctionTime > 0)
                        auction.AuctionTime = auct.AuctionTime;
                    else
                        auction.AuctionTime = parameters.DefaultAuctionTime;
                    auction.CreatedOn = DateTime.UtcNow;
                    auction.Status = "READY";
                    auction.Name = auct.Name;
                    auction.StartPrice = auct.StartPrice;
                    auction.CurrentPrice = auct.StartPrice;
                    auction.FirstUser = ((User)Session["User"]).Id;
                    auction.Currency = parameters.Currency;
                    if (auct.IMG != null)
                    {
                        auction.upIMG = auct.IMG;
                        auction.IMG = new byte[auct.IMG.ContentLength];
                        auction.upIMG.InputStream.Read(auction.IMG, 0, auction.IMG.Length);
                    }
                    dbcontext.Auction.Add(auction);
                    dbcontext.SaveChanges();
                }
                log.Info("Created Auction Name: " + auct.Name + " Price : " + auct.StartPrice + " Time: " + auction.AuctionTime);
                return RedirectToAction("Index", "Auction");
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public ActionResult AuctionsToOpen(int? page)
        {
           if (Session["User"]!=null  && Session["isAdmin"].Equals(true))
           {
               var auctions = dbcontext.Auction.OrderByDescending(x => x.CreatedOn).Where(x => x.Status.Equals("READY")).ToList();
               int pageSize = dbcontext.SystemParameter.FirstOrDefault().DefaultNumPageAuctions;
               int pageNumber = (page ?? 1);   
               return View(auctions.ToPagedList(pageNumber, pageSize));
           }
           else
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        public ActionResult OpenAuctions(int id, int? returnpage)
        {
            if (Session["User"] != null && Session["isAdmin"].Equals(true))
            {
                Auction auction = dbcontext.Auction.Find(id);
                if (auction.Status == "READY")
                {
                    auction.Status = "OPENED";
                    auction.OpenedOn = DateTime.UtcNow;
                    dbcontext.Entry(auction).State = EntityState.Modified;
                    log.Info("Opened Auction Name : " + auction.Name);
                   dbcontext.SaveChanges();
                }
                return RedirectToAction("AuctionsToOpen", new { page = returnpage });
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        }
        public ActionResult AuctionClose(int id)
        {
            Auction auction = dbcontext.Auction.Find(id);
            if(auction!=null)
                AuctionClose(auction);
            return RedirectToAction("Index", "Auction");
         }
        private bool AuctionClose(Auction auction)
        {
            DateTime time = ((DateTime)auction.OpenedOn).AddSeconds(auction.AuctionTime);
            if (time <= DateTime.UtcNow)
            {
                auction.Status = "COMPLETED";
                auction.CompletedOn = DateTime.UtcNow;
                dbcontext.SaveChanges();
                log.Info("Closed Auction Name : " + auction.Name);
                return true;
            }
            return false;
        }
        public ActionResult Details(int? id)
        {
            if(id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Auction auction = dbcontext.Auction.Find(id);
            if (auction == null)
                ViewBag.ErrorMessage = "Auction counld not be found";
            User user = dbcontext.User.Find(auction.FirstUser);
            ViewBag.CreatedBy = user.FirstName + " " + user.LastName;
            log.Info("Details Auction Name : " + auction.Name);
            return View(auction);
        }

    }
}