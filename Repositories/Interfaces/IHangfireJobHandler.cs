using outofoffice.Models;

namespace outofoffice.Repositories.Interfaces
{
    public interface IHangfireJobHandler
    {
        Task DeleteSingleJob(Guid AppId, string AppName);
        Task<string> DeleteJob(Guid jobId);
        Task AddJobIds(Dictionary<string, string> jobIds , Guid AppId);
    }
}
