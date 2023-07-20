using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Shop_Manager.Module.MainPage.Models
{
    [Table("location", Schema = "public")]
    public class Location
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public string user_id { get; set; }
    }
}
