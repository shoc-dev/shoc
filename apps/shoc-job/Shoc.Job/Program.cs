using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shoc.ApiCore;
using Shoc.ApiCore.Access;
using Shoc.ApiCore.Auth;
using Shoc.ApiCore.DataProtection;
using Shoc.ApiCore.Discovery;
using Shoc.ApiCore.ObjectAccess;
using Shoc.ApiCore.Quartz;
using Shoc.Core.Kubernetes;
using Shoc.Job.Allocator;
using Shoc.Job.Config;
using Shoc.Job.K8s;
using Shoc.Job.Services;

// start building web application
var builder = WebApplication.CreateBuilder(args);

// add environment variables
builder.Configuration.AddEnvironmentVariables();

// register services
builder.Services.AddDiscovery(builder.Configuration);
builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddSelf(builder.Configuration);
builder.Services.AddPersistenceDataProtection();
builder.Services.AddDefaultAuthenticationMiddleware(JwtBearerDefaults.AuthenticationScheme).AddJwtAuthentication(builder.Configuration);
builder.Services.AddAccessAuthorization();
builder.Services.AddAuthenticationClient(builder.Configuration);
builder.Services.AddGrpcClients();
builder.Services.AddObjectAccessEssentials();
builder.Services.AddLogging();
builder.Services.AddQuartzEssentials(builder.Configuration);
builder.Services.AddSingleton<ResourceParser>();
builder.Services.AddSingleton<JobResourceFormatter>();
builder.Services.AddSingleton<JobResourceAllocatorFactory>();
builder.Services.AddSingleton<LabelValidationService>();
builder.Services.AddSingleton<GitRepoValidationService>();
builder.Services.AddSingleton<LabelService>();
builder.Services.AddSingleton<GitRepoService>();
builder.Services.AddSingleton<WorkspaceLabelService>();
builder.Services.AddSingleton<WorkspaceGitRepoService>();
builder.Services.AddSingleton<JobClusterResolver>();
builder.Services.AddSingleton<JobPackageResolver>();
builder.Services.AddSingleton<JobSecretResolver>();
builder.Services.AddSingleton<JobProtectionProvider>();
builder.Services.AddSingleton<JobValidationService>();
builder.Services.AddSingleton<JobSubmissionService>();
builder.Services.AddSingleton<JobService>();
builder.Services.AddSingleton<JobTaskService>();
builder.Services.AddSingleton<WorkspaceJobSubmissionService>();
builder.Services.AddSingleton<WorkspaceJobService>();
builder.Services.AddSingleton<WorkspaceJobTaskService>();
builder.Services.AddSingleton<KubernetesTaskClientFactory>();
builder.Services.AddSingleton<LogStreamingService>();
builder.Services.AddAnyOriginCors(ApiDefaults.DEFAULT_CORS);
builder.Services.AddQuartzHosting(builder.Configuration);
builder.Services.AddControllers();

// build the application
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseRouting();
app.UseAuthentication();
app.UseAccessEnrichment();
app.UseAuthorization();
app.MapControllers();
app.UseCors(ApiDefaults.DEFAULT_CORS);
app.Run();