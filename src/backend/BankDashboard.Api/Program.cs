using BankDashboard.Application;
using BankDashboard.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging();
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionFeature?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        if (exception is ValidationException validationException)
        {
            logger.LogWarning(
                validationException,
                "Request validation failed while processing {Path}",
                context.Request.Path);

            var validationProblemDetails = new ValidationProblemDetails(
                validationException.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray()))
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.Request.Path
            };
            validationProblemDetails.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(validationProblemDetails);
            return;
        }

        if (exception is InvalidDataException or JsonException)
        {
            logger.LogError(
                exception,
                "Configured demo data is invalid while processing {Path}",
                context.Request.Path);

            var dataProblemDetails = new ProblemDetails
            {
                Title = "Demo data is invalid.",
                Detail = app.Environment.IsDevelopment() ? exception.Message : "The configured data source could not be loaded.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = context.Request.Path
            };
            dataProblemDetails.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(dataProblemDetails);
            return;
        }

        logger.LogError(exception, "Unhandled exception while processing {Path}", context.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Title = "An unexpected error occurred.",
            Detail = app.Environment.IsDevelopment() ? exception?.Message : "Please try again later.",
            Status = StatusCodes.Status500InternalServerError,
            Instance = context.Request.Path
        };
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});
app.UseCors("FrontendDev");

app.MapControllers();

app.Run();
