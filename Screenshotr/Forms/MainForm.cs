using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Screenshotr.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            InitializeHotkeys();
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private void InitializeHotkeys()
        {
            RegisterHotKey(Handle, 0, (int) KeyModifier.None, Keys.PrintScreen.GetHashCode());
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0312)
            {
                var key = (Keys) (((int) m.LParam >> 16) & 0xFFFF); // The key of the hotkey that was pressed.
                var modifier = (KeyModifier) ((int) m.LParam & 0xFFFF); // The modifier of the hotkey that was pressed.
                var id = m.WParam.ToInt32(); // The id of the hotkey that was pressed.

                var dialog = new RegionArea();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var screenshot = ScreenshotProvider.TakeScreenshot(SystemInformation.VirtualScreen);
                    var bitmap = screenshot.Clone(dialog.SelectedArea, PixelFormat.Format32bppArgb);
                    ImgurHelper.UploadToImgur(bitmap);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnregisterHotKey(Handle, 0);
        }

        private enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }
    }
}