using Gridcore.Win32;
using System;
using System.Collections;

namespace Gridcore {
    internal struct HotkeyAction {
        public BitArray BitArray { get; }
        public Action Action { get; }

        public HotkeyAction(Action action, params VK[] keys) {
            Action = action;
            BitArray = new BitArray(256);

            foreach (var key in keys) {
                BitArray[(int) key] = true;
            }

        }
    }
}
