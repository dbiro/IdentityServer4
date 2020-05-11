// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Api
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;

                    options.Audience = "api1";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {                        
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero // See: https://github.com/IdentityServer/IdentityServer4/issues/497
                    };
                });

            services.AddCors(options =>
            {
                // this defines a CORS policy called "default"
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins("http://localhost:5003")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseCors("default");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}