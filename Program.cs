using System.Threading;

namespace DAEmu
{
    internal class Program
    {
        public static LobbyServer LoginServer { get; private set; }
        public static GameServer GameServer { get; private set; }

        private static void Main()
        {
            LoginServer = new LobbyServer();
            LoginServer.Start(2610);

            GameServer = new GameServer();
            GameServer.Start(2615);


            Thread.CurrentThread.Join();
        }
    }
}