namespace CubeWorld.World.Generator
{
    public class CubeWorldGenerator
    {
        private bool executed;

        public virtual void Prepare()
        {
            executed = false;
        }

        public virtual int GetTotalCost()
        {
            return 1;
        }

        public virtual int GetCurrentCost()
        {
            if (executed)
                return 1;
            else
                return 0;
        }

        public virtual bool Generate(CubeWorld world)
        {
            return true;
        }
    }
}