using System.Text.Json.Serialization;
using CryptoWise.API.Authorization;
using CryptoWise.API.Helpers;
using CryptoWise.API.Middleware;
using CryptoWise.API.Services.Account;
using CryptoWise.API.Services.Email;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
{
    var services = builder.Services;
    var env = builder.Environment;
    
    services.AddHttpClient("MetaAuthClient", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["MetaAuth:Url"]);
        client.Timeout = new TimeSpan(0, 0, 30);
        client.DefaultRequestHeaders.Clear();
    });

    services.AddControllers().AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddDbContext<DataContext>();
    services.AddCors();
    services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    services.AddScoped<ErrorHandlingMiddleware>();
    services.AddScoped<JwtMiddleware>();

    services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
    services.Configure<MetaAuth>(builder.Configuration.GetSection("MetaAuth"));
    
    services.AddScoped<IJwtUtils, JwtUtils>();
    services.AddScoped<IAccountService, AccountService>();
    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<IMetaAuthAccountService, MetaAuthAccountService>();
}

var app = builder.Build();

// migrate any database changes on startup (includes initial db creation)
using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();    
    dataContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
{
    app.UseSwagger();
    app.UseSwaggerUI(x => x.SwaggerEndpoint("/swagger/v1/swagger.json", "CryptoWise API"));
    
    app.UseCors(x => x
        .SetIsOriginAllowed(origin => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
    
    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseMiddleware<JwtMiddleware>();
    
    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();
}

app.Run();