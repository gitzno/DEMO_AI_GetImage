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
    options.AddPolicy("AllowVue", policy =>
    {
        // Cho phép frontend từ cổng 5173 truy cập
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
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

app.UseCors("AllowVue");
app.MapControllers();

app.Run();
