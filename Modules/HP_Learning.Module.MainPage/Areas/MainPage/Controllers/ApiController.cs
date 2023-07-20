using System;
using System.Threading.Tasks;
using AutoMapper;
using Shop_Manager.Module.MainPage.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Smooth.IoC.UnitOfWork.Interfaces;
using VietGIS.Infrastructure.Enums;
using VietGIS.Infrastructure.Models.DTO.Response;
using Dapper.FastCrud;
using Smooth.IoC.Repository.UnitOfWork.Extensions;
using Shop_Manager.Module.MainPage.Areas.MainPage.Controllers.Extends;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using Dapper;
using HP_Learning.Module.MainPage.Areas.MainPage.Controllers.Extends;

namespace HP_Learning.Modules.MainPage.Areas.MainPage.Controllers
{
    [Route("[area]/[controller]")]
    public class ApiController : BaseController
    {
        private readonly IMemoryCache _memoryCache;
        private const string HOME_CACHE_KEY = "_home_cache";

        public ApiController(
            IMapper mapper,
            IDbFactory dbFactory, IMemoryCache memoryCache
        ) : base(dbFactory, mapper)
        {
            _memoryCache = memoryCache;
        }
        [HttpPost("/uploadLocation")]
        [ValidateAntiForgeryToken]
        public async Task<RestBase> CreateOrUpdateAsync(Location item)
        {
            using (var session = OpenSession())
            {
                using (var uow = session.UnitOfWork())
                {
                    try
                    {
                        await uow.InsertAsync(item);
                        return new RestBase(EnumErrorCode.OK);
                    }
                    catch (Exception e)
                    {
                        return new RestError(e.Message);
                    }
                }

            }
        }
    }
}
