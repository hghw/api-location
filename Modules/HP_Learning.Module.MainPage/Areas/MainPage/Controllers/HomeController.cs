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

namespace HP_Learning.Modules.MainPage.Areas.MainPage.Controllers
{
    [Route("[area]/[controller]")]
    public class HomeController : BaseController
    {
        private readonly IMemoryCache _memoryCache;
        private const string HOME_CACHE_KEY = "_home_cache";

        public HomeController(
            IMapper mapper,
            IDbFactory dbFactory, IMemoryCache memoryCache
        ) : base(dbFactory, mapper)
        {
            _memoryCache = memoryCache;
        }
        [HttpGet("/")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
