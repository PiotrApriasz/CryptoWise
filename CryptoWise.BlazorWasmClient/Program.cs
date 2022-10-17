using CryptoWise.BlazorWasmClient;
using CryptoWise.BlazorWasmClient.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient());
builder.Services.AddMudServices();

builder.Services.AddScoped<IAccountService, AccountService>();

await builder.Build().RunAsync();
