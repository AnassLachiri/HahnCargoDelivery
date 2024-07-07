using HahnCargoDelivery.Configs;
using HahnCargoDelivery.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

// Add services to the container.

builder.Services.Configure<UserInfosConfig>(builder.Configuration.GetSection("UserInfos"));
builder.Services.Configure<HahnCargoSimApiConfig>(builder.Configuration.GetSection("HahnCargoSimApi"));

// Register HttpClient
builder.Services.AddHttpClient();

// Register your ExternalApiService
builder.Services.AddSingleton<IExternalApiService, ExternalApiService>();

// Add services
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IOrderService, OrderService>();
builder.Services.AddSingleton<ITransporterService, TransporterService>();
builder.Services.AddSingleton<IGridService, GridService>();
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<SimulationService>();
builder.Services.AddHostedService<SimulationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();