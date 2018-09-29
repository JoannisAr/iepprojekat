using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IepProject0.Models
{

    public class AuctionViewModelForm
    {
        public IPagedList<Auction> Auctions { get; set; }

        [Display(Name = "Name or partial name")]
        public string Name { get; set; }

        [Display(Name = "Status")]
        public string List { get; set; }

        [Display(Name = "Low Price")]
        public decimal? LowPrice { get; set; }

        [Display(Name = "High price")]
        public decimal? HighPrice { get; set; }

        public int? Page { get; set; }
        public IEnumerable<SelectListItem> thisStatus { get; set; }
    }

    public class AuctionCreateViewModelForm
    {

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public int AuctionTime { get; set; }

        public decimal StartPrice { get; set; }

        [Required]
        public HttpPostedFileBase IMG { get; set; }
    }

}