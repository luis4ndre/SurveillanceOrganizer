using Hangfire;
using Hangfire.MySql;
using SurveillanceOrganizer;
using SurveillanceOrganizer.CameraTypes;
using SurveillanceOrganizer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseStorage(new MySqlStorage(Environment.GetEnvironmentVariable("HangfireConn"), new MySqlStorageOptions())));

builder.Services.AddHangfireServer();

builder.Services.AddScoped<Xiaomi360>();
builder.Services.AddScoped<TpLinkC100>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new MyAuthorizationFilter() }
});

var rootPath = Environment.GetEnvironmentVariable("RootPath");

Console.WriteLine($"rootPath: {rootPath}");

var recurringJobManager = new RecurringJobManager();

var surveillanceCron = Environment.GetEnvironmentVariable("SurveillanceCron") ?? Cron.Never();

Console.WriteLine($"surveillanceCron: {surveillanceCron}");

recurringJobManager.AddOrUpdate("Surveillance", (SurveillanceService s) => s.StartAsync(rootPath, "Consolidated"), surveillanceCron);

app.UseHttpsRedirection();

app.Run();