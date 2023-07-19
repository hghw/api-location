using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace HP_Learning.Module.MainPage.Models
{
    [Table("bai_duthi", Schema = "public")]
    public class BaiDuThi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string ho_ten { get; set; }
        public string truong { get; set; }
        public string lop { get; set; }
        public string so_dt { get; set; }
        public string email { get; set; }
        public string commune_code { get; set; }
        public string district_code { get; set; }
        public string uid { get; set; }
        [NotMapped]
        public IEnumerable<districts> districts { get; set; }
        [NotMapped]
        public IEnumerable<communes> communes { get; set; }
        [NotMapped]
        public IEnumerable<BaiDuThiDinhKem> dinhkems { get; set; }
    }
}
