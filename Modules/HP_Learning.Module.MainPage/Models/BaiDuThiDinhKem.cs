
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace HP_Learning.Module.MainPage.Models
{
    [Table("bai_duthi_dinhkem", Schema = "public")]
    public class BaiDuThiDinhKem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [ForeignKey(nameof(baithis))]
        public int bai_duthi_id { get; set; }
        public string ten_dinhkem { get; set; }
        public string duong_dan { get; set; }

        [NotMapped]
        public BaiDuThi baithis { get; set; }
    }
}
