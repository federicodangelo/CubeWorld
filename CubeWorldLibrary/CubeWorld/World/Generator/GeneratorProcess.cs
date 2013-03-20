using System;
using System.Collections.Generic;

namespace CubeWorld.World.Generator
{
    public class GeneratorProcess
    {
        private CubeWorldGenerator generator;
        private CubeWorld world;

        private bool finished = false;

        private int totalCost;

        public GeneratorProcess(CubeWorldGenerator generator, CubeWorld world)
        {
            this.finished = false;
            this.generator = generator;
            this.world = world;

            generator.Prepare();
            totalCost = generator.GetTotalCost();
        }

        public bool Generate()
        {
            if (finished == false)
                if (generator.Generate(world) == true)
                    finished = true;

            return finished;
        }

        public int GetProgress()
        {
            if (totalCost != 0)
                return generator.GetCurrentCost() * 100 / totalCost;
            else
                return 100;
        }

        public bool IsFinished()
        {
            return finished;
        }

        public override string ToString()
        {
            return generator.ToString();
        }
    }
}
