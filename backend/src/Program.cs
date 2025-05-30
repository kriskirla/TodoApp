using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using NReco.Logging.File;
using System.Text;
using TodoApp.Data;
using TodoApp.Services;
using TodoApp.Util;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

namespace TodoApp;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Enable logging
        builder.Logging.ClearProviders();
        builder.Logging.AddFile("Logs/todoapp.txt");

        // Register services
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ITodoService, TodoService>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUserContext, UserContext>();

        // Add controllers, swagger, SignalR, DbContextPool, etc.
        builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // This is added to make Enum more readable
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and then your valid token."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference= new OpenApiReference { Type= ReferenceType.SecurityScheme, Id= "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        builder.Services.AddSignalR();
        builder.Services.AddDbContextPool<AppDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });
        });

        // Authentication and Authorization
        var secretKey = builder.Configuration["Jwt:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new Exception("JWT SecretKey is not configured.");
        }

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/todohub"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });
        builder.Services.AddSingleton(new JwtUtil(secretKey));
        builder.Services.AddAuthorization();

        // Configure CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", builder =>
            {
                builder
                    .WithOrigins("http://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials(); // This is required by SignalR
            });
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        }

        // Allow serving static files
        var mediaDir = Path.Combine(Directory.GetCurrentDirectory(), "media");
        if (!Directory.Exists(mediaDir))
        {
            Directory.CreateDirectory(mediaDir);
        }
        app.UseStaticFiles();
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "media")),
            RequestPath = "/media"
        });

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
            c.RoutePrefix = string.Empty;
        });

        app.UseRouting();
        app.UseCors("AllowFrontend");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapHub<TodoHub>("/todohub");
        app.MapControllers();
        app.Run();
    }
}
