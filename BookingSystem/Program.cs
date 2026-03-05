using BookingSystem.Data;
using BookingSystem.Entities;
using BookingSystem.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BookingSystem.Entities.Enums;
using BCrypt.Net;
using System;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = "THIS_IS_A_SUPER_SECRET_KEY_1234567890"; // Dev thôi nha, prod thì bỏ vào appsettings

// ================= DB =================
builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ================= Services =================
builder.Services.AddScoped<AuthService>();

// ================= JWT =================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ================= Swagger =================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token dạng: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
// ================= Controllers =================
builder.Services.AddControllers();

// ================= CORS =================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// ================= Seed Data =================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookingDbContext>();

    context.Database.EnsureCreated();

    if (!context.Users.Any())
    {
        var serviceProvider = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Super Admin",
            Email = "admin@example.com",
            PhoneNumber = "0999999999",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Lekhanh0402!Lekhanh0402!"),
            Role = UserRole.ServiceProvider,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(serviceProvider);
        context.SaveChanges();
    }
}


// ================= Middleware =================
app.UseHttpsRedirection();

app.UseCors("AllowReact");

app.UseAuthentication();   // 🔥 BẮT BUỘC PHẢI CÓ
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();