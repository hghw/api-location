
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace HP_Learning.Module.MainPage.Models
{
    [Table("districts", Schema = "category")]
    public class districts
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string area_code { get; set; }
        public string name_vn { get; set; }


    }
}
