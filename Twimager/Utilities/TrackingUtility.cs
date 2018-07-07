using Twimager.Objects;

namespace Twimager.Utilities
{
    public static class TrackingUtility
    {
        public static void Reset(this ITracking tracking)
        {
            tracking.IsCompleted = false;
            tracking.Oldest = tracking.Latest = null;
            App.GetCurrent().Config.Save();
        }
    }
}
