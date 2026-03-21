using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.IdentityModel.Tokens;
using my_api_app.Data;
using my_api_app.DTOs;
using my_api_app.Filters.Authorization;
using my_api_app.Filters.Logging;
using my_api_app.Helpers;
using my_api_app.Middlewares.Exception;
using my_api_app.Middlewares.Logging;
using my_api_app.Repositories.Auth.Implementations;
using my_api_app.Repositories.Auth.Interfaces;
using my_api_app.Responses;
using my_api_app.Responses.Extensions;
using my_api_app.Services.Auth;
using my_api_app.Services.Security.Implementations;
using my_api_app.Services.Security.Interfaces;
using my_api_app.Services.User;
using my_api_app.Validators.Auth.Register;
using System.Text;
using System.Text.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

//Force UTC at application level — works on both local and Azure (Azure by default UTC)
System.Environment.SetEnvironmentVariable("TZ", "UTC");
TimeZoneInfo.ClearCachedData();



// ------------------------------
// Logging Implementation
// ------------------------------
// ── Logger Step 1: Writing Log into Console Provider ────────────────────────────────────────────
builder.Logging.ClearProviders(); //Clear default providers (Console, Debug, EventSource, EventLog)

// ── Logger Step 2: Writing Log into Console Provider ────────────────────────────────────────────
if (builder.Environment.IsProduction())
{
    builder.Logging.AddJsonConsole(options =>
    {
        options.IncludeScopes = true;
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
        options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions
        {
            Indented = false
        };
    });
}
else
{
    builder.Logging.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
        options.SingleLine = true;
    });
}

// ── Logger Step 3: File Provider (Karambolo Package) ────────────────────────────────────────────
builder.Logging.AddFile(o => o.RootPath = builder.Environment.ContentRootPath);

// ── Logger Step 4: HTTP Request/Response logging ────────────────────────────────────────────
builder.Services.AddHttpLogging(logging =>
{
    //Common fields for both environments
    logging.LoggingFields = HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.RequestQuery
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;

    logging.RequestHeaders.Add("X-Correlation-Id");
    logging.RequestHeaders.Add("X-Request-Id");

    logging.CombineLogs = true;

    if (builder.Environment.IsProduction())
    {
        //Production — never log body (sensitive customer data)
        logging.RequestBodyLogLimit = 0;
        logging.ResponseBodyLogLimit = 0;
    }
    else
    {
        //Developement/Staging — log body for debugging
        logging.LoggingFields |= HttpLoggingFields.RequestBody
            | HttpLoggingFields.ResponseBody;

        logging.RequestBodyLogLimit = 4096;   // 4 KB
        logging.ResponseBodyLogLimit = 4096;  // 4 KB
    }
});


// ------------------------------
// Dependency Injection (Filters)
// ------------------------------
builder.Services.AddScoped<ApiKeyFilter>();
builder.Services.AddScoped<RequestLoggingFilter>();



// ------------------------------
// Controllers + FluentValidation + Filters + snake_case
// ------------------------------
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiKeyFilter>();
    options.Filters.Add<RequestLoggingFilter>();
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy(); // For snake_case, we need a custom policy
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never; //return property with null value
    });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
builder.Services.AddHttpContextAccessor(); // required for IHttpContextAccessor


// ------------------------------
// Dependency Injection (Services)
// ------------------------------
// Infrastructure ────────────────────────────────────────────
builder.Services.AddSingleton<IDbConnectionFactory, SqlServerConnectionFactory>(); //Singleton BD Connection
builder.Services.AddSingleton<IApiResponseFactory, ApiResponseFactory>();

// Utilities / Low-Level Services ───────────────────────────
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IResetTokenHasher, ResetTokenHasher>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();

// Repositories ──────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPendingUserRepository, PendingUserRepository>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

// Application / Domain Services ────────────────────────────
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

// Misc ──────────────────────────────────────────────────────
builder.Services.AddCustomModelValidationResponse();


// ------------------------------
// Kestrel Configuration to listen port no 5000 and 5001
// ------------------------------
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000); // HTTP
    options.ListenLocalhost(5001, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS
    });
});


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


// ------------------------------
// Build app
// ------------------------------
WebApplication app = builder.Build();
Console.WriteLine($"Environment Name: {app.Environment.EnvironmentName}");

// ── Logger Step 5: Register req/res Middleware ────────────────────────────────────────────
app.UseHttpLogging(); // Built in Middleware to capture req/response logs
app.UseConsoleRequestLogger(); // Custom Middleware, not required already handled by app.UseHttpLogging();

app.UseGlobalExceptionMiddleware(); //Custom Middleware

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
    var aboutMe = new AboutMeDto()
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


// ── Logger Step 6: Application Startup Log Entry ─────────────────────────────────────────────────
app.Lifetime.ApplicationStarted.Register(() =>
{
    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Program>();

    logger.LogInformation("Application started. Environment: {Environment} | Time: {Time}", app.Environment.EnvironmentName, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));
});


app.Run();