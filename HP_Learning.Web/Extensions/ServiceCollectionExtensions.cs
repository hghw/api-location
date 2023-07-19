using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using IdentityModel;
using System.Security.Claims;
using IdentityServer4.Models;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using VietGIS.Infrastructure.Modules;
using VietGIS.Infrastructure;
using VietGIS.Infrastructure.Web.ModelBinders;
using VietGIS.Infrastructure.Identity.Entities;
using VietGIS.Infrastructure.Identity.Extensions;
using VietGIS.Infrastructure.Identity.DbContexts;
using VietGIS.Infrastructure.Identity.Managers;
using VietGIS.Infrastructure.Identity.Implements;
using VietGIS.Infrastructure.Identity.Stores;
using VietGIS.Infrastructure.Identity.Services;
using System.ComponentModel;
using IdentityServer4.Services;
using IdentityServer4.Validation;

namespace HP_Learning.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static readonly IModuleConfigurationManager _modulesConfig = new ModuleConfigurationManager();

        public static IServiceCollection AddModules(this IServiceCollection services)
        {
            foreach (var module in _modulesConfig.GetModules())
            {
                if (module.Enabled)
                {
                    if (!module.IsBundledWithHost)
                    {
                        TryLoadModuleAssembly(module.Id, module);
                        if (module.Assembly == null)
                        {
                            throw new Exception($"Cannot find main assembly for module {module.Id}");
                        }
                    }
                    else
                    {
                        module.Assembly = Assembly.Load(new AssemblyName(module.Id));
                    }

                    GlobalConfiguration.Modules.Add(module);
                }
            }

            return services;
        }

        public static IServiceCollection AddCustomizedMvc(this IServiceCollection services, IList<ModuleInfo> modules)
        {
            var mvcBuilder = services
                .AddMvc(o =>
                {
                    o.EnableEndpointRouting = false;
                    o.ModelBinderProviders.Insert(0, new InvariantDecimalModelBinderProvider());
                })
                .AddRazorRuntimeCompilation(options =>
                {
                    foreach (var module in modules.Where(x => x.IsBundledWithHost && x.Enabled))
                    {
                        string modulePath = Path.GetFullPath($"../Modules/{module.Id}");
                        if (Directory.Exists(modulePath))
                        {
                            options.FileProviders.Add(new PhysicalFileProvider(modulePath));
                        }
                    }
                })
                //.AddViewLocalization()
                //.AddModelBindingMessagesLocalizer(services)
                //.AddDataAnnotationsLocalization(o =>
                //{
                //    var factory = services.BuildServiceProvider().GetService<IStringLocalizerFactory>();
                //    var L = factory.Create(null);
                //    o.DataAnnotationLocalizerProvider = (t, f) => L;
                //})
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    // options.SerializerSettings.TypeNameHandling = TypeNameHandling.Objects;
                    options.SerializerSettings.MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead;
                    options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    // options.SerializerSettings.SerializationBinder = new KnownTypesBinder() { KnownTypes = new List<Type>() { typeof(Layer) } };
                });
            foreach (var module in modules.Where(x => !x.IsBundledWithHost && x.Enabled))
            {
                AddApplicationPart(mvcBuilder, module.Assembly);
            }

            return services;
        }

        /// <summary>
        /// Localize ModelBinding messages, e.g. when user enters string value instead of number...
        /// these messages can't be localized like data attributes
        /// </summary>
        /// <param name="mvc"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IMvcBuilder AddModelBindingMessagesLocalizer
            (this IMvcBuilder mvc, IServiceCollection services)
        {
            return mvc.AddMvcOptions(o =>
            {
                var factory = services.BuildServiceProvider().GetService<IStringLocalizerFactory>();
                var L = factory.Create(null);

                o.ModelBindingMessageProvider.SetValueIsInvalidAccessor((x) => L["The value '{0}' is invalid.", x]);
                o.ModelBindingMessageProvider.SetValueMustBeANumberAccessor((x) => L["The field {0} must be a number.", x]);
                o.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor((x) => L["A value for the '{0}' property was not provided.", x]);
                o.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((x, y) => L["The value '{0}' is not valid for {1}.", x, y]);
                o.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(() => L["A value is required."]);
                o.ModelBindingMessageProvider.SetMissingRequestBodyRequiredValueAccessor(() => L["A non-empty request body is required."]);
                o.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor((x) => L["The value '{0}' is not valid.", x]);
                o.ModelBindingMessageProvider.SetNonPropertyUnknownValueIsInvalidAccessor(() => L["The value provided is invalid."]);
                o.ModelBindingMessageProvider.SetNonPropertyValueMustBeANumberAccessor(() => L["The field must be a number."]);
                o.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor((x) => L["The supplied value is invalid for {0}.", x]);
                o.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor((x) => L["Null value is invalid."]);
            });
        }

        private static void AddApplicationPart(IMvcBuilder mvcBuilder, Assembly assembly)
        {
            var partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
            foreach (var part in partFactory.GetApplicationParts(assembly))
            {
                mvcBuilder.PartManager.ApplicationParts.Add(part);
            }

            var relatedAssemblies = RelatedAssemblyAttribute.GetRelatedAssemblies(assembly, throwOnError: false);
            foreach (var relatedAssembly in relatedAssemblies)
            {
                partFactory = ApplicationPartFactory.GetApplicationPartFactory(relatedAssembly);
                foreach (var part in partFactory.GetApplicationParts(relatedAssembly))
                {
                    mvcBuilder.PartManager.ApplicationParts.Add(part);
                }
            }
        }

        public static IServiceCollection AddCustomizedIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            TypeDescriptor.AddAttributes(typeof(Secret), new TypeConverterAttribute(typeof(ClientSecretConverter)));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                // .AddDapperStores(options =>
                // {
                // options.AddUsersTable<LUsersTable, ApplicationUser>();
                // options.AddRolesTable<LRolesTable, ApplicationRole>();
                // })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddNumericTotpTokenProvider();

            services.AddScoped<AuditableSignInManager<ApplicationUser>, AuditableSignInManager<ApplicationUser>>();

            List<Client> clients = new List<Client>();
            configuration.GetSection("Clients").Bind(clients);

            var identityServerBuilder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                //
                options.UserInteraction.LoginUrl = "/account/login";
                options.UserInteraction.LogoutUrl = "/account/logout";
            })
            .AddDeveloperSigningCredential(persistKey: true, filename: "kttv.key")
            // .AddInMemoryPersistedGrants()
            // .AddSigningCredential(cert)
            .AddInMemoryIdentityResources(new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Address(),
                new IdentityResources.Email(),
                new IdentityResources.Phone(),
                new IdentityResource {Name = "given_name", UserClaims = {ClaimTypes.GivenName}},
                new IdentityResource {Name = "role", UserClaims = {ClaimTypes.Role}},
            })
            .AddInMemoryApiResources(new List<ApiResource>
            {
            })
            .AddInMemoryClients(clients)
            // .AddConfigurationStore(options =>
            // {
            //     options.ConfigureDbContext = builder =>
            //     {
            //         builder.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"),
            //             pgsql => pgsql.MigrationsAssembly("DenHung.DataAccess"));
            //     };
            //     options.DefaultSchema = "identity";
            // })
            // .AddOperationalStore(options =>
            // {
            //     options.ConfigureDbContext = builder =>
            //     {
            //         builder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
            //             pgsql => pgsql.MigrationsAssembly("VietGIS.Infrastructure").SetPostgresVersion(new Version(9, 6)));
            //         // builder.UseSnakeCaseNamingConvention();
            //     };
            //     options.DefaultSchema = "identity";
            // })
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<OwnerProfileService>()
            .AddResourceOwnerValidator<OwnerPasswordValidator>();

            services.AddTransient<IGroupStore<ApplicationGroup>, GroupStore<ApplicationGroup, ApplicationDbContext>>();
            services.AddTransient<IUnitStore<ApplicationUnit>, UnitStore<ApplicationUnit, ApplicationDbContext, string, ApplicationUser>>();

            services.AddTransient<UserManager<ApplicationUser>, ApplicationUserManager>();
            // services.AddScoped<RoleManager<ApplicationRole>, CustomRoleManager>();
            services.AddTransient<UnitManager<ApplicationUnit>, ApplicationUnitManager>();
            services.AddTransient<GroupManager<ApplicationGroup>, ApplicationGroupManager>();

            services.AddTransient<IEmailSender, EmailSender>();
            // services.AddTransient<ISmsSender, CustomSMSSender>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";
            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "kttv.cookie";
                options.LoginPath = "/";
                options.AccessDeniedPath = "/Account/AccessDenied";

                // options.LoginPath = string.Empty;
                // options.AccessDeniedPath = string.Empty;
                // options.Events.OnRedirectToLogin = context =>
                // {
                //    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                //    return Task.CompletedTask;
                // };
                //                    options.SessionStore = new MemoryCacheTicketStore();
            })
            .AddOpenIdConnect("oidc", options =>
            {
                options.SignInScheme = "Cookies";
                options.Authority = configuration["Authentication:OIDC:Authority"];
                options.RequireHttpsMetadata = false;

                options.ClientId = configuration["Authentication:OIDC:ClientId"];
                options.ClientSecret = configuration["Authentication:OIDC:ClientSecret"];
                options.ResponseType = configuration["Authentication:OIDC:ResponseType"];
                options.CallbackPath = configuration["Authentication:OIDC:CallbackPath"];
                options.SignedOutCallbackPath = configuration["Authentication:OIDC:SignoutCallbackPath"];

                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;

                // options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                // options.NonceCookie.SameSite = SameSiteMode.Lax;

                // options.Scope.Add("lovelife_api");
                // options.Scope.Add("address");
                // options.Scope.Add("email");
                // options.Scope.Add("phone");
                // options.Scope.Add("offline_access");
                options.Scope.Add("role");

                options.Events = new OpenIdConnectEvents()
                {
                    OnUserInformationReceived = async context =>
                    {
                        if (context.User.RootElement.TryGetProperty(JwtClaimTypes.Role, value: out var roles))
                        {
                            var claims = new List<Claim>();

                            if (roles.ValueKind == System.Text.Json.JsonValueKind.String)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, roles.GetString()));
                            }
                            else if (roles.ValueKind == System.Text.Json.JsonValueKind.Array)
                            {
                                foreach (var p in roles.EnumerateArray().ToList())
                                {
                                    claims.Add(new Claim(ClaimTypes.Role, p.GetString()));
                                }
                            }
                            var id = context.Principal.Identity as ClaimsIdentity;
                            id.AddClaims(claims);
                        }
                    },

                    OnRedirectToIdentityProvider = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.Redirect("/");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    }
                };
            }).AddLocalApi(options =>
            {
                options.ExpectedScope = "api";
            }).Services.ConfigureApplicationCookie(options =>
            {
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            });

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = false;

                // User settings
                options.User.RequireUniqueEmail = false;
                // options.User.AllowedUserNameCharacters = ""
                ;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedAccount = false;
            });

            services.AddScoped<IProfileService, OwnerProfileService>();
            services.AddScoped<IResourceOwnerPasswordValidator, OwnerPasswordValidator>();

            return services;
        }

        public static IServiceCollection AddCustomizedDataStore(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddDbContextPool<SimplDbContext>(options =>
            //    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
            //        b => b.MigrationsAssembly("HP_Learning.Web")));
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), opts => opts.SetPostgresVersion(new Version(9, 6))).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            );
            services.AddDbContext<ApplicationPersistedGrantDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), opts => opts.SetPostgresVersion(new Version(9, 6))).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
            return services;
        }

        private static void TryLoadModuleAssembly(string moduleFolderPath, ModuleInfo module)
        {
            const string binariesFolderName = "bin";
            var binariesFolderPath = Path.Combine(moduleFolderPath, binariesFolderName);
            var binariesFolder = new DirectoryInfo(binariesFolderPath);

            if (Directory.Exists(binariesFolderPath))
            {
                foreach (var file in binariesFolder.GetFileSystemInfos("*.dll", SearchOption.AllDirectories))
                {
                    Assembly assembly;
                    try
                    {
                        assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file.FullName);
                    }
                    catch (FileLoadException)
                    {
                        // Get loaded assembly. This assembly might be loaded
                        assembly = Assembly.Load(new AssemblyName(Path.GetFileNameWithoutExtension(file.Name)));

                        if (assembly == null)
                        {
                            throw;
                        }

                        string loadedAssemblyVersion = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
                        string tryToLoadAssemblyVersion = FileVersionInfo.GetVersionInfo(file.FullName).FileVersion;

                        // Or log the exception somewhere and don't add the module to list so that it will not be initialized
                        if (tryToLoadAssemblyVersion != loadedAssemblyVersion)
                        {
                            throw new Exception($"Cannot load {file.FullName} {tryToLoadAssemblyVersion} because {assembly.Location} {loadedAssemblyVersion} has been loaded");
                        }
                    }

                    if (Path.GetFileNameWithoutExtension(assembly.ManifestModule.Name) == module.Id)
                    {
                        module.Assembly = assembly;
                    }
                }
            }
        }

        private static Task HandleRemoteLoginFailure(RemoteFailureContext ctx)
        {
            ctx.Response.Redirect("/login");
            ctx.HandleResponse();
            return Task.CompletedTask;
        }
    }
}
