using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json.Serialization;

using TronWebApp.Hubs;

namespace TronWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    //options.SerializerSettings.Formatting = Formatting.Indented;
                });

            services.AddSignalR();

            services.AddTransient<PlayerMatchmakingService>();
            services.AddTransient<PlayerSpawnService>();            
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseSignalR(route =>
            {
                route.MapHub<TronGameHub>("/tron-game-hub");
            });

            app.Run(async context =>
            {
                await context.Response.WriteAsync("OK");
            });
        }
    }
}
