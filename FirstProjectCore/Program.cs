using FirstProjectCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("FirstProjectCoreContextConnection") ?? throw new InvalidOperationException("Connection string 'FirstProjectCoreContextConnection' not found.");
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IPieRepository, PieRepository>();
builder.Services.AddDbContext<MyDbContext>(options => {
    options.UseSqlServer(
        builder.Configuration["ConnectionStrings:MyDBContextConnection"]);
});
builder.Services.AddDefaultIdentity<IdentityUser>
    ().//(options =>options.SignIn.RequireConfirmedAccount = true).
    AddEntityFrameworkStores<MyDbContext>(); 
var app = builder.Build();

app.UseStaticFiles();
//app.UseAuthentication();
if(app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

//app.UseRouting();

//app.MapGet("/", () => "Hello World!");
//app.MapDefaultControllerRoute();
app.MapControllerRoute(name:"default",pattern:"{controller=Home}/{action=Index}/{id?}");//conventional based routing
app.UseAuthentication();;
app.Run();
