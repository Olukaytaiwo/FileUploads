using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PartnersPlatform.Data.Entities
{    
    // SAVE PROPOSAL DETAILS
    public class NseProposal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PolicyId { get; set; }
        public string CompanyName { get; set; }
        public string CustomerNumber { get; set; }
        public string ProductName { get; set; }
        public string SumAssured { get; set; }
        public string LastPremiumPaid { get; set; }
        public DateTime CoverStartDate { get; set; }
        public DateTime CoverEndDate { get; set; }
        public string FullName { get; set; }    
        public string UserName { get; set; }        
        public string UserId { get; set; }
        public string EmailAddress { get; set; }
        public double AmountPayable { get; set; }
        public string TelephoneNumber { get; set; }
        public string RcNumber { get; set; }
        public string QuoteId { get; set; }
        public string PricingOption { get; set; }
        public string Tier { get; set; }
        public string DateUploaded { get; set; }
        public byte[] AxaLogo { get; set; }
    }

}
