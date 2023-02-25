using System.Globalization;
using NGitLab;
using NGitLab.Impl;
using NGitLab.Models;

namespace CustomNGitLab;

public class CustomMergeRequestClient : MergeRequestClient
{
    protected readonly API _api;
    protected readonly string _projectPath;

    public CustomMergeRequestClient(API api, int projectId): base(api, projectId)
    {
        _api = api;
        _projectPath = Project.Url + "/" + projectId.ToString(CultureInfo.InvariantCulture);
    }

    public MergeRequestVersionClient Versions(int mergeRequestIid) => new MergeRequestVersionClient(_api, _projectPath, mergeRequestIid);
}