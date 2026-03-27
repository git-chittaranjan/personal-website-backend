using Azure.Identity;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using my_api_app.Core.Extensions;
using my_api_app.Core.Filters.Authorization;
using my_api_app.Core.Filters.Logging;
using my_api_app.Core.Helpers;
using my_api_app.Core.Middlewares.ExceptionHandling;
using my_api_app.Core.Middlewares.Logging;
using my_api_app.Core.Responses;
using my_api_app.Features.Auth.Services;
using my_api_app.Features.Auth.Validators.Register;
using my_api_app.Features.Health.Services;
using my_api_app.Features.User.DTOs.AboutMe;
using my_api_app.Features.User.Services;
using my_api_app.Infrastructure.Database;
using my_api_app.Infrastructure.Email;
using my_api_app.Infrastructure.OTP;
using my_api_app.Infrastructure.Security.Hasher;
using my_api_app.Infrastructure.Security.Token;
using my_api_app.Repositories.Auth;
using my_api_app.Repositories.HealthRepo;
using my_api_app.Repositories.User;
using my_api_app.Repositories.UserRepo;
using Serilog;
using System.Text;
using System.Text.Json;

Serilog.Debugging.SelfLog.Enable(msg =>
    System.IO.File.AppendAllText(
        "C:\\home\\LogFiles\\Application\\serilog-selflog.txt",
        $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} {msg}{Environment.NewLine}"
    )
);

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
builder.Services.AddHttpContextAccessor();



//Force UTC at application level — works on both local and Azure (Azure by default UTC)
System.Environment.SetEnvironmentVariable("TZ", "UTC");
TimeZoneInfo.ClearCachedData();


// ── Key Vault Configuration ─────────────────────────────────────────────────────────
// This code dynamically loads secrets from Azure Key Vault into your ASP.NET Core configuration at runtime
var keyVaultName = builder.Configuration["KeyVaultName"];
if (!string.IsNullOrWhiteSpace(keyVaultName))
{
    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");

    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential()); // Connects to Key Vault, Lads all secrets into Configuration ans makes them accessible like normal config values
}



// It registers and configures Application Insights SDK in the dependency injection (DI) container.
builder.Services.AddApplicationInsightsTelemetry();



// ------------------------------
// Logging Implementation
// ------------------------------
builder.Logging.ClearProviders();

builder.Host.UseSerilog((ctx, services, config) =>
{
    config
        .ReadFrom.Configuration(ctx.Configuration) // Reads Serilog config block from appsettings.json
        .ReadFrom.Services(services); // Adds DI like sinks (Azure AppInsights, MS SQL Server etc.) which are registered in Program.cs
});

//builder.Logging.AddFile(o => o.RootPath = builder.Environment.ContentRootPath); -- Karambolo Package
builder.Services.AddHttpLoggingConfiguration(builder.Environment);




// ------------------------------
// Dependency Injection (Filters)
// ------------------------------
builder.Services.AddScoped<ApiKeyFilter>();
builder.Services.AddScoped<ActionLoggingFilter>();



// ------------------------------
// Controllers + FluentValidation + Filters + snake_case
// ------------------------------
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiKeyFilter>();
    options.Filters.Add<ActionLoggingFilter>();
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy(); // For snake_case, we need a custom policy
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never; //return property with null value
    });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<UserRegisterRequestDtoValidator>();



// ------------------------------
// Dependency Injection (Services)
// ------------------------------
// Infrastructure ────────────────────────────────────────────
builder.Services.AddSingleton<IDbConnectionFactory, SqlServerConnectionFactory>(); //Singleton BD Connection
builder.Services.AddSingleton<IApiResponseFactory, ApiResponseFactory>();

// Utilities / Low-Level Services ───────────────────────────
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IResetTokenHasher, ResetTokenHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IHealthService, HealthService>();

// Repositories ──────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPendingUserRepository, PendingUserRepository>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IHealthRepository, HealthRepository>();

// Application / Domain Services ────────────────────────────
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

// Misc ──────────────────────────────────────────────────────
builder.Services.AddCustomModelValidationResponse();



// ------------------------------
// Kestrel — local only, Azure manages its own ports
// ------------------------------
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(5000);
        options.ListenLocalhost(5001, listenOptions =>
        {
            listenOptions.UseHttps();
        });
    });
}



// ------------------------------
// JWT Authentication
// ------------------------------
var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();



// ------------------------------
// Add CORS Service
// ------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowChittaranjanApp", policy =>
        policy.WithOrigins(
            "https://chittaranjansaha.com",
            "https://www.chittaranjansaha.com/",
            "http://localhost:3000"
            )
              .WithMethods("GET", "POST")
              .AllowAnyHeader());
});



// ------------------------------
// Add HSTS Service
// ------------------------------
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
    options.ExcludedHosts.Add("localhost");
});






// ===============================================================================================================
// Build app
// ===============================================================================================================

WebApplication app = builder.Build();
Console.WriteLine($"Environment Name: {app.Environment.EnvironmentName}");



// ------------------------------
// Middlewares Registration
// ------------------------------
app.UseConsoleRequestLogger();

app.UseCorrelationGeneratorIdMiddleware();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
}); // o/p - HTTP POST /api/auth/login responded 200 in 123.4560ms (UseHttpLogging() Middleware does same with extra details



app.UseHttpLogging(); // Built in Middleware to capture req/res logs

app.UseGlobalExceptionMiddleware();

if (!app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/api/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting(); //Not required implicitily defined
app.UseCors("AllowChittaranjanApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/api/chittaranjan", permanent: true));
app.MapGet("/api/chittaranjan", async context =>
{
    var aboutMe = new AboutMeResponseDto()
    {
        Name = "Chittaranjan Saha",
        Gender = "Male",
        //Age = 27,
        Role = "Full Stack Developer",
        Skills = new List<string> { ".NET", "SQL Server", "React" },
        ExperienceYears = 5,
        Location = "India",
        Interests = new List<string> { "System Design", "Clean Architecture", "SOLID Principles" }
    };
    var json = JsonSerializer.Serialize(aboutMe);
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(json);
});



// ------------------------------
// Bootstrap Logging
// ------------------------------
app.Lifetime.ApplicationStarted.Register(() =>
{
    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Program>();

    logger.LogInformation("Application started. Environment: {Environment} | Time: {Time}", app.Environment.EnvironmentName, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));
});



app.Run();