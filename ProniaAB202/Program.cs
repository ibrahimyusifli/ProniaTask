using Microsoft.EntityFrameworkCore;
using ProniaAB202.DAL;
using ProniaAB202.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(
    opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
    );
builder.Services.AddScoped<LayoutService>();

var app = builder.Build();

app.UseStaticFiles();
app.MapControllerRoute(
    "area",
    "{area:exists}/{controller=home}/{action=index}/{id?}"


    );
app.MapControllerRoute(
    "default",
    "{controller=home}/{action=index}/{id?}"
    
    
    );

app.Run();
