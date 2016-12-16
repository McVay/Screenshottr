using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Screenshotr.Windows
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr GetDC(IntPtr windowHandle);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        internal static extern bool ReleaseDC(IntPtr windowHandle, IntPtr dcHandle);

        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("gdi32.dll")]
        public static extern bool PtInRegion(IntPtr hrgn, int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindowDC(IntPtr windowHandle);

        [DllImport("user32.dll")]
        internal static extern bool ClientToScreen(IntPtr hwnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern IntPtr WindowFromPoint(Point pnt);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateCompatibleDC(IntPtr dcHandle);

        [DllImport("gdi32.dll")]
        internal static extern bool DeleteDC(IntPtr dcHandle);

        [DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr objectHandle);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr SelectObject(IntPtr dcHandle, IntPtr objectHandle);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateCompatibleBitmap(IntPtr dcHandle, int width, int height);

        [DllImport("gdi32.dll")]
        internal static extern bool BitBlt(
            IntPtr destinationDcHandle,
            int destinationX,
            int destinationY,
            int width,
            int height,
            IntPtr sourceDcHandle,
            int sourceX,
            int sourceY,
            CopyPixelOperation rasterOperation);


        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        internal static extern bool GetWindowRect(IntPtr windowHandle, out NativeRectangle rectangle);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeRectangle
    {
        internal int Left;
        internal int Top;
        internal int Right;
        internal int Bottom;

        internal Rectangle ToRectangle()
        {
            return new Rectangle(Left, Top, Right - Left, Bottom - Top);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Win32Point
    {
        public Int32 X;
        public Int32 Y;
    };
}