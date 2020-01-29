using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegionTDServerReborn.ModelBinder;
using LegionTDServerReborn.Models;
using LegionTDServerReborn.Seed;
using LegionTDServerReborn.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.HttpOverrides;

namespace LegionTDServerReborn
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc(config =>
            {
                config.EnableEndpointRouting = false;
                config.ModelBinderProviders.Insert(0, new InvariantFloatModelBinderProvider());
            }).AddRazorPagesOptions(options => {
                options.Conventions.AddPageRoute("/Ranking", "Players");
                options.Conventions.AddPageRoute("/Player", "Players/{playerId:long}");
                options.Conventions.AddPageRoute("/Match", "Matches/{matchId:int}");
                options.Conventions.AddPageRoute("/Builder", "Builders/{builder}");
            });

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddTransient<SteamApi>();
            services.AddTransient<FileLogger>();

            services.AddMemoryCache();
            services.AddDbContext<LegionTdContext>(
                options => {
                    options.UseMySql(Configuration.GetConnectionString("MySQLConnection"),
                                     sqlServerOptions => {
                                         sqlServerOptions.CommandTimeout(36000);
                                        });
                });
            services.Configure<GzipCompressionProviderOptions>(options => options.Level =
                System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseMvc(routes =>
            {
               routes.MapRoute("default", "{controller=legiontd}/");
            });
            //app.UseDeveloperExceptionPage();
            app.UseExceptionHandler("/Error");

            app.UseStaticFiles();
//            LegionTdContextMover.SetTraining();
//            LegionTdContextMover.Seed();
        }
    }
}
