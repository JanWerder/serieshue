
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Dashboard;
using serieshue.Interfaces;
using serieshue.Models;
using Microsoft.EntityFrameworkCore;
using serieshue.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddEntityFrameworkNpgsql().AddDbContext<SeriesHueContext>(opt => opt.UseNpgsql(builder.Configuration["ConnectionStrings:DBConnection"]));
builder.Services.AddHangfire(config => config.UsePostgreSqlStorage(builder.Configuration["ConnectionStrings:DBConnection"]));
builder.Services.AddHangfireServer();

builder.Services.AddScoped<ITaskRunnerService, TaskRunnerService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseAuthorization();
app.MapRazorPages();
app.UseHangfireDashboard("/jobs", new DashboardOptions
{
    Authorization = new[] { new AlwaysOkAuthorizationFilter() }
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

RecurringJob.AddOrUpdate<ITaskRunnerService>("Update IMDB", x => x.RunIMDBUpdate(), Cron.Daily);

app.Run();

public class AlwaysOkAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}