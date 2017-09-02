using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegionTDServerReborn.ModelBinder;
using LegionTDServerReborn.Models;
using LegionTDServerReborn.Seed;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LegionTDServerReborn
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
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
                config.ModelBinderProviders.Insert(0, new InvariantFloatModelBinderProvider());
            });

            services.AddSingleton<IConfiguration>(Configuration);

            services.AddMemoryCache();
            services.AddDbContext<LegionTdContext>(
                options => options.UseMySql(Configuration.GetConnectionString("MySQLConnection")));
            services.Configure<GzipCompressionProviderOptions>(options => options.Level =
                System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=legiontd}/");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
//            LegionTdContextMover.SetTraining();
//            LegionTdContextMover.Seed();
        }
    }
}
