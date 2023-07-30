using apigee.sms.intf.Helper;
using apigee.sms.intf.Models;
using apigee.sms.intf.Utility;
using AspNetCore.ServiceRegistration.Dynamic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using NLog;
using NLog.Web;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureHostConfiguration(o => o.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
    optional: false, reloadOnChange: true));

ConfigurationManager configuration = builder.Configuration;
builder.Services.Configure<AppSettings>(configuration);
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    });
builder.Services.AddControllers()
    .AddNewtonsoftJson();
builder.Services.AddControllers().AddJsonOptions(option => option.JsonSerializerOptions.PropertyNamingPolicy = null);
//RedisConnection(builder);
//static void RedisConnection(WebApplicationBuilder? builder)
//{
//    builder.Services.AddSingleton<IConnectionMultiplexer>(opt => null);
//    //builder.Services.AddSingleton<IConnectionMultiplexer>(opt =>
//    //    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisCacheAWSConnection")));
//    //if (builder.Configuration["dependency"] == "Cache")
//    //{
//    //    try
//    //    {
//    //        builder.Services.AddSingleton<IConnectionMultiplexer>(opt =>
//    //       ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisCacheAWSConnection")));
//    //    }
//    //    catch (Exception ex)
//    //    {

//    //    }
//    //}
//    //else
//    //{
//    //    try
//    //    {
//    //        builder.Services.AddSingleton<IConnectionMultiplexer>(opt => null);
//    //    }
//    //    catch (Exception ex)
//    //    {

//    //    }
//    //}
//}
LogManager.Setup()
          .RegisterNLogWeb(configuration)
          .LoadConfigurationFromFile("NLog.config")
          .GetCurrentClassLogger();

builder.WebHost.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
}).UseNLog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddServicesOfType<IScopedService>();
builder.Services.AddServicesWithAttributeOfType<ScopedServiceAttribute>();

SecurityKey key = new X509SecurityKey(Common.GetCertificateFromStore(configuration.GetSection("JwtConfig").GetSection("thumbprint").Value));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(option =>
    {
        option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration.GetSection("JwtConfig").GetSection("Issuer").Value,
            ValidAudience = configuration.GetSection("JwtConfig").GetSection("AudienceId").Value,
            ClockSkew = TimeSpan.Zero
        };
    });
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
builder.Services.AddHttpClient();
builder.Services.AddServicesWithAttributeOfType<ScopedServiceAttribute>();
LogManager.Setup()
              .RegisterNLogWeb(configuration)
              .LoadConfigurationFromFile("NLog.config")
              .GetCurrentClassLogger();
builder.WebHost.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
}).UseNLog();
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.MapControllers();

app.Run();
