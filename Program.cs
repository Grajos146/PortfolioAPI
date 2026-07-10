using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using PortfolioAPI.Data;
using PortfolioAPI.Extensions;
using PortfolioAPI.Model.Services;
using PortfolioAPI.Validation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention()
);

builder.Services.AddScoped<EmailService>();

builder.Services.AddValidatorsFromAssemblyContaining<ContactMessageDtoValidator>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy => policy.WithOrigins("https://grajos-portfolio.netlify.app/").AllowAnyMethod().AllowAnyHeader()
    );
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(
        "ContactFormPolicy",
        opt =>
        {
            opt.PermitLimit = 1;
            opt.Window = TimeSpan.FromSeconds(20);
            opt.QueueProcessingOrder = System
                .Threading
                .RateLimiting
                .QueueProcessingOrder
                .OldestFirst;
            opt.QueueLimit = 0;
        }
    );
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseHttpsRedirection();
}

app.MapProjectEndpoints();
app.MapContactMessageEndpoints();

app.Run();
