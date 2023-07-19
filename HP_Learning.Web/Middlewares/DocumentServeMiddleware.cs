using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HP_Learning.Web.Middlewares
{
    public class DocumentServeMiddleware
    {
        private IWebHostEnvironment _env;
        private readonly RequestDelegate _next;

        public DocumentServeMiddleware(IWebHostEnvironment env, RequestDelegate next)
        {
            _env = env;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            List<string> paths = context.Request.Path.Value.Split("/").ToList();
            if ("GET".Equals(context.Request.Method) && paths.Count == 2)
            {
                var req = context.Request;
                byte[] buffer = getFile(paths[1]);
                if (buffer != null)
                {
                    await context.Response.BodyWriter.WriteAsync(buffer);
                }
                else
                {
                    context.Response.StatusCode = ((int)HttpStatusCode.NotFound);
                }
            }
            else
            {
                await _next(context);
            }
        }

        public byte[] getFile(string name)
        {
            if (System.IO.File.Exists(Path.Combine(_env.ContentRootPath, "Data_Stores", "documents", name)))
            {
                return System.IO.File.ReadAllBytes(Path.Combine(_env.ContentRootPath, "Data_Stores", "documents", name));
            }
            return null;
        }
    }
}
