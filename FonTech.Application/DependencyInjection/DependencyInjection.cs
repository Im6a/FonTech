using FonTech.Application.Mapping;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using System.Reflection;
using FonTech.Domain.Interfaces.Services;
using FonTech.Application.Services;
using FonTech.Domain.Dto.Report;
using FonTech.Application.Validations.FluentValidations.Report;
using FonTech.Domain.Interfaces.Validations;
using FonTech.Application.Validations;
using FonTech.Domain.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
namespace FonTech.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(typeof(ReportMapping));

            var options = configuration.GetSection(nameof(RedisSettings));
            var resitUrl = options["Url"];
            var instanceName = options["InstanceName"];

            services.AddStackExchangeRedisCache(redisCacheOptions =>
            {
                redisCacheOptions.Configuration = resitUrl;
                redisCacheOptions.InstanceName = instanceName;
            });

            InitServices(services);
        }
        private static void InitServices(this IServiceCollection services)
        {
            services.AddScoped<IReportValidator, ReportValidator>();
            services.AddScoped<IValidator<CreateReportDto>, CreateReportValidator>();
            services.AddScoped<IValidator<UpdateReportDto>, UpdateReportValidator>();

            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();

        }
    }
}
