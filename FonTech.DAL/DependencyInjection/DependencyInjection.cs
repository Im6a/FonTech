using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using FonTech.DAL.Interceptors;
using FonTech.Domain.Interfaces.Repositories;
using FonTech.Domain.Entity;
using FonTech.DAL.Repositories;
using FonTech.Domain.Interfaces.Databases;

namespace FonTech.DAL.DependencyInjection
{
    public static class DependencyInjection
    {
        public static void AddDataAccessLayer(this IServiceCollection service, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("PostgresSQL");

            service.AddSingleton<DateInterceptor>();

            service.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });
            service.InitRepositories();
        }

        private static void InitRepositories(this IServiceCollection services)
        {
            services.AddScoped<IBaseRepository<User>, BaseRepository<User>>();
            services.AddScoped<IBaseRepository<Role>, BaseRepository<Role>>();
            services.AddScoped<IBaseRepository<UserRole>, BaseRepository<UserRole>>();
            services.AddScoped<IBaseRepository<Report>, BaseRepository<Report>>();
            services.AddScoped<IBaseRepository<UserToken>, BaseRepository<UserToken>>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
