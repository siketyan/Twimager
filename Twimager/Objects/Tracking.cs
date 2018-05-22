using CoreTweet;
using CoreTweet.Core;
using System.Threading.Tasks;

namespace Twimager.Objects
{
    public interface ITracking
    {
        long? Latest { get; set; }

        Task<ListedResponse<Status>> GetStatusesAsync();
    }
}
