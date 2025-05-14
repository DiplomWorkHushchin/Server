using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Interfaces.Helpers;
using API.Interfaces.Services;
using API.Services;
using API.Services.ContollersServices;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers();
        services.AddDbContext<DataContext>(opt =>
        {
            opt.UseNpgsql(config.GetConnectionString("DefaultConnection"));
        });

        services.AddCors();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<ICoursePermissionsService, CoursePermissionsService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IDateTimeConverter, DateTimeConverter>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICourseService, CourseService>();

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddHttpContextAccessor();

        return services;
    }
}
