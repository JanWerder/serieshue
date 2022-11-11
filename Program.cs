
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
    // app.UseHttpsRedirection();
    // app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseAuthorization();

app.MapRazorPages();

app.Use((context, next) =>
{
    var pathBase = context.Request.Headers["X-Forwarded-PathBase"];
    if ((String)pathBase != null)
        context.Request.PathBase = new PathString(pathBase);
    return next();
});


app.UseHangfireDashboard("/jobs");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    // endpoints.MapHangfireDashboard("/admin/jobs", new DashboardOptions()
    // {
    //     Authorization = new List<IDashboardAuthorizationFilter> { }
    // })
    // .RequireAuthorization(HangfirePolicyName);
});

RecurringJob.AddOrUpdate<ITaskRunnerService>("Update IMDB", x => x.RunIMDBUpdate(), Cron.Daily);

app.Run();
