using NGitLab;
using NGitLab.Impl;

namespace CustomNGitLab;

public class CustomGitLabClient : GitLabClient
{
    protected readonly API _api;

    public CustomGitLabClient(string hostUrl, string apiToken): base(hostUrl, apiToken)
    {
        _api = new API(new GitLabCredentials(hostUrl, apiToken), RequestOptions.Default);
    }

    new public CustomMergeRequestClient GetMergeRequest(int projectId)
    {
        return new CustomMergeRequestClient(_api, projectId);
    }
}