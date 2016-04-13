using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MvcControlsToolkit.Core.ITests.Models;
using MvcControlsToolkit.Core.ITests.Services;
using MvcControlsToolkit.Core.ITests.Options;
using MvcControlsToolkit.Core.Options.Extensions;
using MvcControlsToolkit.Core.Options.Providers;
using MvcControlsToolkit.Core.Extensions;
using Microsoft.AspNet.Localization;
using System.Globalization;

namespace MvcControlsToolkit.Core.ITests
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
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

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddLocalization(m => { m.ResourcesPath = "Resources"; });

            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization(); 

            services.AddMvcControlsToolkit(m => { m.CustomMessagesResourceType = typeof(Resources.ErrorMessages); });

            services.AddPreferences()
                .AddPreferencesClass<WelcomeMessage>("UI.Strings.Welcome")
                .AddPreferencesProvider(new ApplicationConfigurationProvider("UI", Configuration)
                {
                    SourcePrefix = "CustomUI"
                })
                .AddPreferencesProvider(new CookieProvider("UI.Strings.Welcome", "_welcome") {
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
                .AddPreferencesProvider(new ClaimsProvider<ApplicationUser>("UI.Strings.Welcome"){
                    Priority=2,
                    AutoCreate=true,
                    SourcePrefix= "http://www.mvc-controls.com/Welcome"
                })
                .AddPreferencesProvider(new RequestProvider("UI.Strings.Welcome")
                {
                    Priority = 10,
                    SourcePrefix = "Welcome"
                }) ;

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                // For more details on creating database during deployment see http://go.microsoft.com/fwlink/?LinkID=615859
                try
                {
                    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                        .CreateScope())
                    {
                        serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
                             .Database.Migrate();
                    }
                }
                catch { }
            }

            var requestLocalizationOptions = new RequestLocalizationOptions
            {
                SupportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("it"),
                },
                SupportedUICultures = new List<CultureInfo>
                {
                    new CultureInfo("it"),
                }  
            };

            app.UseRequestLocalization(requestLocalizationOptions, new RequestCulture(new CultureInfo("it")));

            app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseIdentity();

            // To configure external authentication please see http://go.microsoft.com/fwlink/?LinkID=532715
            app.UsePreferences();
            app.UseMvcControlsToolkit();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
