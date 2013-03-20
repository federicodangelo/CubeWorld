namespace CubeWorld.World
{
    public class CubeWorldStats
    {
        public int executedRules;
        public int invalidatedTiles;
        public int updatedTiles;
        public int checkedConditions;

        public void Reset()
        {
            executedRules = 0;
            invalidatedTiles = 0;
            updatedTiles = 0;
            checkedConditions = 0;
        }

        public override string ToString()
        {
            return "ER: " + executedRules + " , IT: " + invalidatedTiles + " , UT: " + updatedTiles + " , CC: " + checkedConditions;
        }
    }
}