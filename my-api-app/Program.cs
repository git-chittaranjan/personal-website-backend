using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using my_api_app.Data;
using my_api_app.Helpers;
using my_api_app.Middlewares;
using my_api_app.Repositories.Implementations;
using my_api_app.Repositories.Interfaces;
using my_api_app.Responses;
using my_api_app.Services.Auth;
using my_api_app.Services.Security.Implementations;
using my_api_app.Services.Security.Interfaces;
using my_api_app.Validators.Auth.Register;
using System.Text;
using System.Text.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

// ------------------------------
// Controllers + FluentValidation
// ------------------------------
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

// ------------------------------
// Dependency Injection (Services)
// ------------------------------
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IDbConnectionFactory, SqlServerConnectionFactory>(); //Singleton BD Connection
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPendingUserRepository, PendingUserRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IResetTokenHasher, ResetTokenHasher>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();

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
// Convert response to snae_case
// ------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy(); // For snake_case, we need a custom policy
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never; //return property with null value
    });


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
              .WithMethods("GET","POST")
              .AllowAnyHeader());
});


// ------------------------------
// Build app
// ------------------------------
WebApplication app = builder.Build();

Console.WriteLine($"Environment Name: {app.Environment.EnvironmentName}");
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();  // Detailed errors only in dev
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowChittaranjanApp");
app.MapControllers();

app.Run();


