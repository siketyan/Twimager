using CoreTweet;
using CoreTweet.Core;
using System.Threading.Tasks;

namespace Twimager.Objects
{
    public interface ITracking
    {
        long? Latest { get; set; }
        string Directory { get; }

        Task<ListedResponse<Status>> GetStatusesAsync();
    }
}
