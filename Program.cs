using Newtonsoft.Json.Linq;
using NGitLab;
using System.Text.RegularExpressions;

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
    
    var gitlab = new GitLabClient(gitLabSettings.Url, gitLabSettings.ImpersonationToken);

    var mrClient = gitlab.GetMergeRequest(projectId);
    var mrChangeClient = mrClient.Changes(mergeRequestId);
    var mrDiscussionClient = mrClient.Discussions(mergeRequestId);

    var mrVersion = mrClient.GetVersionsAsync(mergeRequestId).First();
    var mrChanges = mrChangeClient.MergeRequestChange.Changes;

    foreach (var change in mrChanges)
    {
        Console.WriteLine(change.Diff);

        var hunkPattern = @"^@@ -\d+(?:,\d+)? \+(\d)(?:,(\d+))? @@";
        var matches = Regex.Matches(change.Diff, hunkPattern, RegexOptions.Multiline);
        // var last = matches.ToList().Last();
        // var modifiedStart = int.Parse(last.Groups[1].Value);
        // var modifiedLineCount = string.IsNullOrEmpty(last.Groups[2].Value) ? 1 : int.Parse(last.Groups[2].Value);

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("PRIVATE-TOKEN", gitLabSettings.ImpersonationToken);

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("text"), "position[position_type]");
        form.Add(new StringContent(mrVersion.BaseCommitSha), "position[base_sha]");
        form.Add(new StringContent(mrVersion.HeadCommitSha), "position[head_sha]");
        form.Add(new StringContent(mrVersion.StartCommitSha), "position[start_sha]");
        form.Add(new StringContent(change.NewPath), "position[new_path]");
        form.Add(new StringContent(change.OldPath), "position[old_path]");
        form.Add(new StringContent((1).ToString()), "position[new_line]");
        form.Add(new StringContent("test"), "body");

        // see https://docs.gitlab.com/ee/api/discussions.html#create-a-new-thread-in-the-merge-request-diff
        // for commenting on an old, new or unchanged line

        using var response = await httpClient.PostAsync(
            new Uri(new Uri(gitLabSettings.Url), $"api/v4/projects/{projectId}/merge_requests/{mergeRequestId}/discussions"),
            form
        );
    };

    context.Response.StatusCode = StatusCodes.Status200OK;
});

app.Run();