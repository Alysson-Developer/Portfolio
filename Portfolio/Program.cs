using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Portfolio.Components;
using Portfolio.Data;
using Portfolio.Services;

var builder = WebApplication.CreateBuilder(args);

var storagePathSetting = builder.Configuration["Portfolio:StoragePath"] ?? ".";
var storageRoot = Path.GetFullPath(storagePathSetting, builder.Environment.ContentRootPath);
var uploadsPathSetting = builder.Configuration["Portfolio:UploadsPath"];
var uploadsRoot = Path.GetFullPath(
    uploadsPathSetting ?? Path.Combine(storageRoot, "uploads"),
    builder.Environment.ContentRootPath);
Directory.CreateDirectory(storageRoot);
Directory.CreateDirectory(uploadsRoot);

var configuredConnectionString = builder.Configuration.GetConnectionString("Portfolio");
var connectionString = string.IsNullOrWhiteSpace(configuredConnectionString)
    ? new SqliteConnectionStringBuilder { DataSource = Path.Combine(storageRoot, "portfolio.db") }.ToString()
    : configuredConnectionString;
var databasePath = new SqliteConnectionStringBuilder(connectionString).DataSource;
if (!string.IsNullOrWhiteSpace(databasePath) && databasePath != ":memory:")
    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(databasePath, builder.Environment.ContentRootPath))!);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddDbContext<PortfolioDbContext>(o =>
    o.UseSqlite(connectionString));
builder.Services.AddScoped<IPortfolioContentService, PortfolioContentService>();
builder.Services.AddSingleton<IPasswordHasher<string>, PasswordHasher<string>>();
builder.Services.AddSingleton(new PortfolioUploadStorage(uploadsRoot));
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
    o.ForwardLimit = 1;
    if (builder.Environment.IsProduction())
    {
        // Render's proxy addresses can change; the container is reached publicly through Render's proxy.
        o.KnownIPNetworks.Clear();
        o.KnownProxies.Clear();
    }
});
var configuredAdminPassword = builder.Configuration["Admin:Password"];
var adminPasswordHash = string.IsNullOrEmpty(configuredAdminPassword)
    ? null
    : new PasswordHasher<string>().HashPassword("admin", configuredAdminPassword);
builder.Services.AddSingleton(new AdminPasswordHash(adminPasswordHash ?? string.Empty));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
{
    o.LoginPath = "/admin/login";
    o.Cookie.Name = "Portfolio.Admin";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Strict;
    o.Cookie.SecurePolicy = builder.Environment.IsProduction()
        ? CookieSecurePolicy.Always
        : CookieSecurePolicy.SameAsRequest;
    o.SlidingExpiration = true;
    o.ExpireTimeSpan = TimeSpan.FromHours(8);
});
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(o => o.AddFixedWindowLimiter("admin-login", x =>
{
    x.PermitLimit = 5;
    x.Window = TimeSpan.FromMinutes(5);
    x.QueueLimit = 0;
}));
var app = builder.Build();
app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseWhen(context => !context.Request.Path.Equals("/health", StringComparison.OrdinalIgnoreCase),
    branch => branch.UseHttpsRedirection());
app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/uploads", out var remainder))
    {
        await next();
        return;
    }

    if (context.Request.Method is not ("GET" or "HEAD"))
    {
        context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
        return;
    }

    var parts = remainder.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [];
    var storage = context.RequestServices.GetRequiredService<PortfolioUploadStorage>();
    if (parts.Length != 2 || !storage.TryResolve(parts[0], parts[1], out var filePath, out var contentType))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    context.Response.ContentType = contentType;
    if (HttpMethods.IsHead(context.Request.Method))
    {
        context.Response.ContentLength = new FileInfo(filePath).Length;
        return;
    }

    await context.Response.SendFileAsync(filePath);
});
app.UseStaticFiles();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/admin") && !ctx.Request.Path.StartsWithSegments("/admin/login") &&
        !ctx.Request.Path.StartsWithSegments("/admin/session") && !(ctx.User.Identity?.IsAuthenticated ?? false))
    {
        ctx.Response.Redirect("/admin/login");
        return;
    }

    await next();
});
app.MapPost("/admin/session", async (HttpContext ctx, IServiceProvider services, IPasswordHasher<string> hasher) =>
{
    var storedHash = services.GetRequiredService<AdminPasswordHash>().Value;
    if (string.IsNullOrEmpty(storedHash))
        return Results.Problem("Configure Admin__Password no ambiente antes do login.", statusCode: 503);
    var form = await ctx.Request.ReadFormAsync();
    var candidate = form["password"].ToString();
    if (hasher.VerifyHashedPassword("admin", storedHash, candidate) == PasswordVerificationResult.Failed)
        return Results.Redirect("/admin/login?error=1");
    var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "admin")],
        CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    return Results.Redirect("/admin");
}).RequireRateLimiting("admin-login");
app.MapPost("/admin/logout", async (HttpContext c) =>
{
    await c.SignOutAsync();
    return Results.Redirect("/admin/login");
});
app.MapPost("/admin/upload/{kind}", async (string kind, HttpRequest request, PortfolioUploadStorage storage) =>
{
    var form = await request.ReadFormAsync();
    var file = form.Files.GetFile("file");
    if (file is null) return Results.BadRequest("Arquivo ausente.");
    var allowed = new Dictionary<string, string>
        { [".png"] = "image/png", [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg", [".webp"] = "image/webp" };
    if (!storage.TryGetDirectory(kind, out var dir) || file.Length is < 1 or > 5_242_880)
        return Results.BadRequest("Tipo ou tamanho de imagem inválido (máximo 5 MB).");
    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
    if (!allowed.TryGetValue(ext, out var mime) || file.ContentType != mime)
        return Results.BadRequest("Formato de imagem inválido.");
    var bytes = new byte[file.Length];
    await using (var input = file.OpenReadStream())
    {
        await input.ReadExactlyAsync(bytes);
    }

    var valid = ext == ".png"
        ? bytes.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })
        :
        ext == ".webp"
            ?
            System.Text.Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" &&
            System.Text.Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP"
            : bytes[0] == 255 && bytes[1] == 216 && bytes[^2] == 255 && bytes[^1] == 217;
    if (!valid) return Results.BadRequest("O conteúdo não corresponde a uma imagem válida.");
    var name = $"{Guid.NewGuid():N}{ext}";
    Directory.CreateDirectory(dir);
    await File.WriteAllBytesAsync(Path.Combine(dir, name), bytes);
    return Results.Ok(new { url = $"/uploads/{kind}/{name}" });
}).RequireAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<PortfolioDbContext>().Database.MigrateAsync();
}

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
