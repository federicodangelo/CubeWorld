using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CubeWorld.Gameplay.Multiplayer
{
    public class MultiplayerProtocol
    {
        static public byte[] Serialize(MultiplayerAction action)
        {
            byte[] data = new byte[1 + action.extraData.Length];
            data[0] = (byte) action.action;
            Array.ConstrainedCopy(action.extraData, 0, data, 1, action.extraData.Length);
            return data;
        }

        static public MultiplayerAction Deserialize(byte[] data)
        {
            MultiplayerAction.Action action = (MultiplayerAction.Action) data[0];
            byte[] extraData = new byte[data.Length - 1];
            Array.ConstrainedCopy(data, 1, extraData, 0, extraData.Length);

            MultiplayerAction ma = new MultiplayerAction(action, extraData);

            return ma;
        }
    }
}
