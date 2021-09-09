using System;
using System.Runtime.InteropServices;

namespace Misc
{
    class Xtra
    {
        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(System.Int32 vKey);
    }
}
