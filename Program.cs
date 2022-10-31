
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Dashboard;
using serieshue.Interfaces;
using serieshue.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddEntityFrameworkNpgsql().AddDbContext<SeriesHueContext>(opt => opt.UseNpgsql(builder.Configuration["ConnectionStrings:DBConnection"]));
builder.Services.AddHangfire(config => config.UsePostgreSqlStorage(builder.Configuration["ConnectionStrings:DBConnection"]));
builder.Services.AddHangfireServer();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.UseHangfireDashboard("/jobs");

RecurringJob.AddOrUpdate<ITaskRunnerService>("Update IMDB", x => x.RunIMDBUpdate(), Cron.Daily);

app.Run();
