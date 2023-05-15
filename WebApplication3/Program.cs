using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;
using WebApplication3.Authentication;
using WebApplication3.Models;
using WebApplication3.Permission;
using WebApplication3.Seeds;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IPieRepository, PieRepository>();
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores<MyDbContext>()
.AddDefaultTokenProviders();
builder.Services.AddScoped<UserManager<IdentityUser>>();
builder.Services.AddSwaggerGen();

// Adding Authentication  
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ByYM000OLlMQG6VVVp1OH7Xzyr7gHuw1qvUC5dcGt3SNM"))
    };
});

//Adding Authorization 
builder.Services.AddAuthorization(options =>
{
    //below fallback is equivalentr to applying authorize on controller side or on an action
    //options.FallbackPolicy = new AuthorizationPolicyBuilder()// fallback is triggered when no authorize attribute is present. Authorize is not on controller nor on action
    //.RequireAuthenticatedUser()// it will check if there is an identity cookie present
    //.Build();
    options.AddPolicy("AdminAssignRolePolicy", policy =>
        policy.RequireClaim(Permissions.AssignRole).RequireRole(UserRoles.Admin));
});
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
//here

builder.Services.AddDbContext<MyDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration["ConnectionStrings:MyDBContextConnection"]);
});
var app = builder.Build();
app.UseStaticFiles();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("app");
    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await DefaultRoles.SeedAsync(userManager, roleManager);
        await DefaultUsers.SeedBasicUserAsync(userManager, roleManager);
        await DefaultUsers.SeedSuperAdminAsync(userManager, roleManager);
        logger.LogInformation("Finished Seeding Default Data");
        logger.LogInformation("Application Starting");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "An error occurred seeding the DB");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();

app.UseRouting();
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized) // 401
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(new ErrorHandle()
        {
            StatusCode = 401,
            Message = "Token is not valid"
        }.ToString());
    }
    else if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden) // 403
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(new ErrorHandle()
        {
            StatusCode = 403,
            Message = "You do not have any access for this resource, Please contact administrator!"
        }.ToString());
    }
});
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

