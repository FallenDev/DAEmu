using DAEmu.Networking;

namespace DAEmu
{
    public class Client :
        GameClient
    {
    }

    public class GameServer : Server<Client>
    {
        public override void ClientDataReceived(Client client, NetworkPacket packet)
        {
        }

        public override void OnClientDisconnected(Client client)
        {
        }

        public override void OnClientConnected(Client client)
        {
        }
    }
}