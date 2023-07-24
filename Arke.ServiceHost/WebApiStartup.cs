using System;
using System.Collections.Generic;
using System.Reflection;
using Arke.DependencyInjection;
using Arke.ManagementApi.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleInjector.Integration.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Linq;
using Arke.SipEngine.Api;
using SimpleInjector;

namespace Arke.ServiceHost
{
    public class WebApiStartup
    {
        const string apiScope = "Arke.Editor.Api";

        public WebApiStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var mvc = services.AddMvc((options => { options.EnableEndpointRouting = false;}));

            AppDomain.CurrentDomain.GetAssemblies()//Get all the assemblies in the app domain
                .SelectMany(_ => _.GetTypes()).Distinct()//Get all the contained types and remove dups
                .Where(t => !t.IsAbstract && !t.IsInterface && t.GetInterfaces().Any(i => i == typeof(IManagementAPIController)))//Filter for non abstract classes that implement IWebApiController
                .Select(_ => new AssemblyPart(_.Assembly))//Select results as AssemblyParts
                .Union(new[] { new AssemblyPart(typeof(StepsController).GetTypeInfo().Assembly) })//Union the management API Assembly to the results
                .Distinct()//Remove duplicates
                .ToList()//Cast to list for foreach extension method
                .ForEach(_ => mvc.ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(_)));//Add the assemblyparts to the MVC Builder object

            services.AddMicrosoftIdentityWebApiAuthentication(Configuration, "AzureAd");


            var corsOrigins = Configuration.GetSection("appSettings:corsOrigins").GetChildren().ToArray().Select(c => c.Value).ToArray();
            services.AddCors(options =>
            {
                options.AddPolicy(name: "ArkeAllowedOrigins", builder =>
                {
                    builder.WithOrigins(corsOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddSimpleInjector(ObjectContainer.GetInstance().GetSimpleInjectorContainer(), options =>
            {
                options.AddAspNetCore()
                .AddControllerActivation();
                options.AddLogging();
            });

            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSimpleInjector(ObjectContainer.GetInstance().GetSimpleInjectorContainer());
            app.UseCors("ArkeAllowedOrigins");
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arke API"));
            app.UseRouting();
            app.UseMvc();
        }

    }
}