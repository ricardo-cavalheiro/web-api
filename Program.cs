using Blog.Data;
using Blog.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore;

namespace Blog;

public static class Program {
  public static void Main(string[] args) {
    var builder = WebApplication.CreateBuilder(args);

    ConfigureAuthentication(builder);
    ConfigureMvc(builder);
    ConfigureServices(builder);

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    LoadConfiguration(app);

    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseResponseCompression();
    app.MapControllers();

    if (app.Environment.IsDevelopment()) {
      Console.WriteLine("app in development");
      app.UseHttpsRedirection();
      app.UseSwagger();
    }

    app.Run();
  }

  private static void LoadConfiguration(WebApplication app) {
    Configuration.JwtKey = app.Configuration.GetValue<string>("JwtKey");
    Configuration.ApiKeyName = app.Configuration.GetValue<string>("ApiKeyName");
    Configuration.ApiKey = app.Configuration.GetValue<string>("ApiKey");

    var smtp = new SmptConfiguration();
    app.Configuration.GetSection("Smtp").Bind(smtp);
    Configuration.Smtp = smtp;
  }

  private static void ConfigureAuthentication(WebApplicationBuilder builder) {
    var key = Encoding.ASCII.GetBytes(Configuration.JwtKey);

    builder
      .Services
      .AddAuthentication(options => {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(key),
          ValidateIssuer = false,
          ValidateAudience = false
        };
      });
  }

  private static void ConfigureMvc(WebApplicationBuilder builder) {
    builder
     .Services
     .AddControllers()
     .ConfigureApiBehaviorOptions(options => {
       options.SuppressModelStateInvalidFilter = true;
     })
     .AddJsonOptions(options => {
       options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
       options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
     });

    builder
      .Services
      .AddResponseCompression(options => {
        options.Providers.Add<GzipCompressionProvider>();
      });

    builder
      .Services
      .Configure<GzipCompressionProviderOptions>(options => {
        options.Level = CompressionLevel.Optimal;
      });

    builder
      .Services
      .AddMemoryCache();
  }

  private static void ConfigureServices(WebApplicationBuilder builder) {
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    builder
      .Services
      .AddDbContext<BlogDataContext>(options => {
        options.UseSqlServer(connectionString);
      });

    builder
      .Services
      .AddTransient<TokenService>();

    builder
      .Services
      .AddTransient<EmailService>();
  }
}