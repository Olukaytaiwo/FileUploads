using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FileUploads.Models
{
    public class PaymentUpload
    {
        public string RecieptName { get; set; }
        public IFormFile Reciept { get; set; }
        public string ExcelName { get; set; }
        public IFormFile Excel { get; set; }
    }
}
