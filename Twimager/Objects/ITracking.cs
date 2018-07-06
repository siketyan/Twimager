using CoreTweet;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Twimager.Objects
{
    public interface ITracking
    {
        long? Latest { get; set; }
        string Directory { get; }

        Task UpdateSummaryAsync();
        Task<IEnumerable<Status>> GetStatusesAsync();

        void Reset();
    }
}
