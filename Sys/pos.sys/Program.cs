using pos.sys.Models;
using pos.sys.Repositories;
using AspNetCore.ServiceRegistration.Dynamic;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using pos.sys.Common;
using NLog;
using NLog.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureHostConfiguration(o => o.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
    optional: false, reloadOnChange: true));

ConfigurationManager configuration = builder.Configuration;
builder.Services.Configure<AppSettings>(configuration);

builder.Services.AddControllers().AddJsonOptions(option => option.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SMS", Version = "v2" });
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddServicesOfType<IScopedService>();
builder.Services.AddServicesWithAttributeOfType<ScopedServiceAttribute>();

//builder.Services.AddDbContext<ApplicationDBContext>(option => option. configuration.GetConnectionString("MainConnection"));
builder.Services.AddDbContext<ApplicationDBContext>(options => options.UseSqlServer(configuration.GetConnectionString("MainConnection")));
//builder.Services.AddDbContext<ConfigDBContext>(options => options.UseSqlServer(configuration.GetConnectionString("ConfigConnection")));
//builder.Services.AddDbContext<ConfigDBContext>(option => option.UseOracle(configuration.GetConnectionString("ConfigConnection"), b =>
//            b.UseOracleSQLCompatibility("11")));

LogManager.Setup()
              .RegisterNLogWeb(configuration)
              .LoadConfigurationFromFile("NLog.config")
              .GetCurrentClassLogger();

builder.WebHost.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Error);
}).UseNLog();

SecurityKey key = new X509SecurityKey(Security.GetCertificateFromStore(configuration.GetSection("JwtConfig").GetSection("thumbprint").Value));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(option =>
    {
        option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            //IssuerSigningKey = key,
            IssuerSigningKey = new SymmetricSecurityKey
        (Encoding.UTF8.GetBytes(configuration.GetSection("JwtConfig").GetSection("Key").Value)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration.GetSection("JwtConfig").GetSection("Issuer").Value,
            ValidAudience = configuration.GetSection("JwtConfig").GetSection("AudienceId").Value,
            ClockSkew = TimeSpan.Zero
        };
    });



var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
