﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rezolver.Logging;

namespace Rezolver.Examples.AspnetCore
{
	public class Startup
	{
		private ILoggerFactory _loggerFactory;

		public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();

			//_loggerFactory is captured early to support the ConfigureDevelopment_LoggingServices
			//version.
			_loggerFactory = loggerFactory;
		}

		public IConfigurationRoot Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			// Add framework services.
			services.AddMvc();

			//create the Rezolver container from the services, without making any additional
			//registrations etc.
			return services.CreateRezolverContainer();
		}

		public IServiceProvider ConfigureDevelopment2Services(IServiceCollection services)
		{
			// Add framework services.
			services.AddMvc();

			//alternatively, you can create a container in advance and then populate it with the services.
			//this is the most likely way you'll do it, given that you're probably going to want to register
			//your own application's services etc directly into the container.
			//Note - it's very important the top-level container is also a ScopedContainer.
			var container = new ScopedContainer();
			container.Populate(services);
			return container;
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			_loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			_loggerFactory.AddDebug(LogLevel.Debug);
			//slight modification to the 'standard' template's environment check purely 
			//to cater for the additional environments that we have configured for these Rezolver demos.
			if (env.IsDevelopment() || env.EnvironmentName.StartsWith("Development"))
			{
				app.UseDeveloperExceptionPage();
				app.UseBrowserLink();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
				  name: "default",
				  template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
