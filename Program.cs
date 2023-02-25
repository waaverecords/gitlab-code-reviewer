using CustomNGitLab;
using Newtonsoft.Json.Linq;
using NGitLab.Models;

var builder = WebApplication.CreateBuilder(args);
var gitLabSettings = builder.Configuration.GetSection("GitLab").Get<GitLabSettings>();

var app = builder.Build();

app.MapGet("/", () => "gitlab-code-reviewer is running!");

app.MapPost("/gitlab-webhook", async context =>
{
    if (!context.Request.Headers.TryGetValue("X-Gitlab-Token", out var gitlabToken)
        || gitlabToken != gitLabSettings.WebhookToken)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return;
    }

    var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
    var data = JObject.Parse(requestBody) as dynamic;

    if (data.event_type != "merge_request")
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        return;
    }
    
    var projectId = (int)data.project.id;
    var mergeRequestId = (int)data.object_attributes.id;
    
    var gitlab = new CustomGitLabClient(gitLabSettings.Url, gitLabSettings.ImpersonationToken);

    var mrClient = gitlab.GetMergeRequest(projectId);
    var mrChangeClient = mrClient.Changes(mergeRequestId);
    var mrVersionClient = mrClient.Versions(mergeRequestId);
    var mrDiscussionClient = mrClient.Discussions(mergeRequestId);

    var versions = mrVersionClient.All();
    var changes = mrChangeClient.MergeRequestChange.Changes;
    
    var discussion = mrDiscussionClient.Add(new MergeRequestDiscussionCreate
    {
        Body = "test"
    });

    context.Response.StatusCode = StatusCodes.Status200OK;
});

app.Run();