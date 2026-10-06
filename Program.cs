using Microsoft.EntityFrameworkCore;
using ShopApi.Data;
using ShopApi.Interfaces;
using ShopApi.Middlewares;
using ShopApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// OpenAPI / Swagger
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL
builder.Services.AddDbContext<AppDbContext>(
    options =>
        options.UseNpgsql(
            builder.Configuration
                .GetConnectionString("DefaultConnection")
        )
);

// Services
builder.Services.AddScoped<IImageService, CloudinaryImageService>();

builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<
    IProductService,
    ProductService
>();

// BURAYA KADAR bütün builder.Services kayıtları yapılmalı.
// Bundan sonra service eklemiyoruz.
var app = builder.Build();


// Global exception middleware
app.UseMiddleware<ExceptionMiddleware>();


// Swagger / OpenAPI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();

    app.UseSwaggerUI();
}


app.UseHttpsRedirection();


// Controller endpointlerini aktif eder.
app.MapControllers();


app.Run();