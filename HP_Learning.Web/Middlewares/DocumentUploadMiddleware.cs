using VietGIS.Infrastructure.Models.DTO.Response;
using VietGIS.Infrastructure.Helpers;
using HeyRed.Mime;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HP_Learning.Web.Middlewares
{
    public class DocumentUploadMiddleware
    {
        private IWebHostEnvironment _env;
        private readonly RequestDelegate _next;

        public DocumentUploadMiddleware(IWebHostEnvironment env, RequestDelegate next)
        {
            _env = env;
            _next = next;
            checkAndCreateCacheDir();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if ("POST".Equals(context.Request.Method) && context.Request.Form.Files.Count > 0)
            {
                var req = context.Request;
                if (req.Form.Files.Any(x => x.Name == "chunkContent"))
                {
                    List<string> documents = storeDocuments(req.Form.Files.Where(x => x.Name == "chunkContent").ToList());
                    await context.Response.WriteJsonAsync(new RestData { data = documents });
                }
            }
            else
            {
                // Call the next delegate/middleware in the pipeline
                await _next(context);
            }
        }

        private void checkAndCreateCacheDir()
        {
            if (Directory.Exists(Path.Combine(_env.ContentRootPath, "Data_Stores", "documents")) == false)
            {
                Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Data_Stores", "documents"));
            }
        }

        private List<string> storeDocuments(List<IFormFile> files)
        {
            List<string> names = new List<string>();
            foreach (var file in files)
            {
                string ext = MimeTypesMap.GetExtension(file.ContentType);
                string rdName = $"{StringHelper.RandomFileName()}.{ext}";

                using (FileStream fs = new FileStream(Path.Combine(_env.ContentRootPath, "Data_Stores", "documents", rdName), FileMode.OpenOrCreate))
                {
                    file.CopyTo(fs);
                }

                names.Add(rdName);
            }
            return names;
        }
    }
}
