using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeWorld.Gameplay
{
    public class GameplayFactory
    {
        static private GameplayDefinition[] gameplays;

        static public GameplayDefinition[] AvailableGameplays
        {
            get
            {
                if (gameplays == null)
                {
                    gameplays = new GameplayDefinition[] {
                        new GameplayDefinition("Basic", "Basic Gameplay", new BasicGameplay(), false),
                        new GameplayDefinition("Roguelike", "Roguelike Gameplay", new RoguelikeGameplay(), true),
                        new GameplayDefinition("Basic w/Enemies", "Basic Gameplay with Enemies", new BasicEnemiesGameplay(), false)
                    };
                }

                return gameplays;
            }
        }

        static public BaseGameplay GetGameplayById(string id)
        {
            foreach (GameplayDefinition gameplayDefinition in AvailableGameplays)
                if (gameplayDefinition.id == id)
                    return gameplayDefinition.gameplay;

            return null;
        }
    }
}
