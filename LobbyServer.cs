using DAEmu.Networking;

namespace DAEmu
{
    public class LobbyClient :
        GameClient
    {
    }

    public class LobbyServer : Server<LobbyClient>
    {
        public override void ClientDataReceived(LobbyClient client, NetworkPacket packet)
        {
        }

        public override void OnClientDisconnected(LobbyClient client)
        {
        }

        public override void OnClientConnected(LobbyClient client)
        {
        }
    }
}