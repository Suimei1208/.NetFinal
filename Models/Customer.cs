﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace NetTechnology_Final.Models
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

		public string Phone { get; set; }
        public string Name { get; set; }

        public string Address { get; set; }
        public DateTime CreateDate { get; set; }      
    } 
}

