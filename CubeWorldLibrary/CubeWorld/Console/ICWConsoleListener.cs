using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeWorld.Console
{
    public interface ICWConsoleListener
    {
        void Log(CWConsole.LogLevel level, string message);
    }
}
