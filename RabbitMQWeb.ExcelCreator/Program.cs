using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQWeb.ExcelCreator.Hubs;
using RabbitMQWeb.ExcelCreator.Models;
using RabbitMQWeb.ExcelCreator.Services;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);



//
builder.Services.AddSingleton(sp => new ConnectionFactory() { Uri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")), DispatchConsumersAsync = true });
builder.Services.AddSingleton<RabbitMQPublisher>();
builder.Services.AddSingleton<RabbitMQClientService>();

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<AppIdentityDbContext>();
//


builder.Services.AddControllersWithViews();

//
builder.Services.AddSignalR();
//

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();



//
app.UseAuthentication();

using (var scope=app.Services.CreateScope())
{
    var appDbContext = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    appDbContext.Database.Migrate();

    if (!appDbContext.Users.Any())
    {
      
    }
}
//




app.UseAuthorization();

//
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<MyHub>("/MyHub");
});
//


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
