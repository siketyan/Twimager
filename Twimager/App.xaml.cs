using CoreTweet;

namespace Twimager
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App
    {
        public Tokens Twitter { get; set; }

        public static App GetCurrent()
        {
            return (App)Current;
        }

        // TODO: Create Twitter tokens
    }
}
