using DEMOShoesSizeTag.Interfaces;
using DEMOShoesSizeTag.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Thêm vào trước builder.Build();
builder.Services.AddHttpClient<IGeminiVisionService, GeminiVisionService>();
// Thêm trước builder.Build()
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        // Cho phép tất cả các domain
        policy.AllowAnyOrigin()
              // Cho phép tất cả các phương thức (GET, POST, PUT, DELETE...)
              .AllowAnyMethod()
              // Cho phép tất cả các header
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("AllowAll");
app.MapControllers();

app.Run();
