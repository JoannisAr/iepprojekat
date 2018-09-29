using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IepProject0.Controllers;
using Microsoft.AspNet.SignalR;

namespace IepProject0.Hubs
{
    public class AuctionHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }
        public static void AuctionUpdate(int idAuction, int tokensNum, string fullName)
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<AuctionHub>();
            hub.Clients.All.AuctionUpdate(idAuction, tokensNum, fullName);
        }
        public void CloseAuction(int idAuction)
        {
            AuctionController auctionController = new AuctionController();
            auctionController.AuctionClose(idAuction);
          //  CloseAuctionNotify(idAuction);
        }
      /*  public static void CloseAuctionNotify(int idAuction)
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<AuctionHub>();
            hub.Clients.All.AuctionUpdate(idAuction);
        }*/
    }
}