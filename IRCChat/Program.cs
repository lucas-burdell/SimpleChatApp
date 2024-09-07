using SadConsole;
using SadConsole.Configuration;

namespace SimpleChat.UI
{
    public class Program
    {
        static void Main(string[] args)
        {
            Settings.WindowTitle = "IRC Chat";

            Builder startup = new Builder()
                .SetScreenSize(120, 30)
                .OnStart(Started)
                .ConfigureFonts(true);

            Game.Create(startup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        private static void Started(object? sender, GameHost e)
        {
            Game.Instance.Screen = new RootScreen(120, 30);
        }
    }
}
