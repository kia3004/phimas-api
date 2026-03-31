using System.Data.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using PHIMAS_PREDICTIVE_ANALYTICS.Data;
using PHIMAS_PREDICTIVE_ANALYTICS.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = ResolveConnectionString(builder.Configuration);
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("A MySQL connection string is required. Configure ConnectionStrings:DefaultConnection, MYSQLCONNSTR_DefaultConnection, DATABASE_URL, or Database/DB_* settings.");
}
var mySqlOptions = ResolveMySqlConnectionOptions(connectionString, builder.Environment);

bool? configuredSeedDemoData = builder.Configuration.GetValue<bool?>("AppStartup:SeedDemoData");
var shouldSeedDemoData = configuredSeedDemoData ?? builder.Environment.IsDevelopment();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/SignIn";
        options.AccessDeniedPath = "/Account/SignIn";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        mySqlOptions.ConnectionString,
        mySqlOptions.ServerVersion));

builder.Services.AddScoped<PredictiveAnalyticsService>();
builder.Services.AddScoped<AIAssistantService>();
builder.Services.AddScoped<FieldSubmissionService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.EnsureSchemaAsync(context);
    await SeedData.ValidateSchemaAsync(context);
    if (shouldSeedDemoData)
    {
        await SeedData.EnsureSeedDataAsync(context);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/healthz", async (AppDbContext dbContext) =>
{
    await dbContext.Database.ExecuteSqlRawAsync("SELECT 1;");
    return Results.Ok(new { status = "ok" });
});
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static string? ResolveConnectionString(IConfiguration configuration)
{
    return BuildConnectionStringFromDatabaseUrl(configuration["DATABASE_URL"])
        ?? configuration["MYSQLCONNSTR_DefaultConnection"]
        ?? BuildConnectionStringFromSettings(configuration)
        ?? configuration.GetConnectionString("DefaultConnection");
}

static (string ConnectionString, ServerVersion ServerVersion) ResolveMySqlConnectionOptions(
    string connectionString,
    IWebHostEnvironment environment)
{
    var normalizedConnectionString = NormalizeMySqlConnectionString(connectionString);

    try
    {
        return (normalizedConnectionString, ServerVersion.AutoDetect(normalizedConnectionString));
    }
    catch (Exception ex) when (environment.IsDevelopment() && IsSslNegotiationFailure(ex))
    {
        var localVerificationConnectionString = BuildLocalVerificationConnectionString(normalizedConnectionString);
        return (localVerificationConnectionString, ServerVersion.AutoDetect(localVerificationConnectionString));
    }
}

static string? BuildConnectionStringFromDatabaseUrl(string? databaseUrl)
{
    if (string.IsNullOrWhiteSpace(databaseUrl) ||
        !Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri) ||
        string.IsNullOrWhiteSpace(uri.Host) ||
        string.IsNullOrWhiteSpace(uri.UserInfo))
    {
        return null;
    }

    var userInfo = uri.UserInfo.Split(':', 2, StringSplitOptions.TrimEntries);
    if (userInfo.Length == 0 || string.IsNullOrWhiteSpace(userInfo[0]))
    {
        return null;
    }

    var sslMode = GetQueryValue(uri, "sslmode")
        ?? GetQueryValue(uri, "ssl-mode")
        ?? "Preferred";

    return BuildConnectionString(
        host: uri.Host,
        port: uri.IsDefaultPort ? 3306 : uri.Port,
        database: uri.AbsolutePath.Trim('/'),
        user: Uri.UnescapeDataString(userInfo[0]),
        password: userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
        sslMode: sslMode);
}

static string? BuildConnectionStringFromSettings(IConfiguration configuration)
{
    var host = configuration["Database:Host"] ?? configuration["DB_HOST"];
    var database = configuration["Database:Name"] ?? configuration["DB_NAME"];
    var user = configuration["Database:User"] ?? configuration["DB_USER"];
    var password = configuration["Database:Password"] ?? configuration["DB_PASSWORD"];

    if (string.IsNullOrWhiteSpace(host) ||
        string.IsNullOrWhiteSpace(database) ||
        string.IsNullOrWhiteSpace(user))
    {
        return null;
    }

    var portSetting = configuration["Database:Port"] ?? configuration["DB_PORT"];
    var sslMode = configuration["Database:SslMode"] ?? configuration["DB_SSL_MODE"] ?? "Preferred";

    return BuildConnectionString(
        host: host,
        port: int.TryParse(portSetting, out var port) ? port : 3306,
        database: database,
        user: user,
        password: password ?? string.Empty,
        sslMode: sslMode);
}

static string BuildConnectionString(
    string host,
    int port,
    string database,
    string user,
    string password,
    string sslMode)
{
    return $"server={host};port={port};database={database};user={user};password={password};SslMode={sslMode};AllowPublicKeyRetrieval=true;";
}

static string NormalizeMySqlConnectionString(string connectionString)
{
    var builder = new DbConnectionStringBuilder
    {
        ConnectionString = connectionString
    };

    if (!builder.TryGetValue("AllowPublicKeyRetrieval", out _))
    {
        builder["AllowPublicKeyRetrieval"] = "true";
    }

    if (!builder.TryGetValue("SslMode", out var sslModeValue) || string.IsNullOrWhiteSpace(Convert.ToString(sslModeValue)))
    {
        builder["SslMode"] = "Preferred";
    }

    return builder.ConnectionString;
}

static string BuildLocalVerificationConnectionString(string connectionString)
{
    var builder = new DbConnectionStringBuilder
    {
        ConnectionString = connectionString
    };

    builder["SslMode"] = "None";
    builder["AllowPublicKeyRetrieval"] = "true";

    return builder.ConnectionString;
}

static bool IsSslNegotiationFailure(Exception exception)
{
    for (var current = exception; current is not null; current = current.InnerException)
    {
        if (ContainsAny(
                current.Message,
                "ssl",
                "tls",
                "certificate",
                "public key retrieval",
                "secure connection"))
        {
            return true;
        }
    }

    return false;
}

static bool ContainsAny(string? text, params string[] fragments)
{
    if (string.IsNullOrWhiteSpace(text))
    {
        return false;
    }

    return fragments.Any(fragment => text.Contains(fragment, StringComparison.OrdinalIgnoreCase));
}

static string? GetQueryValue(Uri uri, string key)
{
    var query = uri.Query.TrimStart('?');
    if (string.IsNullOrWhiteSpace(query))
    {
        return null;
    }

    foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        var pieces = pair.Split('=', 2, StringSplitOptions.TrimEntries);
        if (!string.Equals(pieces[0], key, StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        return pieces.Length > 1 ? Uri.UnescapeDataString(pieces[1]) : string.Empty;
    }

    return null;
}
