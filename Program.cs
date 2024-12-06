using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Kiota.Abstractions.Authentication;
using outofoffice.Graph;
using Quartz;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using outofoffice.App_code;
using outofoffice.Repositories.Implementations;
using outofoffice.Repositories.Interfaces;
using outofoffice.Services;
using outofoffice.Models;
using outofoffice.Servicess;
using Hangfire;
using outofoffice.Helper;
using outofoffice.Jobs;
using Hangfire.SqlServer;
using outofoffice.Service;

#region Builder Setup
var builder = WebApplication.CreateBuilder(args);
#endregion

#region Authentication and Microsoft Identity
builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        options.Prompt = "select_account";

        options.Events.OnTokenValidated = async context =>
        {
            var tokenAcquisition = context.HttpContext.RequestServices
                .GetRequiredService<ITokenAcquisition>();

            var graphClient = new GraphServiceClient(
                new BaseBearerTokenAuthenticationProvider(
                    new TokenAcquisitionTokenProvider(
                        tokenAcquisition,
                        GraphConstants.Scopes,
                        context.Principal)));

            var user = await graphClient.Me
                .GetAsync(config =>
                {
                    config.QueryParameters.Select = new[] { "displayName", "mail", "mailboxSettings", "userPrincipalName", "id" };
                });

            context.Principal?.AddUserGraphInfo(user);
        };

        options.Events.OnAuthenticationFailed = context =>
        {
            var error = WebUtility.UrlEncode(context.Exception.Message);
            context.Response.Redirect($"/Home/ErrorWithMessage?message=Authentication+error&debug={error}");
            context.HandleResponse();
            return Task.CompletedTask;
        };

        options.Events.OnRemoteFailure = context =>
        {
            if (context.Failure is OpenIdConnectProtocolException)
            {
                var error = WebUtility.UrlEncode(context.Failure.Message);
                context.Response.Redirect($"/Home/ErrorWithMessage?message=Sign+in+error&debug={error}");
                context.HandleResponse();
            }
            return Task.CompletedTask;
        };
    })
    .EnableTokenAcquisitionToCallDownstreamApi(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
    }, GraphConstants.Scopes)
    .AddMicrosoftGraph(options =>
    {
        options.Scopes = GraphConstants.Scopes;
    })
    .AddInMemoryTokenCaches();
#endregion

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Set the maximum allowed content length (e.g., 1 GB)
    serverOptions.Limits.MaxRequestBodySize = 1073741824; // 1 GB in bytes

    // Set the maximum query string length (e.g., 1 MB)
});

#region Service Configuration
builder.Services.AddHttpClient("NoCertValidation")
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true; // Use with caution
        return handler;
    });

builder.Services.AddScoped<HangfireJobService>();
builder.Services.AddScoped<TeamsServices>();
builder.Services.AddScoped<GetTeamsToken>();
builder.Services.AddScoped<LdapService>();

builder.Services.AddDbContext<OOODbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("HangFireConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    })
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseFilter(new AutomaticRetryAttribute { Attempts = 0 })
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseColouredConsoleLogProvider();
});

builder.Services.AddHangfireServer();
#endregion

#region Session and HTTP Client Configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddScoped<TeamTokenResponse, TeamTokenResponse>();

builder.Services.AddScoped<IHangfireJobHandler, HangfireJobHandler>();

builder.Services.AddHttpClient<HistoryValues>();
builder.Services.AddHttpClient<IHistoryValues, HistoryValues>();
builder.Services.Configure<ZohoFSMUserDetails>(builder.Configuration.GetSection("ZohoFSMUserDetails"));
builder.Services.AddHttpClient<AuthService>();
builder.Services.AddHttpClient<IExchangeClient, ExchangeClient>();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
#endregion

#region Custom Service Registration
builder.Services.AddScoped<IZohoTimeOffService, ZohoTimeOffService>();
builder.Services.AddScoped<ISlackService, SlackService>();
builder.Services.AddScoped<IAppSettingsManager, AppSettingsManager>();
builder.Services.AddScoped<TeamTokenResponse>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IMessageAppListRepository, MessageAppListRepository>();
builder.Services.AddScoped<IUserAppMessageRepository, UserAppMessageRepository>();
builder.Services.AddAutoMapper(typeof(MappingProfile));
#endregion

#region Middleware Configuration
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Optional: Configure Swagger for development
    // app.UseSwagger();
    // app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "Out of Office API V1"); });
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard("/schedule");

// Map controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=UserLogin}/{action=Login}/{id?}");

app.Run();
#endregion
