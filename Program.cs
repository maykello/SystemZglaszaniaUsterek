using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Mapster;
using SystemZglaszaniaUsterek.Data;
using SystemZglaszaniaUsterek.Models.Entities;
using SystemZglaszaniaUsterek.Models.Options;
using SystemZglaszaniaUsterek.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<SystemZglaszaniaUsterekDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";

        options.ExpireTimeSpan = TimeSpan.FromMinutes(7);
        options.SlidingExpiration = true;

        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

builder.Services.AddMemoryCache();

// Cloudinary configuration
builder.Services.Configure<CloudinaryOptions>(
    builder.Configuration.GetSection(CloudinaryOptions.SectionName));

// SMTP configuration
builder.Services.Configure<SmtpOptions>(
    builder.Configuration.GetSection(SmtpOptions.SectionName));

// Multipart limits: max 5 files * 10 MB + bufor na same metadane formularza
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 60L * 1024L * 1024L; // 60 MB
    options.ValueLengthLimit = 60 * 1024 * 1024;
    options.MultipartHeadersLengthLimit = 32 * 1024;
});

// Domain services
builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IAttachmentValidator, AttachmentValidator>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();

// Mapster - skanuje assembly i rejestruje wszystkie IRegister
TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<SystemZglaszaniaUsterekDbContext>();
    DbSeeder.SeedAll(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
