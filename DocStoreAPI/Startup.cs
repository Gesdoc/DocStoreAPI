﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using DocStoreAPI.Models;
using DocStoreAPI.Models.Stor;
using DocStoreAPI.Services;
using DocStoreAPI.Repositories;

namespace DocStoreAPI
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
            services.AddDbContext<DocStoreContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddSingleton(typeof(StorConfig), StorConfigFactory.GetStorConfig(Configuration.GetValue<string>("StorConfigFilePath")));
            services.AddScoped<MetadataRepository>();
            services.AddScoped<DocumentRepository>();
            services.AddScoped<SecurityRepository>();
            services.AddLogging(configure => configure.AddEventSourceLogger());
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultScheme = IISDefaults.AuthenticationScheme;
            })
            .AddAzureAdBearer(options => Configuration.Bind("AzureAd", options));
            services.AddAuthorization(options =>
            {
                //Standard User Policy
                options.AddPolicy("AzAppUser", policy => policy
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireRole("AzDocStoreUser")
                    .Build());
                options.AddPolicy("ADAppUser", policy => policy
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(IISDefaults.AuthenticationScheme)
                    .RequireRole("ADDocStoreUser")
                    .Build());

                //Admin User Policy
                options.AddPolicy("AzAppAdmin", policy => policy
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireRole("AzDocStoreAdmin")
                    .Build());
                options.AddPolicy("ADAppAdmin", policy => policy
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(IISDefaults.AuthenticationScheme)
                    .RequireRole("ADDocStoreAdmin")
                    .Build());

                //Auditor User Policy
                options.AddPolicy("AzAppAuditor", policy => policy
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireRole("AzDocStoreAuditor")
                    .Build());
                options.AddPolicy("ADAppAuditor", policy => policy
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(IISDefaults.AuthenticationScheme)
                    .RequireRole("ADDocStoreAuditor")
                    .Build());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}