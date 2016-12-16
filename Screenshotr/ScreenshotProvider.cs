using System.Drawing;
using Screenshotr.Windows;

namespace Screenshotr
{
    public static class ScreenshotProvider
    {
        public static Bitmap TakeScreenshot(Rectangle area)
        {
            // Use the Windows API here instead of the managed equivalent because the
            // managed equivalent has a number of bugs.

            var handleDesktopWindow = NativeMethods.GetDesktopWindow();
            var handleSource = NativeMethods.GetWindowDC(handleDesktopWindow);
            var handleDestination = NativeMethods.CreateCompatibleDC(handleSource);
            var handleBitmap = NativeMethods.CreateCompatibleBitmap(handleSource, area.Width, area.Height);
            var handleOldBitmap = NativeMethods.SelectObject(handleDestination, handleBitmap);

            NativeMethods.BitBlt(
                handleDestination,
                0,
                0,
                area.Width,
                area.Height,
                handleSource,
                area.X,
                area.Y,
                CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);

            var screenshot = Image.FromHbitmap(handleBitmap);
            NativeMethods.SelectObject(handleDestination, handleOldBitmap);
            NativeMethods.DeleteObject(handleBitmap);
            NativeMethods.DeleteDC(handleDestination);
            NativeMethods.ReleaseDC(handleDesktopWindow, handleSource);

            return screenshot;
        }
    }
}