﻿using System.ComponentModel.DataAnnotations.Schema;

namespace ProniaAB202.Models
{
    public class Slide
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public string Description { get; set; }

        public string Image {  get; set; }

        public int Order { get; set; }
       
    }
}
