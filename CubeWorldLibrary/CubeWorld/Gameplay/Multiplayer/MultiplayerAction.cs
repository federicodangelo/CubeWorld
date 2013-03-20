using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeWorld.Gameplay.Multiplayer
{
    public class MultiplayerAction
    {
        public enum Action
        {
            INITIAL_DATA,
            AVATAR_CREATE,
            AVATAR_DESTROY,
            AVATAR_MOVE,
            ITEM_CREATE,
            TILE_CREATE,
            TILE_CLICKED,
            TILE_HIT,
            TILE_INVALIDATED,

            LEN
        }

        public Action action;
        public byte[] extraData;

        private string[] extraDataString;

        public MultiplayerAction(Action action, byte[] extraData)
        {
            this.action = action;
            this.extraData = extraData;
        }

        public MultiplayerAction(Action action, string[] parameters)
        {
            this.action = action;
            extraData = Encoding.UTF8.GetBytes(String.Join(",", parameters));
        }

        public String GetParameter(int n)
        {
            if (extraDataString == null)
            {
                String s = Encoding.UTF8.GetString(extraData);
                extraDataString = s.Split(',');
            }

            return extraDataString[n];
        }
    }
}
