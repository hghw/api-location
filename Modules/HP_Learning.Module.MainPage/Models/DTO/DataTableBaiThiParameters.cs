using System;
using VietGIS.Infrastructure.Models.DTO.Request;

namespace HP_Learning.Module.MainPage.Models.DTO
{
    public class DataTableBaiThiParameters : DataTableParameters
    {
        public int districts { get; set; }
        public int communes { get; set; }

    }

}