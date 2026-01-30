using HetznerWhatsApp.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    options.ListenAnyIP(int.Parse(port));
});

builder.Services.AddControllers();

builder.Services.AddHttpClient<IHetznerService, HetznerService>();
builder.Services.AddSingleton<IWhatsAppService, TwilioWhatsAppService>();
builder.Services.AddScoped<ICommandHandler, CommandHandler>();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();
