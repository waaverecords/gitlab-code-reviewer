using NGitLab.Models;

namespace NGitLab;

public interface IMergeRequestVersionClient
{
    IEnumerable<MergeRequestVersion> All();
}