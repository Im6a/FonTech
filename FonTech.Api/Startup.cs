using Asp.Versioning;
using System.Runtime;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography.Xml;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using FonTech.Domain.Settings;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace FonTech.Api
{
    public static class Startup
    {
        /// <summary>
        /// Подключение аутентификации и авторизации
        /// </summary>
        /// <param name="services"></param>
        public static void AddAuthenticationAndAuthorization(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddAuthorization();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                var options = builder.Configuration.GetSection(JwtSettings.DefaultSection).Get<JwtSettings>();
                var jwtKey = options.JwtKey;
                var issuer = options.Issuer;
                var audience = options.Audience;
                var lifeTime = options.LifeTime;
                var refreshTokenValididtyInDays = options.RefreshTokenValidityInDays;

                o.Authority = options.Authority;
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });
        }



        /// <summary>
        /// Подключение Swagger
        /// </summary>
        /// <param name="services"></param>
        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddApiVersioning()
                .AddApiExplorer(options =>
                {
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                });
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "FonTech.API",
                    Description = "This is swagger documentation",
                    Version = "v1",
                    TermsOfService = new Uri("https://google.com"),
                    Contact = new OpenApiContact()
                    {
                        Name = "Test contact",
                        Url = new Uri("https://yandex.ru"),
                        Email = "Arseniy2018@yandex.ru"
                    }
                });

                options.SwaggerDoc("v2", new OpenApiInfo
                {
                    Title = "FonTech.API",
                    Description = "This is swagger documentation",
                    Version = "v2",
                    TermsOfService = new Uri("https://google.com"),
                    Contact = new OpenApiContact()
                    {
                        Name = "Test contact2",
                        Url = new Uri("https://yandex.ru"),
                        Email = "Arseniy2018@yandex.ru"
                    }
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    In = ParameterLocation.Header,
                    Description = "Введите пожалуйста валидный токен",
                    Name = "Авторизация",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        Array.Empty<string>()
                    }

                });

                var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
            });

        }
    }
}
