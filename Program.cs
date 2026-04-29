using ArlianTrans.Web.Data;
using ArlianTrans.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddInMemoryCollection(DotEnvLoader.Load(Path.Combine(builder.Environment.ContentRootPath, ".env")));
var sqlitePath = Path.Combine(builder.Environment.ContentRootPath, "database", "arlian_trans.db");
Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath)!);
var legacySqlitePath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "arliantrans.db");
if (!File.Exists(sqlitePath) && File.Exists(legacySqlitePath))
{
    File.Copy(legacySqlitePath, sqlitePath);
}

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={sqlitePath}"));
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "ArlianTrans.Session";
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<AdminAuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<DatabaseRefreshService>();

var app = builder.Build();

await DbInitializer.InitializeAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.Use(async (context, next) =>
{
    if (HttpMethods.IsGet(context.Request.Method) && !Path.HasExtension(context.Request.Path))
    {
        context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
        context.Response.Headers.Pragma = "no-cache";
        context.Response.Headers.Expires = "0";

        using var scope = context.RequestServices.CreateScope();
        var refreshService = scope.ServiceProvider.GetRequiredService<DatabaseRefreshService>();
        await refreshService.SyncManualDatabaseChangesAsync();
    }

    await next();
});
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
