using DataAccess.Logic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.AccountSessionServices;
using Services.BackgroundServices;
using Services.EmailSendingMechanism;
using SoftwareTechnologyCalendarApplicationMVC;

namespace SoftwareTechnologyCalendarApplication
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddHttpContextAccessor();

            services.AddSingleton<IUserDataAccess, UserDataAccess>();
            services.AddSingleton<ICalendarDataAccess, CalendarDataAccess>();
            services.AddSingleton<IEventDataAccess, EventDataAccess>();
            services.AddSingleton<IEmailService,EmailService>();
            services.AddSingleton<IAccountSessionManager,AccountSessionManager>();

            services.AddScoped<IActiveUsers, ActiveUsers>();

            services.AddHostedService<AccountTokenExpirationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Authentication}/{action=Login}");
            });
        }
    }
}
