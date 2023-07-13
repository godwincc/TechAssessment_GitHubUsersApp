using GitHubUsersApp.API.v1.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;
using System.Diagnostics;
using WatchDog;
using WatchDog.src.Enums;
using Microsoft.Extensions.Options;
using GitHubUsersApp.API.v1.Services;
using GitHubUsersApp.API.v1.Interfaces;
using Microsoft.AspNetCore.Mvc.Versioning;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IGitHubUserService, GitHubUserService>();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1,0);
    options.UseApiBehavior = true;
});

//just some test configs for watchdog
builder.Services.AddWatchDogServices(settings => { 
    settings.IsAutoClear = builder.Configuration.GetValue<bool>("WatchDogConfig:EnableAutoClear"); 
    settings.ClearTimeSchedule = (WatchDogAutoClearScheduleEnum)builder.Configuration.GetValue<int>("WatchDogConfig:AutoClearSchedule"); 
});

//inject the client API
builder.Services.AddHttpClient("GitHubAPI", client =>
{
    client.DefaultRequestHeaders.AcceptLanguage.Clear();
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ExternalAPI:GitHubAPI"));
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "TranportCapital-GetGitHubUser-Test");

});

var app = builder.Build();

app.UseHttpLogging();

// catch and log some unexpectd errors here
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }

    catch (ArgumentException ex) {

        WatchLogger.LogError(ex.Message, Activity.Current?.Id, Environment.MachineName);

        app.Logger.LogError(
            ex,
            ex.Message);

        await Results.Problem(
              title: ex.Message,
              statusCode: StatusCodes.Status500InternalServerError
            ).ExecuteAsync(context);
    }

    catch (Exception ex)
    {
        WatchLogger.LogError(ex.Message, Activity.Current?.Id, Environment.MachineName);

        app.Logger.LogError(
            ex,
            "Error occured while trying to process request on machine {Machine}. TraceId: {TraceId}",
            Environment.MachineName, Activity.Current?.Id);

        await Results.Problem(
              title: "Error occured while trying to process request. Please try again.",
              statusCode: StatusCodes.Status500InternalServerError
            ).ExecuteAsync(context);
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseWatchDogExceptionLogger();

//change this to a better repo
app.UseWatchDog(opt =>
{
    opt.WatchPageUsername = app.Configuration.GetValue<string>("WatchDogConfig:Username");
    opt.WatchPagePassword = app.Configuration.GetValue<string>("WatchDogConfig:Password");
});


app.Run();
