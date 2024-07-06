using HahnCargoDelivery.Configs;
using HahnCargoDelivery.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<UserInfosConfig>(builder.Configuration.GetSection("UserInfos"));
builder.Services.Configure<HahnCargoSimApiConfig>(builder.Configuration.GetSection("HahnCargoSimApi"));

// Register HttpClient
builder.Services.AddHttpClient();

// Register your ExternalApiService
builder.Services.AddScoped<IExternalApiService, ExternalApiService>();

// Add services
builder.Services.AddSingleton<IAuthService, AuthService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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