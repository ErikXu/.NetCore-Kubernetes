using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;

namespace Logging
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            //https://github.com/serilog/serilog-extensions-logging
            Log.Logger = new LoggerConfiguration().Enrich.FromLogContext()
                                                  .WriteTo.LiterateConsole()
                                                  .WriteTo.RollingFile(env.IsDevelopment() ? @"log\{Date}.log" : "/log/service-{Date}.log")
                                                  .CreateLogger();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {
            //https://github.com/serilog/serilog-extensions-logging
            loggerFactory.AddSerilog();

            // Ensure any buffered events are sent at shutdown
            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            app.UseMvc();
        }
    }
}
