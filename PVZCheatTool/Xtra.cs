using System;
using System.Runtime.InteropServices;

namespace Misc
{
    class Xtra
    {
        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(System.Int32 vKey);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("Kernel32")]
        public static extern IntPtr GetConsoleWindow();

        public static int SW_HIDE = 0;
        public static int SW_SHOW = 5;
    }
}
