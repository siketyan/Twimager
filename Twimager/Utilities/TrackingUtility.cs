using System.Threading.Tasks;
using Twimager.Objects;

namespace Twimager.Utilities
{
    public static class TrackingUtility
    {
        private static App App { get => App.GetCurrent(); }
        private static Logger Logger { get => App.Logger; }
        private static Config Config { get => App.Config; }

        public static async Task ResetAsync(this ITracking tracking)
        {
            await Logger.LogAsync($"Resetting: {tracking.ToString()}");

            tracking.IsCompleted = false;
            tracking.Oldest = tracking.Latest = null;

            Config.Save();
            await Logger.LogAsync("Successfully reset the tracking");
        }
    }
}
