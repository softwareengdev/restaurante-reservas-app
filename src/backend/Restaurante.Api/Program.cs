using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Restaurante.Aplicacion.Repository;
using Restaurante.Aplicacion.Services;
using Restaurante.Infraestructura.DBContext;
using Restaurante.Infraestructura.Repository;
using Restaurante.Infraestructura.Repository.Impl;
using Restaurante.Modelo.Model;
using Restaurante.Modelo.Model.Auth;
using Serilog;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

// Assuming namespace matches your project
namespace Restaurante.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configure Serilog for advanced logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
                .Enrich.FromLogContext()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting web host");

                var builder = WebApplication.CreateBuilder(args);

                // Configuration setup with advanced sources
                builder.Configuration
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables()
                    .AddUserSecrets<Program>(); // For sensitive data in development

                builder.Services.AddOptions<JwtSettings>()
                    .Bind(builder.Configuration.GetSection("JwtSettings"))
                    .ValidateDataAnnotations() // Add validation if JwtSettings has attributes
                    .ValidateOnStart(); // Validate during app startup

                // Replace default logging with Serilog
                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console());

                // Add services to the container with advanced features


                // Database context (assuming SQL Server for example; replace with your actual DbContext)
                builder.Services.AddDbContext<RestauranteDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                        sqlOptions => sqlOptions.EnableRetryOnFailure(maxRetryCount: 5)));

                // Authentication with JWT
                var jwtSettings = builder.Configuration.GetSection("JwtSettings");
                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = jwtSettings["Issuer"],
                            ValidAudience = jwtSettings["Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is missing")))
                        };
                    });

                // Authorization policies
                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
                });

                // In Program.cs, update DbContext registration if needed (already there)
                // Also, add Identity services:
                builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    // Advanced password policies
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequiredUniqueChars = 6;

                    // Lockout settings
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.AllowedForNewUsers = true;

                    // User settings
                    options.User.RequireUniqueEmail = true;

                    // Sign-in
                    options.SignIn.RequireConfirmedEmail = false; // Set to true for production with email confirmation
                })
                .AddEntityFrameworkStores<RestauranteDbContext>()
                .AddDefaultTokenProviders(); // For password reset, etc.

                // For refresh tokens, we'll use a custom store (simple DB table)
                builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
                builder.Services.AddScoped<IAuthService, AuthService>();

                // CORS with named policy
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowSpecificOrigins",
                        policy => policy.AllowAnyHeader()
                                        .AllowAnyMethod()
                                        .AllowCredentials());
                });

                // Controllers o Minimal APIs
                builder.Services.AddControllers();

                // Swagger/OpenAPI with advanced configuration
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Restaurante API",
                        Version = "v1",
                        Description = "API para gestión del restaurante",
                        Contact = new OpenApiContact
                        {
                            Name = "Equipo Restaurante",
                            Email = "soporte@restaurante.com"
                        }
                    });
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Please enter JWT with Bearer into field",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                            },
                            Array.Empty<string>()
                        }
                    });

                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                });


                // Rate limiting with sliding window
                builder.Services.AddRateLimiter(options =>
                {
                    options.AddSlidingWindowLimiter(policyName: "Sliding", options =>
                    {
                        options.PermitLimit = 100;
                        options.Window = TimeSpan.FromMinutes(1);
                        options.SegmentsPerWindow = 10;
                        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                        options.QueueLimit = 50;
                    });
                });

                // Output caching
                builder.Services.AddOutputCache(options =>
                {
                    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(30)));
                });

                // Response compression
                builder.Services.AddResponseCompression(options =>
                {
                    options.EnableForHttps = true;
                });

                // Register AutoMapper: Scans the current assembly for profiles
                builder.Services.AddAutoMapper(typeof(Program));  // Or specify assemblies: AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies())

                // Optionally, add global configuration
                builder.Services.AddAutoMapper(config => {
                    config.AllowNullCollections = true;  // Advanced: Allow null collections without throwing
                    /*config.MaxDepth = 3;*/  // Prevent deep recursion in nested mappings
                });

                // Custom services (example; adjust to your needs)
                // In Program.cs, register the repositories and services
                builder.Services.AddScoped<IMesaRepository,MesaRepository>();
                builder.Services.AddScoped<IReservaRepository,ReservaRepository>();
                builder.Services.AddScoped<IClienteRepository, ClienteRepository>();

                builder.Services.AddScoped<IMesaService, MesaService>();
                builder.Services.AddScoped<IReservaService,ReservaService>();
                builder.Services.AddScoped<IClienteService,ClienteService>();

                var app = builder.Build();

                // Configure the HTTP request pipeline with advanced middleware

                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseSwagger();
                    app.UseSwaggerUI(options =>
                    {
                        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurante API v1");
                    });
                }
                else
                {
                    app.UseExceptionHandler("/Error");
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseRouting();

                app.UseCors("AllowSpecificOrigins");

                app.UseAuthentication();
                app.UseAuthorization();

                app.UseRateLimiter();

                app.UseOutputCache();

                app.UseResponseCompression();

                app.MapControllers();

                // Database migration on startup (for demo; use with caution in prod)
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<RestauranteDbContext>();
                    dbContext.Database.Migrate();
                }

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}