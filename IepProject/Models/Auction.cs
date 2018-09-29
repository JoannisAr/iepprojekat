namespace IepProject0.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web;
    using System.Data.Entity;

    [Table("Auction")]
    public partial class Auction
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Auction()
        {
            Bids = new HashSet<Bids>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Display(Name = "Auction Time")]
        public int AuctionTime { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Opened On")]
        public DateTime? OpenedOn { get; set; }

        [Display(Name = "Completed On")]
        public DateTime? CompletedOn { get; set; }

        [Display(Name = "Start Price")]
        public decimal StartPrice { get; set; }

        [Display(Name = "Current Price")]
        public decimal? CurrentPrice { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; }

        [StringLength(20)]

        public string Status { get; set; }

        [Required]
        public byte[] IMG { get; set; }

        public int? CurUser { get; set; }

        [StringLength(255)]
        [Display(Name = "Current Bider")]
        public string FullName { get; set; }

        [Display(Name = "Created by")]
        public int? FirstUser { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Bids> Bids { get; set; }

        [NotMapped]
        public HttpPostedFileBase upIMG { get; set; }
    }
}
