using Gridcore.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using static Gridcore.Win32.DwmApi;
using static Gridcore.Win32.DwmWindowAttribute;
using static Gridcore.Win32.Kernel32;
using static Gridcore.Win32.User32;

namespace Gridcore {

    public class Gridcore {
        private static readonly BitArray mPressedKeys = new BitArray(256);

        private static List<HotkeyAction> mHotkeyActions = new List<HotkeyAction>();
        private static List<MonitorInfoEx> mMonitors = new List<MonitorInfoEx>();

        private static bool mDebugKeys = false;

        private static IntPtr wmKeyDown = new IntPtr(0x100);
        private static IntPtr wmKeyUp = new IntPtr(0x101);
        private static IntPtr wmSysKeyDown = new IntPtr(0x104);
        private static IntPtr wmSysKeyUp = new IntPtr(0x105);

        private static IntPtr LowLevelKeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode < 0) {
                return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            }

            var key = Marshal.ReadInt32(lParam);

            var down = (wParam == wmKeyDown || wParam == wmSysKeyDown);
            if (mDebugKeys) {
                Console.WriteLine((VK) key + (down ? " DOWN" : " UP"));
            }
            mPressedKeys[key] = down;

            foreach (var hotkeyAction in mHotkeyActions) {
                if (hotkeyAction.BitArray.BitwiseEquals(mPressedKeys)) {
                    hotkeyAction.Action();
                    if (mDebugKeys) {
                        Console.WriteLine($"Handled {mPressedKeys.ToKeyString()}");
                    }
                    return new IntPtr(1);
                }
            }
            if (mDebugKeys) {
                Console.WriteLine($"Ignored {mPressedKeys.ToKeyString()}");
            }

            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static bool EnumDisplayMonitorsCallback(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData) {
            MonitorInfoEx monitorInfo = new MonitorInfoEx().Init();
            GetMonitorInfo(hMonitor, ref monitorInfo);
            mMonitors.Add(monitorInfo);
            Console.WriteLine($"{monitorInfo.DeviceName}: {monitorInfo.WorkArea}");
            return true;
        }

        private static Action SetWindowPosAction(Func<Rect, Rect> workAreaToWindowPos) {
            return () => {
                var foregroundWindow = GetForegroundWindow();
                var monitorInfo = new MonitorInfoEx().Init();
                GetMonitorInfo(MonitorFromWindow(foregroundWindow, MonitorDefault.Primary), ref monitorInfo);
                SetWindowPos(foregroundWindow, workAreaToWindowPos(monitorInfo.WorkArea));
            };
        }

        private static Action TopLeft = SetWindowPosAction(workArea => new Rect(workArea.TopLeft, workArea.Center));
        private static Action TopRight = SetWindowPosAction(workArea => new Rect(workArea.TopRight, workArea.Center));
        private static Action BottomLeft = SetWindowPosAction(workArea => new Rect(workArea.BottomLeft, workArea.Center));
        private static Action BottomRight = SetWindowPosAction(workArea => new Rect(workArea.BottomRight, workArea.Center));
        private static Action Top = SetWindowPosAction(workArea => new Rect(workArea.TopLeft, new Point(workArea.Right, workArea.VCenter)));
        private static Action Bottom = SetWindowPosAction(workArea => new Rect(workArea.BottomLeft, new Point(workArea.Right, workArea.VCenter)));
        private static Action Left = SetWindowPosAction(workArea => new Rect(workArea.TopLeft, new Point(workArea.HCenter, workArea.Bottom)));
        private static Action Right = SetWindowPosAction(workArea => new Rect(workArea.TopRight, new Point(workArea.HCenter, workArea.Bottom)));

        private static void PrintCurrentMonitor() {
            var foregroundWindow = GetForegroundWindow();
            var monitorInfo = new MonitorInfoEx().Init();
            GetMonitorInfo(MonitorFromWindow(foregroundWindow, MonitorDefault.Primary), ref monitorInfo);
            Console.WriteLine(monitorInfo.DeviceName + " " + monitorInfo.WorkArea);
        }

        private static void MoveToNextMonitor() {
            mMonitors.Clear();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, EnumDisplayMonitorsCallback, IntPtr.Zero);
            mMonitors.Sort();

            var foregroundWindow = GetForegroundWindow();
            var monitorInfo = new MonitorInfoEx().Init();
            GetMonitorInfo(MonitorFromWindow(foregroundWindow, MonitorDefault.Primary), ref monitorInfo);

            var i = mMonitors.IndexOf(monitorInfo) + 1;
            if (i == mMonitors.Count) {
                i = 0;
            }
            var workArea = mMonitors[i].WorkArea;
            var windowPos = new Rect(workArea.TopLeft, workArea.Center);
            SetWindowPos(foregroundWindow, windowPos, 0);
            SetCursorPos(windowPos.Center.X, windowPos.Center.Y);
        }

        private static void PrintForegroundWindowExtendedFrameBounds() {
            var foregroundWindow = GetForegroundWindow();
            Rect windowRect;
            GetWindowRect(foregroundWindow, out windowRect);
            Rect extendedFrameBounds;
            DwmGetWindowAttribute(foregroundWindow, ExtendedFrameBounds, out extendedFrameBounds, 4 * 4);

            Console.WriteLine($"Window: {windowRect} Extended Frame: {extendedFrameBounds}");
        }

        public static void Main(string[] args) {
            Console.WriteLine("Press a hotkey!");
            mHotkeyActions = new List<HotkeyAction>() {
                new HotkeyAction(PrintForegroundWindowExtendedFrameBounds, VK.LeftWindows, VK.LeftControl, VK.N0),
                new HotkeyAction(() => mDebugKeys = !mDebugKeys, VK.LeftWindows, VK.LeftControl, VK.K),
                new HotkeyAction(MoveToNextMonitor, VK.LeftWindows, VK.Numpad5),
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
                    SetWindowsHookEx(13, LowLevelKeyboardCallback, GetModuleHandle(curModule.ModuleName), 0);
                }
            }

            Msg msg;
            while (GetMessage(out msg, IntPtr.Zero, 0, 0) > 0) {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }
    }
}
