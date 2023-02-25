using System.Globalization;
using NGitLab.Models;
using NGitLab.Impl;

namespace NGitLab;

/// <summary>
/// Retrieves versions of a specific Merge Request
/// </summary>
/// <see cref="https://docs.gitlab.com/ee/api/merge_requests.html#get-merge-request-diff-versions"/>
public class MergeRequestVersionClient : IMergeRequestVersionClient
{
    private readonly API _api;
    private readonly string _versionsPath;

    public MergeRequestVersionClient(API api, string projectPath, int mergeRequestIid)
    {
        _api = api;
        _versionsPath = projectPath + "/merge_requests/" + mergeRequestIid.ToString(CultureInfo.InvariantCulture) + "/versions";
    }

    public IEnumerable<MergeRequestVersion> All()
    {
        return _api.Get().To<IEnumerable<MergeRequestVersion>>(_versionsPath);
    }
}