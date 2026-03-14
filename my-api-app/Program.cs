using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using my_api_app.Data;
using my_api_app.DTOs;
using my_api_app.Filters.Validations;
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


// ------------------------------
// Dependency Injection (Filters)
// ------------------------------
builder.Services.AddScoped<ModelValidationFilter>();



// ------------------------------
// Controllers + FluentValidation + Filters + snake_case
// ------------------------------
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ModelValidationFilter>();
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

app.UseConsoleRequestLogger(); // Custom Middleware
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

app.Run();




