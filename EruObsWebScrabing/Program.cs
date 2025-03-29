using EruObsWebScrabing.Controllers;
using EruObsWebScrabing.IServices;
using EruObsWebScrabing.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<HomeController>(client =>
{
    client.BaseAddress = new Uri("https://obisis2.erciyes.edu.tr/");
});
builder.Services.AddHttpClient();

 
builder.Services.AddScoped<IObsLoginService,ObsLoginService>();
builder.Services.AddScoped<IPreProcessingOCR,PreProcessingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run();
