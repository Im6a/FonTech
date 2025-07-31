using FonTech.DAL.DependencyInjection;
using FonTech.Application.DependencyInjection;
using Serilog;
using FonTech.Domain.Settings;
using FonTech.Api.Middlewares;
namespace FonTech.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.DefaultSection)); //���������� ��������� Jwt �� appsettings.json � ����������� � �

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddAuthenticationAndAuthorization(builder);


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

            builder.Services.AddSwagger();

            builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));


            builder.Services.AddDataAccessLayer(builder.Configuration);
            builder.Services.AddApplication();

            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FonTech Swagger v1.0"); //���������� � ������ ������ � swagger
                    c.SwaggerEndpoint("/swagger/v2/swagger.json", "FonTech Swagger v2.0");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()); //������ ��� ���� ��� (����� ��� ����� ��������� ���������� �� ����� �������� �� ������ ��������)

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();


        }
    }
}
