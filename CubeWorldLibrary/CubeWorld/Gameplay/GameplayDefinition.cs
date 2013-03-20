using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeWorld.Gameplay
{
    public class GameplayDefinition
    {
        public string id;
        public string name;
        public string description;
        public BaseGameplay gameplay;
        public bool hasCustomGenerator;

        public GameplayDefinition(string name, string description, BaseGameplay gameplay, bool hasCustomGenerator)
        {
            this.id = gameplay.Id;
            this.name = name;
            this.description = description;
            this.gameplay = gameplay;
            this.hasCustomGenerator = hasCustomGenerator;
        }
    }
}
