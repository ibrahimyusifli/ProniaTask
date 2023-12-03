using Microsoft.EntityFrameworkCore;
using ProniaAB202.DAL;
using ProniaAB202.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSession(options =>
{
    options.IdleTimeout=TimeSpan.FromSeconds(5);
});

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(
    opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
    );

    
builder.Services.AddSingleton<IHttpContextAccessor,HttpContextAccessor>();
builder.Services.AddScoped<LayoutService>();

var app = builder.Build();

app.UseSession();   
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
