using System.Collections.Generic;

namespace CubeWorld.World.Generator
{
    public class ChainedWorldGenerator : CubeWorldGenerator
    {
        private List<CubeWorldGenerator> generators = new List<CubeWorldGenerator>();
        private int currentGeneratorIndex;
        private int currentCost;

        public void AddGenertor(CubeWorldGenerator generator)
        {
            generators.Add(generator);
        }

        public override void Prepare()
        {
            currentGeneratorIndex = 0;
            currentCost = 0;

            foreach (CubeWorldGenerator generator in generators)
                generator.Prepare();
        }

        public override int GetTotalCost()
        {
            int total = 0;
            foreach (CubeWorldGenerator generator in generators)
                total += generator.GetTotalCost();
            return total;
        }

        public override int GetCurrentCost()
        {
            int cost = currentCost;

            if (currentGeneratorIndex < generators.Count)
                cost += generators[currentGeneratorIndex].GetCurrentCost();

            return cost;
        }

        public override bool Generate(CubeWorld world)
        {
            if (currentGeneratorIndex < generators.Count)
            {
                if (generators[currentGeneratorIndex].Generate(world) == true)
                {
                    currentCost += generators[currentGeneratorIndex].GetTotalCost();
                    currentGeneratorIndex++;
                }
            }

            return currentGeneratorIndex == generators.Count;
        }

        public override string ToString()
        {
            if (currentGeneratorIndex < generators.Count)
                return generators[currentGeneratorIndex].ToString();
            else
                return "All sort of things";
        }
    }
}