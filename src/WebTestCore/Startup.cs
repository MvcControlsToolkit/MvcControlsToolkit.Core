using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MvcControlsToolkit.Core.Extensions;
using MvcControlsToolkit.Core.Options.Extensions;
using MvcControlsToolkit.Core.Options.Providers;
using WebTestCore.Data;
using WebTestCore.Models;
using WebTestCore.Options;
using WebTestCore.Services;

namespace WebTestCore
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();

                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();
            services.AddMvcControlsToolkit(m => { m.CustomMessagesResourceType = typeof(Resources.ErrorMessages); });
            services.AddPreferences()
                .AddPreferencesClass<WelcomeMessage>("UI.Strings.Welcome")
                .AddPreferencesProvider(new ApplicationConfigurationProvider("UI", Configuration)
                {
                    SourcePrefix = "CustomUI"
                })
                .AddPreferencesProvider(new CookieProvider("UI.Strings.Welcome", "_welcome")
                {
                    Priority = 1,
                    AutoCreate = true,
                    WhenEnabled = x => x.User == null || x.User.Identity == null || !x.User.Identity.IsAuthenticated
                })
                //.AddPreferencesProvider(new EntityFrameworkProvider<ApplicationUser, WelcomeMessage>(
                //    "UI.Strings.Welcome",
                //    user => new WelcomeMessage
                //    {
                //        Message = "Welcome " + user.UserName,
                //        AddDate = true
                //    }
                //    )
                //{
                //    Priority = 2
                //})
                .AddPreferencesProvider(new ClaimsProvider<ApplicationUser>("UI.Strings.Welcome")
                {
                    Priority = 2,
                    AutoCreate = true,
                    SourcePrefix = "http://www.mvc-controls.com/Welcome"
                })
                .AddPreferencesProvider(new RequestProvider("UI.Strings.Welcome")
                {
                    Priority = 10,
                    SourcePrefix = "Welcome"
                });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            var supportedCultures = new[]
              {

                  new CultureInfo("en-AU"),
                  new CultureInfo("en-GB"),
                  new CultureInfo("en"),
                  new CultureInfo("es-MX"),
                  new CultureInfo("es"),
                  new CultureInfo("fr-CA"),
                  new CultureInfo("fr"),
                  new CultureInfo("it-CH"),
                  new CultureInfo("it")
              };
            var supportedUICultures = new[]
              {
                  new CultureInfo("en"),
              };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en"),

                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedUICultures,
                FallBackToParentCultures = true,
                FallBackToParentUICultures = true
            });

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseIdentity();

            app.UsePreferences();

            app.UseMvcControlsToolkit();
            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
