using CompanyEmployees.ActionFilters;
using CompanyEmployees.Extensions;
using CompanyEmployees.Utility;
using Contracts;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using Repository.DataShaping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace CompanyEmployees
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            // using NLog’s LogManager static class with the LoadConfiguration method to provide a path to the configuration file.
            LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // add service extension, CORS and IIS configuration
            services.ConfigureCors();
            services.ConfigureIISIntegration();
            services.ConfigureLoggerService();
            services.ConfigureSqlContext(Configuration);
            services.ConfigureRepositoryManager();
            services.AddAutoMapper(typeof(Startup));
            
            // register Action Filters
            services.AddScoped<ValidationFilterAttribute>();
            services.AddScoped<ValidateCompanyExistsAttribute>();
            services.AddScoped<ValidateEmployeeForCompanyExistsAttribute>();
            // 
            services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();
            services.AddScoped<IDataShaper<CompanyDto>, DataShaper<CompanyDto>>();

            services.AddScoped<ValidateMediaTypeAttribute>();

            services.AddScoped<EmployeeLinks>();
            services.AddScoped<CompanyLinks>();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddControllers(config =>
            {
                config.RespectBrowserAcceptHeader = true;
                config.ReturnHttpNotAcceptable = true;
                //config.CacheProfiles.Add("120SecondsDuration", new CacheProfile { Duration = 120 });
            }).AddNewtonsoftJson()
              .AddXmlDataContractSerializerFormatters()
              .AddCustomCSVFormatter();
            
            services.AddCustomMediaTypes();

            services.ConfigureVersioning();

            // Cache configuration
            services.ConfigureResponseCaching();
            
            services.ConfigureHttpCacheHeaders();
            services.AddHttpContextAccessor();
            // 

            services.AddAuthentication();
            services.ConfigureIdentity();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerManager logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // will add middleware for using HSTS, which adds the Strict-Transport-Security header.
                app.UseHsts(); 
            }
            app.ConfigureExceptionHandler(logger);
            app.UseHttpsRedirection();
            // enables using static files for the request. If we don’t set a path to the static files directory, it will use a wwwroot folder in our project by default.
            app.UseStaticFiles();

            // will forward proxy headers to the current request. This will help us during application deployment.
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });
            //

            app.UseRouting();
            app.UseCors("CorsPolicy");

            app.UseResponseCaching();
            app.UseHttpCacheHeaders();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name : "default",
                    pattern : "{controller:Home}/{action=Index}/{id?}");
            });
        }
    }
}
