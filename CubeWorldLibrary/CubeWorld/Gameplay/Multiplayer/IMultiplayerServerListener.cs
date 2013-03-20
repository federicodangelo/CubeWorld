using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace CubeWorld.Gameplay.Multiplayer
{
    public interface IMultiplayerServerListener
    {
        void ClientConnected(MultiplayerClient client);
        void ClientDisconnected(MultiplayerClient client);
    }
}
