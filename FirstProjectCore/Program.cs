using FirstProjectCore.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IPieRepository, PieRepository>();
builder.Services.AddDbContext<MyDbContext>(options => {
    options.UseSqlServer(
        builder.Configuration["ConnectionStrings:MyDBContextConnection"]);
});
var app = builder.Build();

app.UseStaticFiles();
app.UseAuthentication();
if(app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

//app.UseRouting();

//app.MapGet("/", () => "Hello World!");
//app.MapDefaultControllerRoute();
app.MapControllerRoute(name:"default",pattern:"{controller=Home}/{action=Index}/{id?}");//conventional based routing
app.Run();
