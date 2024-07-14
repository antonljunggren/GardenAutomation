using Core.Shared.CAN;
using Infrastructure;
using Microsoft.Extensions.FileProviders;
using WebAPI.Middleware;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddInfrastructure();
            builder.Services.AddServices();
            builder.Services.AddBackgroundServices();

            builder.Services.AddCors(opt => opt.AddPolicy("CORS", builder =>
            {
                builder.WithOrigins("*")
                .AllowAnyMethod()
                .AllowAnyHeader();
            }));

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseCors("CORS");

            //app.UseHttpsRedirection();

            var clientFilesPath = Path.Combine(app.Environment.ContentRootPath, "Client", "dist");

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(clientFilesPath),
                RequestPath = ""
            });

            app.UseAuthorization();

            app.UseMiddleware<ExceptionLoggingMiddleware>();

            app.MapControllers();

            app.MapFallbackToFile("/index.html", new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(clientFilesPath)
            });

            var canService = app.Services.GetRequiredService<ICanService>();
            var canHandler = app.Services.GetRequiredService<CanHandler>();
            canService.Start();

            app.Run();
            
        }
    }
}
