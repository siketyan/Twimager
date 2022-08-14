using System.Threading.Tasks;
using Twimager.Objects;

namespace Twimager.Utilities
{
    public static class TrackingUtility
    {
        private static App App => App.GetCurrent();
        private static Logger Logger => App.Logger;
        private static Config Config => App.Config;

        public static async Task ResetAsync(this ITracking tracking)
        {
            await Logger.LogAsync($"Resetting: {tracking}");

            tracking.IsCompleted = false;
            tracking.Oldest = tracking.Latest = null;

            Config.Save();
            await Logger.LogAsync("Successfully reset the tracking");
        }
    }
}
