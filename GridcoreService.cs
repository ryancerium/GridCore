using Gridcore.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Gridcore {
    public static class GridcoreExt {
        private static readonly FieldInfo bitArrayMArrayField = typeof(BitArray).GetField("m_array", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        public static int[] GetArray(this BitArray self) {
            return (int[]) bitArrayMArrayField.GetValue(self);
        }

        public static bool BitwiseEquals(this BitArray lhs, BitArray rhs) {
            return lhs.GetArray().SequenceEqual(rhs.GetArray());
        }

        public static String PressedKeys(this BitArray self) {
            var value = "";
            var intValue = 0;
            foreach (bool key in self) {
                if (key && Enum.IsDefined(typeof(VK), intValue)) {
                    if (value != "")
                        value += " ";

                    value += (VK) intValue;
                }
                ++intValue;
            }
            return value;
        }
    }

    internal struct HotkeyAction {
        public BitArray BitArray { get; }
        public Action Action { get; }

        public HotkeyAction(Action action, params VK[] keys) {
            BitArray = new BitArray(256);
            foreach (var key in keys) {
                BitArray[(int) key] = true;
            }
            Action = action;
        }
    }

    public class Gridcore {
        public Gridcore() {
        }

        private static readonly BitArray pressedKeys = new BitArray(256);

        private static List<HotkeyAction> mHotkeyActions = new List<HotkeyAction>();
        private static bool debugKeys = false;

        public static IntPtr LowLevelKeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode < 0) {
                return User32.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            }

            IntPtr wmKeyDown = new IntPtr(0x100);
            IntPtr wmKeyUp = new IntPtr(0x101);
            IntPtr wmSysKeyDown = new IntPtr(0x104);
            IntPtr wmSysKeyUp = new IntPtr(0x105);

            VK key = (VK) Marshal.ReadInt32(lParam);

            var down = (wParam == wmKeyDown || wParam == wmSysKeyDown);
            if (debugKeys) {
                Console.WriteLine(key + (down ? " DOWN" : " UP"));
            }
            pressedKeys[(int) key] = down;

            foreach (var hotkeyAction in mHotkeyActions) {
                if (hotkeyAction.BitArray.BitwiseEquals(pressedKeys)) {
                    hotkeyAction.Action();
                    if (debugKeys) {
                        Console.WriteLine($"Handled {pressedKeys.PressedKeys()}");
                    }
                    return new IntPtr(1);
                }
            }
            if (debugKeys) {
                Console.WriteLine($"Ignored {pressedKeys.PressedKeys()}");
            }

            return User32.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static MonitorInfoEx GetMonitor(IntPtr foregroundWindow) {
            var monitor = User32.MonitorFromWindow(foregroundWindow, User32.MONITOR_DEFAULTTOPRIMARY);
            MonitorInfoEx monitorInfo = new MonitorInfoEx();
            monitorInfo.Init();
            User32.GetMonitorInfo(monitor, ref monitorInfo);
            return monitorInfo;
        }

        const int SWP_NOZORDER = 0x0004;

        public static void TopLeft() {
            var foregroundWindow = User32.GetForegroundWindow();
            var workArea = GetMonitor(foregroundWindow).WorkArea;
            User32.SetWindowPos(foregroundWindow, new Rect(workArea.TopLeft, workArea.Center));
        }

        public static void TopRight() {
            var foregroundWindow = User32.GetForegroundWindow();
            var workArea = GetMonitor(foregroundWindow).WorkArea;
            User32.SetWindowPos(foregroundWindow, new Rect(workArea.TopRight, workArea.Center));
        }

        public static void BottomLeft() {
            var foregroundWindow = User32.GetForegroundWindow();
            var workArea = GetMonitor(foregroundWindow).WorkArea;
            User32.SetWindowPos(foregroundWindow, new Rect(workArea.BottomLeft, workArea.Center));
        }

        public static void BottomRight() {
            var foregroundWindow = User32.GetForegroundWindow();
            var workArea = GetMonitor(foregroundWindow).WorkArea;
            User32.SetWindowPos(foregroundWindow, new Rect(workArea.BottomRight, workArea.Center));
        }

        public static void Top() {
            var foregroundWindow = User32.GetForegroundWindow();
            var monitorInfo = GetMonitor(foregroundWindow);
            User32.ShowWindow(foregroundWindow, ShowWindowCommands.Restore);
            var margin = CalculateMargin(foregroundWindow);
            User32.SetWindowPos(
                    foregroundWindow,
                    IntPtr.Zero,
                    monitorInfo.Left + margin.Left,
                    monitorInfo.Top + margin.Top,
                    monitorInfo.Width + margin.Right - margin.Left,
                    monitorInfo.HalfHeight + margin.Bottom - margin.Top,
                    SWP_NOZORDER);
        }

        public static void Bottom() {
            var foregroundWindow = User32.GetForegroundWindow();
            var monitorInfo = GetMonitor(foregroundWindow);
            User32.ShowWindow(foregroundWindow, ShowWindowCommands.Restore);
            var margin = CalculateMargin(foregroundWindow);
            User32.SetWindowPos(
                    foregroundWindow,
                    IntPtr.Zero,
                    monitorInfo.Left + margin.Left,
                    monitorInfo.VCenter + margin.Top,
                    monitorInfo.Width + margin.Right - margin.Left,
                    monitorInfo.HalfHeight + margin.Bottom - margin.Top,
                    SWP_NOZORDER);
        }

        public static void Left() {
            var foregroundWindow = User32.GetForegroundWindow();
            var monitorInfo = GetMonitor(foregroundWindow);
            User32.ShowWindow(foregroundWindow, ShowWindowCommands.Restore);
            var margin = CalculateMargin(foregroundWindow);
            User32.SetWindowPos(
                    foregroundWindow,
                    IntPtr.Zero,
                    monitorInfo.Left + margin.Left,
                    monitorInfo.Top + margin.Top,
                    monitorInfo.HalfWidth + margin.Right - margin.Left,
                    monitorInfo.Height + margin.Bottom - margin.Top,
                    SWP_NOZORDER);
        }

        public static void Right() {
            var foregroundWindow = User32.GetForegroundWindow();
            var monitorInfo = GetMonitor(foregroundWindow);
            User32.ShowWindow(foregroundWindow, ShowWindowCommands.Restore);
            var margin = CalculateMargin(foregroundWindow);
            User32.SetWindowPos(
                    foregroundWindow,
                    IntPtr.Zero,
                    monitorInfo.HCenter + margin.Left,
                    monitorInfo.Top + margin.Top,
                    monitorInfo.HalfWidth + margin.Right - margin.Left,
                    monitorInfo.Height + margin.Bottom - margin.Top,
                    SWP_NOZORDER);
        }

        public static void PrintCurrentMonitor() {
            var foregroundWindow = User32.GetForegroundWindow();
            var monitor = User32.MonitorFromWindow(foregroundWindow, User32.MONITOR_DEFAULTTOPRIMARY);
            MonitorInfoEx monitorInfo = new MonitorInfoEx();
            monitorInfo.Init();
            User32.GetMonitorInfo(monitor, ref monitorInfo);
            Console.WriteLine(monitorInfo.DeviceName + " " + monitorInfo.WorkArea);
        }

        public static void PrintForegroundWindowExtendedFrameBounds() {
            var foregroundWindow = User32.GetForegroundWindow();
            Rect windowRect;
            User32.GetWindowRect(foregroundWindow, out windowRect);
            Rect extendedFrameBounds;
            User32.DwmGetWindowAttribute(foregroundWindow, DWMWINDOWATTRIBUTE.ExtendedFrameBounds, out extendedFrameBounds, 4 * 4);

            Console.WriteLine($"Window RECT: {windowRect} Extended Frame: {extendedFrameBounds}");
        }

        public static Rect CalculateMargin(IntPtr foregroundWindow) {
            Rect windowRect;
            User32.GetWindowRect(foregroundWindow, out windowRect);
            Rect extendedFrameBounds;
            User32.DwmGetWindowAttribute(foregroundWindow, DWMWINDOWATTRIBUTE.ExtendedFrameBounds, out extendedFrameBounds, 4 * 4);

            var margin = new Rect {
                Left = windowRect.Left - extendedFrameBounds.Left,
                Right = windowRect.Right - extendedFrameBounds.Right,
                Top = windowRect.Top - extendedFrameBounds.Top,
                Bottom = windowRect.Bottom - extendedFrameBounds.Bottom
            };
            Console.WriteLine(margin);
            return margin;
        }

        public static void Main(string[] args) {
            Console.WriteLine("Press a hotkey!");
            mHotkeyActions = new List<HotkeyAction>() {
                new HotkeyAction(PrintForegroundWindowExtendedFrameBounds, VK.LeftWindows, VK.LeftControl, VK.N0),
                new HotkeyAction(() => debugKeys = !debugKeys, VK.LeftWindows, VK.LeftControl, VK.K),
                new HotkeyAction(TopLeft, VK.LeftWindows, VK.LeftControl, VK.N1),
                new HotkeyAction(TopLeft, VK.LeftWindows, VK.Numpad7),
                new HotkeyAction(TopRight, VK.LeftWindows, VK.LeftControl, VK.N2),
                new HotkeyAction(TopRight, VK.LeftWindows, VK.Numpad9),
                new HotkeyAction(BottomLeft, VK.LeftWindows, VK.LeftControl, VK.N3),
                new HotkeyAction(BottomLeft, VK.LeftWindows, VK.Numpad1),
                new HotkeyAction(BottomRight, VK.LeftWindows, VK.LeftControl, VK.N4),
                new HotkeyAction(BottomRight, VK.LeftWindows, VK.Numpad3),
                new HotkeyAction(Top, VK.LeftWindows, VK.LeftControl, VK.N5),
                new HotkeyAction(Top, VK.LeftWindows, VK.Numpad8),
                new HotkeyAction(Bottom, VK.LeftWindows, VK.LeftControl, VK.N6),
                new HotkeyAction(Bottom, VK.LeftWindows, VK.Numpad2),
                new HotkeyAction(Left, VK.LeftWindows, VK.LeftControl, VK.N7),
                new HotkeyAction(Left, VK.LeftWindows, VK.Numpad4),
                new HotkeyAction(Right, VK.LeftWindows, VK.LeftControl, VK.N8),
                new HotkeyAction(Right, VK.LeftWindows, VK.Numpad6),
            };

            using (var curProcess = Process.GetCurrentProcess()) {
                using (var curModule = curProcess.MainModule) {
                    User32.SetWindowsHookEx(13, LowLevelKeyboardCallback, User32.GetModuleHandle(curModule.ModuleName), 0);
                }
            }


            Msg msg;
            while (User32.GetMessage(out msg, IntPtr.Zero, 0, 0) > 0) {
                User32.TranslateMessage(ref msg);
                User32.DispatchMessage(ref msg);
            }
        }
    }
}
