// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Http;
using System;

namespace IdentityServer
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            var builder = services.AddIdentityServer()
                .AddInMemoryIdentityResources(Config.Ids)
                .AddInMemoryApiResources(Config.Apis)
                .AddInMemoryClients(Config.Clients)
                .AddTestUsers(TestUsers.Users);                        

            builder.AddDeveloperSigningCredential();

            var azureAdConfig = configuration.GetSection("AzureAD").Get<AzureADConfiguration>();

            services.AddAuthentication()
                //.AddAzureAD(AzureADDefaults.AuthenticationScheme, AzureADDefaults.OpenIdScheme, , AzureADDefaults.DisplayName, options =>
                //.AddAzureAD(options =>
                //{
                //    options.ClientId = azureAdConfig.ClientId;
                //    options.ClientSecret = azureAdConfig.ClientSecret;
                //    options.CallbackPath = "/signin-oidc";
                //    options.Instance = "https://login.microsoftonline.com";
                //    //options.TenantId = "common";
                //    options.TenantId = azureAdConfig.TenantId;
                //    options.CookieSchemeName = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                //});
                .AddOpenIdConnect("oidc", "Sign in with AzureAD", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                    options.SaveTokens = true;

                    options.Authority = string.Format("https://login.microsoftonline.com/{0}", azureAdConfig.TenantId);
                    //options.Authority = "https://login.microsoftonline.com/common";
                    options.ClientId = azureAdConfig.ClientId;
                    options.ClientSecret = azureAdConfig.ClientSecret;
                    options.ResponseType = OpenIdConnectResponseType.CodeIdToken;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                });                
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();                
            }

            app.UseStaticFiles();
            app.UseRouting();
                        
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}