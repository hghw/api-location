using System;
using System.Threading.Tasks;
using AutoMapper;
using HP_Learning.Module.MainPage.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Smooth.IoC.UnitOfWork.Interfaces;
using VietGIS.Infrastructure.Enums;
using VietGIS.Infrastructure.Models.DTO.Response;
using Dapper.FastCrud;
using Smooth.IoC.Repository.UnitOfWork.Extensions;
using HP_Learning.Module.MainPage.Areas.MainPage.Controllers.Extends;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using Dapper;
using HP_Learning.Module.MainPage.Models.DTO;

namespace HP_Learning.Modules.MainPage.Areas.MainPage.Controllers
{
    [Route("[area]/[controller]")]
    public class DanhSachBaiThiController : BaseController
    {
        private readonly IMemoryCache _memoryCache;
        private const string HOME_CACHE_KEY = "_home_cache";

        public DanhSachBaiThiController(
            IMapper mapper,
            IDbFactory dbFactory, IMemoryCache memoryCache
        ) : base(dbFactory, mapper)
        {
            _memoryCache = memoryCache;
        }
        [HttpGet("/danhSachBaiThi")]
        public IActionResult Index()
        {
            return View("DanhSachBaiThi");
        }
        [HttpPost("/danhSachBaiThi/list")]
        public async Task<JsonResult> getListAsync(DataTableBaiThiParameters dataTb)
        {
            using (var session = OpenSession())
            {
                try
                {
                    var condition = "(1=1)";
                    if (!string.IsNullOrEmpty(dataTb.search.value))
                    {
                        var search = dataTb.search.value;
                        condition += $" and lower({nameof(BaiDuThi.ho_ten)}) Like '%{search}%' or lower({nameof(BaiDuThi.truong)}) Like '%{search}%' or lower({nameof(BaiDuThi.lop)}) Like '%{search}%'";
                    }
                    if (dataTb.districts > 0)
                    {
                        condition += $" and {Sql.TableAndColumn<BaiDuThi>(nameof(BaiDuThi.district_code))} = '{dataTb.districts}'";
                    }
                    if (dataTb.communes > 0)
                    {
                        condition += $" and {Sql.TableAndColumn<BaiDuThi>(nameof(BaiDuThi.commune_code))} = '{dataTb.communes}'";
                    }
                    var data = session.Find<BaiDuThi>(stm => stm.Where($"{condition}")
                                                       .Include<BaiDuThiDinhKem>(join => join.InnerJoin())
                                                        .OrderBy($"{Sql.Table<BaiDuThi>()}.{nameof(BaiDuThi.id)} desc")
                                                       .Skip(dataTb.start).Top(dataTb.length));
                    foreach (var item in data)
                    {
                        item.dinhkems = (session.Find<BaiDuThiDinhKem>(stm => stm.Where($"{nameof(BaiDuThiDinhKem.bai_duthi_id)} = '{item.id}'")));
                    }
                    return Json(new RestPagedDataTable
                    {
                        data = data,
                        recordsFiltered = await session.CountAsync<BaiDuThi>(stm => stm.Where($"{condition}")),
                        recordsTotal = await session.CountAsync<BaiDuThi>()
                    });

                }
                catch (Exception e)
                {
                    return Json(new RestError(e.Message));
                }
            }
        }
    }
}
