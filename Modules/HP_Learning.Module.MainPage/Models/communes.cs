
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace HP_Learning.Module.MainPage.Models
{
    [Table("communes", Schema = "category")]
    public class communes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string area_code { get; set; }
        public string parent_code { get; set; }
        public string name_vn { get; set; }


    }
}
