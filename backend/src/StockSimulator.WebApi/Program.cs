using Microsoft.EntityFrameworkCore;
using StockSimulator.Infrastructure.BackgroundServices;
using StockSimulator.Infrastructure.Data;
using StockSimulator.Infrastructure.Messaging;
using StockSimulator.Infrastructure.Services;
using StockSimulator.Infrastructure.Services.IServices;
using StockSimulator.Infrastructure.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<DbSeeder>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITraderService, TraderService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

builder.Services.AddSingleton<NatsPricePublisher>();
builder.Services.AddHostedService<StockPriceSimulatorWorker>();
builder.Services.AddHostedService<TradeAuditWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
