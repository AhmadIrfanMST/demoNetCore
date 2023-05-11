using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;
using WebApplication3.Authentication;
using WebApplication3.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IPieRepository, PieRepository>();
builder.Services.AddDbContext<MyDbContext>(options => {
    options.UseSqlServer(
        builder.Configuration["ConnectionStrings:MyDBContextConnection"]);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



//here
// For Identity  
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<MyDbContext>()
    .AddDefaultTokenProviders();



// Adding Authentication  
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})

// Adding Jwt Bearer  
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = "http://localhost:4200",//Configuration["JWT:ValidAudience"],
        ValidIssuer = "http://localhost:61955",//Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ByYM000OLlMQG6VVVp1OH7Xzyr7gHuw1qvUC5dcGt3SNM"))//Configuration["JWT:Secret"]))
    };
    /*options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = async (context) =>
        {
            Console.WriteLine("Printing in the delegate OnAuthFailed");
        },
        OnChallenge = async (context) =>
        {
            Console.WriteLine("Printing in the delegate OnChallenge");

            // this is a default method
            // the response statusCode and headers are set here
            context.HandleResponse();

            // AuthenticateFailure property contains 
            // the details about why the authentication has failed
            if (context.AuthenticateFailure != null)
            {
                context.Response.StatusCode = 401;

                // we can write our own custom response content here
                await context.HttpContext.Response.WriteAsync("Token Validation Has Failed. Request Access Denied");
            }
        }
    };*/

});

//Adding Authorization 
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminAssignRolePolicy", policy =>
        policy.RequireClaim(Permissions.AssignRole).RequireRole(UserRoles.Admin));
        //policy.RequireRole(UserRoles.Admin)
         //     .RequireClaim(Permissions.AssignRole)); ; ;

});

//here


var app = builder.Build();
app.UseStaticFiles();
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");//conventional based routing

//app.MapControllers();

//app.Run();





if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();



