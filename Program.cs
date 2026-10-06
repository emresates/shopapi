using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
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

builder.Services.AddScoped<
    ITokenService,
    TokenService
>();

builder.Services.AddScoped<
    IAuthService,
    AuthService
>();

builder.Services.AddScoped<
    IFavoriteService,
    FavoriteService
>();

builder.Services.AddScoped<
    ICartService,
    CartService
>();

builder.Services.AddScoped<
    IAddressService,
    AddressService
>();

builder.Services.AddScoped<
    IAddressService,
    AddressService
>();

builder.Services.AddScoped<
    IOrderService,
    OrderService
>();

var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Jwt:Key bulunamadı."
    );

builder.Services
    .AddAuthentication(
        JwtBearerDefaults
            .AuthenticationScheme
    )
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey =
                    true,

                ValidIssuer =
                    builder.Configuration[
                        "Jwt:Issuer"
                    ],

                ValidAudience =
                    builder.Configuration[
                        "Jwt:Audience"
                    ],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtKey
                        )
                    ),

                ClockSkew =
                    TimeSpan.Zero
            };
    });

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT access token gir."
        }
    );

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "bearer",
                document
            )] = []
        }
    );
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

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