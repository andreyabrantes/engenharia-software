using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BilheteriaVirtualBlazor;
using BilheteriaVirtualBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient da API — registado como Singleton para ser partilhado pelos serviços
var apiBaseAddress = new Uri("http://localhost:5047/");

builder.Services.AddSingleton(new HttpClient { BaseAddress = apiBaseAddress });

builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<EventoService>();
builder.Services.AddSingleton<EmailService>();

await builder.Build().RunAsync();
