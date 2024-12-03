using Microsoft.EntityFrameworkCore;
using VinylShop.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);
//dev2
var connString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<VinylShopContext>(options =>
    options.UseSqlServer(connString));

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
