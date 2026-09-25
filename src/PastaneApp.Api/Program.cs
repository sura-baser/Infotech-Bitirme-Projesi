using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PastaneApp.Api.Services;
using PastaneApp.Core.Entities;
using PastaneApp.Data;
using PastaneApp.Data.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddPersistenceServices(builder.Configuration);

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<PastaneApp.Data.AppDbContext>()
    .AddErrorDescriber<TurkishIdentityErrorDescriber>();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
{
    throw new InvalidOperationException("Jwt:Key ayarı en az 32 karakterlik bir değer olarak tanımlanmalıdır (ortam değişkeni: Jwt__Key).");
}

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<ITokenService, JwtTokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = JwtOptions.NameClaimType,
            RoleClaimType = JwtOptions.RoleClaimType
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
