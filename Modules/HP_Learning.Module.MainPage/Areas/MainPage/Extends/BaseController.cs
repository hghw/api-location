using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Smooth.IoC.UnitOfWork.Interfaces;
using Microsoft.AspNetCore.Authorization;
using VietGIS.Infrastructure.Repositories.Session;
using Microsoft.AspNetCore.Mvc.Filters;
using VietGIS.Infrastructure;
using System.Data;
using System;

namespace HP_Learning.Module.MainPage.Areas.MainPage.Controllers.Extends
{
    [Area(nameof(HP_Learning.Module.MainPage))]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class BaseController : Controller
    {
        protected IDbFactory DbFactory { get; }
        protected IMapper Mapper;
        public BaseController(IDbFactory dbFactory, IMapper mapper)
        {
            DbFactory = dbFactory;
            Mapper = mapper;
        }

        protected ISession OpenSession()
        {
            return DbFactory.Create<IAppSession>();
        }
        
        public override void OnActionExecuted(ActionExecutedContext filterContext)
            {
                ViewBag.CDNUrl = GlobalConfiguration.CDNUrl;
                ViewBag.ImagePath = GlobalConfiguration.ImagePath;
                base.OnActionExecuted(filterContext);
            }
        }
        
}
