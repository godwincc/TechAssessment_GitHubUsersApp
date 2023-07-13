using GitHubUsersAPI.Interfaces;
using GitHubUsersAPI.Models;
using GitHubUsersAPI.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;
using System.Diagnostics;
using WatchDog;
using WatchDog.src.Enums;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IGitHubUserService, GitHubUserService>();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//just some test configs for watchdog
builder.Services.AddWatchDogServices(settings => { 
    settings.IsAutoClear = true;
    settings.ClearTimeSchedule = WatchDogAutoClearScheduleEnum.Quarterly;
});

//inject the client API
builder.Services.AddHttpClient("GitHubAPI", client =>
{
    client.DefaultRequestHeaders.AcceptLanguage.Clear();
    client.BaseAddress = new Uri("https://api.github.com/");
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
    opt.WatchPageUsername = "admin";
    opt.WatchPagePassword = "admin";
});


app.Run();
