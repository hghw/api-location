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

namespace HP_Learning.Modules.MainPage.Areas.MainPage.Controllers
{
    [Route("[area]/[controller]")]
    public class BaiThiController : BaseController
    {
        private readonly IMemoryCache _memoryCache;
        private const string HOME_CACHE_KEY = "_home_cache";

        public BaiThiController(
            IMapper mapper,
            IDbFactory dbFactory, IMemoryCache memoryCache
        ) : base(dbFactory, mapper)
        {
            _memoryCache = memoryCache;
        }
        [HttpGet("/bai-thi")]
        public IActionResult Index()
        {
            return View("BaiThi");
        }
        [HttpGet("/review/{id}")]
        public IActionResult XemBaiThiIndex(string id)
        {
            using (var session = OpenSession())
            {
                try
                {
                    var sql = $"SELECT * FROM {Sql.Table<BaiDuThi>()} inner join {Sql.Table<BaiDuThiDinhKem>()} on " +
                        $"{Sql.TableAndColumn<BaiDuThi>(nameof(BaiDuThi.id))} = {Sql.TableAndColumn<BaiDuThiDinhKem>(nameof(BaiDuThiDinhKem.bai_duthi_id))}" +
                        $" where {Sql.TableAndColumn<BaiDuThi>(nameof(BaiDuThi.uid))} = '{id}';";
                    var data = session.Query<BaiThi>(sql);
                    foreach (var item in data)
                    {
                        item.communes = (session.Find<communes>(stm => stm.Where($"{nameof(communes.area_code)} = '{item.commune_code}'")));
                        item.districts = (session.Find<districts>(stm => stm.Where($"{nameof(districts.area_code)} = '{item.district_code}'")));
                    }
                    return View("ReView",data);

                }
                catch (Exception e)
                {
                    return Json(new RestError(e.Message));
                }
            }
        }
        [HttpGet("/the-le")]
        public IActionResult TheLe()
        {
            return View("TheLe");
        }
        [HttpPost("/upload")]
        [ValidateAntiForgeryToken]
        public async Task<RestBase> CreateOrUpdateAsync(BaiDuThi bdt, BaiDuThiDinhKem dk)
        {
            using (var session = OpenSession())
            {
                using (var uow = session.UnitOfWork())
                {
                    try
                    {
                        string uid = Guid.NewGuid().ToString();
                        bdt.uid = uid;
                        await uow.InsertAsync(bdt);
                        //insert File đính kèm
                        dk.bai_duthi_id = bdt.id;
                        await uow.InsertAsync(dk);
                        return new RestData()
                        {
                            data = bdt,
                            status = EnumErrorCode.OK
                        };
                    }
                    catch (Exception e)
                    {
                        return new RestError(e.Message);

                    }
                }

            }
        }
        [HttpGet("/communes")]
        public async Task<JsonResult> Communes()
        {
            using (var session = OpenSession())
            {
                try
                {
                    var data = await session.FindAsync<communes>();
                    return Json(new RestData
                    {
                        data = data
                    });
                }
                catch (Exception e)
                {
                    return Json(new RestError(e.Message));
                }
            }
        }
        [HttpGet("/districts")]
        public async Task<JsonResult> Districts()
        {
            using (var session = OpenSession())
            {
                try
                {
                    var data = await session.FindAsync<districts>();

                    return Json(new RestData
                    {
                        data = data
                    });
                }
                catch (Exception e)
                {
                    return Json(new RestError(e.Message));
                }
            }
        }
        [HttpGet("/getItemCommune")]
        public async Task<JsonResult> GetItemCommune(string id)
        {
            using (var session = OpenSession())
            {
                try
                {
                    var data = await session.FindAsync<communes>(stm => stm.Where($"{Sql.Table<communes>()}.{nameof(communes.parent_code)}= '{id}'"));

                    return Json(new RestData
                    {
                        data = data
                    });
                }
                catch (Exception e)
                {
                    return Json(new RestError(e.Message));
                }
            }
        }
        [HttpGet("/getExam")]
        public async Task<JsonResult> getExam(int id)
        {
            using (var session = OpenSession())
            {
                try
                {
                    var sql = $"SELECT * FROM {Sql.Table<BaiDuThi>()} inner join {Sql.Table<BaiDuThiDinhKem>()} on " +
                        $"{Sql.TableAndColumn<BaiDuThi>(nameof(BaiDuThi.id))} = {Sql.TableAndColumn<BaiDuThi>(nameof(BaiDuThiDinhKem.bai_duthi_id))}" +
                        $" where {Sql.TableAndColumn<BaiDuThi>(nameof(BaiDuThi.id))} = {id};";
                    var data = await session.QueryAsync<BaiThi>(sql);
                    return Json(new RestData
                    {
                        data = data
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
