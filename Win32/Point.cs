using System.Runtime.InteropServices;

namespace Gridcore.Win32 {
    [StructLayout(LayoutKind.Sequential)]
    public struct Point {
        public int X; // LONG
        public int Y; // LONG

        public Point(int x, int y) {
            X = x;
            Y = y;
        }

        public Point TranslateX(int xDelta) {
            return new Point(X + xDelta, Y);
        }

        public Point TranslateY(int yDelta) {
            return new Point(X, Y + yDelta);
        }

        public static Point operator +(Point lhs, Point rhs) {
            return new Point(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }

        public static Point operator -(Point lhs, Point rhs) {
            return new Point(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static Point operator *(Point lhs, int rhs) {
            return new Point(lhs.X * rhs, lhs.Y * rhs);
        }

        public static Point operator *(int lhs, Point rhs) {
            return new Point(lhs * rhs.X, lhs * rhs.Y);
        }

        public static Point operator /(Point lhs, int rhs) {
            return new Point(lhs.X / rhs, lhs.Y / rhs);
        }
    }
}
