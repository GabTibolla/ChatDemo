using SignalRChatDemo.ChatHub;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("Config\\config.json", false, true);

if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Database"))
{
    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Database");
}

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<ChatDemo.Business.Interfaces.IConfigService>(o => new ChatDemo.Business.Services.ConfigService(builder.Configuration));

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Login/Login";
        options.AccessDeniedPath = "/Home/Denied";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints => {
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chatHub");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");

app.Run();
