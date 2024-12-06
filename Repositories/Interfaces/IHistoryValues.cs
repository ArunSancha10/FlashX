using Newtonsoft.Json.Linq;
using outofoffice.Models;

namespace outofoffice.Repositories.Interfaces
{
    public interface IHistoryValues
    {
        Task<List<OutOfOfficeHistory>> GetHistoryValues(string apiUrl);

    }
}
