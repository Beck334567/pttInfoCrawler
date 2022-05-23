using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using pttInfoCrawler.Data;
using pttInfoCrawler.Model;

namespace pttInfoCrawler
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddOptions();
            services.AddMvc();
            services.Configure<LineSetting>(Configuration.GetSection("LineSetting"));
            services.AddAuthentication(IISDefaults.AuthenticationScheme);
            services.AddHostedService<NotifyTaskManager>();
            //services.AddHostedService<TaskManager>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApi", Version = "v1" });
            });
            services.AddDbContext<ApplicationDbContext>(builder =>
            {
                builder.UseNpgsql(Configuration.GetConnectionString("LineDb"),
                    optionsBuilder =>
                    {
                        // Heroku PostgreSQL 必須使用 SSL
                        // 如果沒設定這行，會在連線時拿到 SSL off 的錯誤
                        optionsBuilder.RemoteCertificateValidationCallback(
                                    (_, _, _, _) => true);
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "pttInfoCrawler v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}