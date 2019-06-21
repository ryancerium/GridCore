using System;
using System.Runtime.InteropServices;

namespace Gridcore.Win32 {
    /// <summary>
    /// The RECT structure defines the coordinates of the upper-left and lower-right corners of a rectangle.
    /// </summary>
    /// <see cref="http://msdn.microsoft.com/en-us/library/dd162897%28VS.85%29.aspx"/>
    /// <remarks>
    /// By convention, the right and bottom edges of the rectangle are normally considered exclusive.
    /// In other words, the pixel whose coordinates are ( right, bottom ) lies immediately outside of the the rectangle.
    /// For example, when RECT is passed to the FillRect function, the rectangle is filled up to, but not including,
    /// the right column and bottom row of pixels. This structure is identical to the RECTL structure.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect {
        /// <summary>
        /// The x-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int Left;

        /// <summary>
        /// The y-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int Top;

        /// <summary>
        /// The x-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int Right;

        /// <summary>
        /// The y-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int Bottom;

        public Rect(Point p1, Point p2) {
            Left = Math.Min(p1.X, p2.X);
            Top = Math.Min(p1.Y, p2.Y);
            Right = Math.Max(p1.X, p2.X);
            Bottom = Math.Max(p1.Y, p2.Y);
        }

        public override string ToString() {
            return $"L:{Left} T:{Top} R:{Right} B:{Bottom}";
        }

        public int Width => Right - Left;
        public int Height => Bottom - Top;
        public int HalfWidth => Width / 2;
        public int HalfHeight => Height / 2;
        public int HCenter => Left + HalfWidth;
        public int VCenter => Top + HalfHeight;

        public Point TopLeft => new Point(Left, Top);
        public Point TopRight => new Point(Right, Top);
        public Point BottomLeft => new Point(Left, Bottom);
        public Point BottomRight => new Point(Right, Bottom);
        public Point Center => new Point(HCenter, VCenter);
        public Point Size => new Point(Width, Height);
    }
}
