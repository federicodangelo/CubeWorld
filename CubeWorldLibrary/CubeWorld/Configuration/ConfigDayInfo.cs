using System;
using CubeWorld.World.Lights;

namespace CubeWorld.Configuration
{
    public class ConfigDayInfo
    {
        public string name;

        public int dayDuration;
        public DayTimeLuminanceInfo[] dayTimeLuminances;
    }
}
