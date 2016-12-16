using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Screenshotr.Windows;

namespace Screenshotr.Forms
{
    public sealed partial class RegionArea : Form
    {
        private readonly Pen _borderPen = new Pen(Color.FromArgb(128, 0, 255, 0), 3);
        private readonly Button _btnTakeScreenshot = new Button();
        private readonly Brush _fillBrush = new SolidBrush(Color.Green);
        private readonly IntPtr _desktopPtr = NativeMethods.GetDesktopWindow();

        private bool _isAreaSelected;
        private bool _isDragging;
        private bool _isMouseDown;
        private CaptureControlLocations _selectedNode;
        private Point _startPoint;
        private readonly List<IntPtr> _windows;

        public Rectangle SelectedArea;


        public RegionArea()
        {
            InitializeComponent();
            InitializeButton();

            _windows = GetChildWindows(_desktopPtr);

            Bounds = SystemInformation.VirtualScreen;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Blue;
            TransparencyKey = BackColor;
        }

        private void InitializeButton()
        {
            Controls.Add(_btnTakeScreenshot);
            _btnTakeScreenshot.Text = "Take Screenshot";
            _btnTakeScreenshot.BackColor = Color.White;
            _btnTakeScreenshot.Width = 100;
            _btnTakeScreenshot.AutoSize = true;
            _btnTakeScreenshot.Click += TakeScreenshot;
            _btnTakeScreenshot.Hide();
        }

        private void TakeScreenshot(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        public void SetButtonProperties(Rectangle selectedArea)
        {
            var width = _btnTakeScreenshot.Width;
            _btnTakeScreenshot.Location = new Point(selectedArea.Left + selectedArea.Width/2 - width/2,
                selectedArea.Y + selectedArea.Height + 5);
            _btnTakeScreenshot.Show();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            _selectedNode = GetSelectedNode(e.Location);
            if (_selectedNode != CaptureControlLocations.None) _isDragging = true;

            _isMouseDown = true;
            _isAreaSelected = true;
            _startPoint = e.Location;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isMouseDown)
            {
                if (_isDragging)
                {
                    var deltaX = e.X - _startPoint.X;
                    var deltaY = e.Y - _startPoint.Y;

                    var newX = SelectedArea.Left;
                    var newY = SelectedArea.Top;
                    var newWidth = SelectedArea.Width;
                    var newHeight = SelectedArea.Height;

                    switch (_selectedNode)
                    {
                        case CaptureControlLocations.TopLeft:
                            newX += deltaX;
                            newY += deltaY;
                            newWidth -= deltaX;
                            newHeight -= deltaY;
                            break;
                        case CaptureControlLocations.TopRight:
                            newY += deltaY;
                            newWidth += deltaX;
                            newHeight -= deltaY;
                            break;
                        case CaptureControlLocations.BottomLeft:
                            newX += deltaX;
                            newWidth -= deltaX;
                            newHeight += deltaY;
                            break;
                        case CaptureControlLocations.BottomRight:
                            newWidth += deltaX;
                            newHeight += deltaY;
                            break;
                    }
                    if ((newWidth > 0) && (newHeight > 0))
                    {
                        SelectedArea.X = newX;
                        SelectedArea.Y = newY;
                        SelectedArea.Width = newWidth;
                        SelectedArea.Height = newHeight;
                    }
                    _startPoint = e.Location;
                }
                else
                    SelectedArea = new Rectangle(
                        Math.Min(_startPoint.X, e.X),
                        Math.Min(_startPoint.Y, e.Y),
                        Math.Abs(_startPoint.X - e.X),
                        Math.Abs(_startPoint.Y - e.Y));
            }
            else
            {
                if (!_isAreaSelected)
                    foreach (var window in _windows)
                    {
                        NativeRectangle rect;
                        NativeMethods.GetWindowRect(window, out rect);
                        var newRect = rect.ToRectangle();
                        if (newRect.Contains(e.Location))
                        {
                            SelectedArea = newRect;
                            break;
                        }
                    }
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(_borderPen, SelectedArea);
            SetButtonProperties(SelectedArea);

            foreach (CaptureControlLocations controlLocations in Enum.GetValues(typeof(CaptureControlLocations)))
            {
                var tempRect = GetControlNodes(controlLocations);
                e.Graphics.FillRectangle(_fillBrush, tempRect);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _isMouseDown = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private CaptureControlLocations GetSelectedNode(Point p)
        {
            foreach (CaptureControlLocations loc in Enum.GetValues(typeof(CaptureControlLocations)))
                if (GetControlNodes(loc).Contains(p)) return loc;
            return CaptureControlLocations.None;
        }

        private Rectangle GetControlNodes(CaptureControlLocations location)
        {
            switch (location)
            {
                case CaptureControlLocations.TopLeft:
                    return new Rectangle(SelectedArea.X - 4, SelectedArea.Y - 4, 8, 8);
                case CaptureControlLocations.TopRight:
                    return new Rectangle(SelectedArea.X + SelectedArea.Width - 4, SelectedArea.Y - 4, 8, 8);
                case CaptureControlLocations.BottomLeft:
                    return new Rectangle(SelectedArea.X - 4, SelectedArea.Y + SelectedArea.Height - 4, 8, 8);
                case CaptureControlLocations.BottomRight:
                    return new Rectangle(SelectedArea.X + SelectedArea.Width - 4,
                        SelectedArea.Y + SelectedArea.Height - 4, 8, 8);
                default:
                    return new Rectangle();
            }
        }

        public List<IntPtr> GetChildWindows(IntPtr parent)
        {
            var result = new List<IntPtr>();
            var listHandle = GCHandle.Alloc(result);
            try
            {
                NativeMethods.EnumWindowsProc enumProc = EnumWindow;
                NativeMethods.EnumWindows(enumProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }

            return result;
        }

        private bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            if (handle == Handle) return true;

            var gch = GCHandle.FromIntPtr(pointer);
            var list = gch.Target as List<IntPtr>;
            if (list == null)
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            if (IsAppWindow(handle))
                list.Add(handle);

            return true;
        }

        private static bool IsAppWindow(IntPtr hWnd)
        {
            var style = NativeMethods.GetWindowLong(hWnd, -16);

            var wsVisible = (style & 0x10000000) == 0x10000000;
            var wsCaption = (style & 0x00C00000) == 0x00C00000;
            var wsPopup = (style & 0x80000000) == 0x80000000;

            return wsVisible && (wsPopup || wsCaption);
        }

        private enum CaptureControlLocations
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            None
        }
    }
}