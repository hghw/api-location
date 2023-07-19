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
using System.Text;
using System.Threading.Tasks;

namespace HP_Learning.Web.Middlewares
{
    public class ImageUploadMiddleware
    {
        private IWebHostEnvironment _env;
        private readonly RequestDelegate _next;

        public ImageUploadMiddleware(IWebHostEnvironment env, RequestDelegate next)
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
                    IFormFile chunk = req.Form.Files.FirstOrDefault(x => x.Name == "chunkContent");
                    if (chunk.ContentType.Contains("image"))
                    {
                        string imageName = storeImage(chunk);
                        await context.Response.WriteJsonAsync(new RestData { data = imageName }).ConfigureAwait(true);
                    }
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
            if (Directory.Exists(Path.Combine(_env.ContentRootPath, "Data_Stores", "salt")) == false)
            {
                Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Data_Stores", "salt"));
            }
            if (Directory.Exists(Path.Combine(_env.ContentRootPath, "Data_Stores", "salt", "media")) == false)
            {
                Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Data_Stores", "salt", "media"));
            }
        }

        private string storeImage(IFormFile file)
        {
            string ext = MimeTypesMap.GetExtension(file.ContentType);
            string rdName = $"{StringHelper.RandomFileName()}.{ext}";

            using (FileStream fs = new FileStream(Path.Combine(_env.ContentRootPath, "Data_Stores", "salt", "media", rdName), FileMode.OpenOrCreate))
            {
                file.CopyTo(fs);
            }

            return rdName;
        }
    }
}
