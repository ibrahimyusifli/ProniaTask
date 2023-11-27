﻿using ProniaAB202.Models;
using System.ComponentModel.DataAnnotations;

namespace ProniaAB202.Areas.ProniaAdmin.ViewModels
{
    public class CreateProductVM
    {
       
        public string Name { get; set; }

        public decimal Price { get; set; }
     
        public string Description { get; set; }

        public string SKU { get; set; }
        [Required]
        public int? CategoryId { get; set; }
           
    }
}
