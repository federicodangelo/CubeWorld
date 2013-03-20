namespace CubeWorld.World.Lights
{
    public class DayTimeLuminanceInfo
    {
        public float toTimePercent;
        public int luminancePercent;

        public DayTimeLuminanceInfo(float toTimePercent, int luminancePercent)
        {
            this.toTimePercent = toTimePercent;
            this.luminancePercent = luminancePercent;
        }
    }
}
