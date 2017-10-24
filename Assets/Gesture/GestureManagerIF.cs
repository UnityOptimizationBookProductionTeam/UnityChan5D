using System.Collections.Generic;

namespace Gesture {

    /// <summary>
    /// partialで分割したGestureManagerクラス.
    /// 各Gestureの操作インターフェイスを実装.
    /// </summary>
    public partial class GestureManager {

        public void Clear() {
            if (_listTap != null) {
                _listTap.Clear();
            }

            if (_listLongTap != null) {
                _listLongTap.Clear();
            }

            if (_listSwipe != null) {
                _listSwipe.Clear();
            }

            if (_listFlick != null) {
                _listFlick.Clear();
            }

            if (_listPinch != null) {
                _listPinch.Clear();
            }
        }

        public bool AddTap(GestureTap tap) {
            if (tap == null) {
                return false;
            }

            if (_listTap == null) {
                _listTap = new List<GestureTap>();
            }

            _listTap.Add(tap);

            return true;
        }

        public bool RemoveTap(GestureTap tap) {
            if (tap == null || _listTap == null) {
                return false;
            }

            return _listTap.Remove(tap);
        }

        public bool AddLongTap(GestureLongTap tap) {
            if (tap == null) {
                return false;
            }

            if (_listLongTap == null) {
                _listLongTap = new List<GestureLongTap>();
            }

            _listLongTap.Add(tap);

            return true;
        }

        public bool RemoveLongTap(GestureLongTap tap) {
            if (tap == null || _listLongTap == null) {
                return false;
            }

            return _listLongTap.Remove(tap);
        }

        public bool AddSwipe(GestureSwipe swipe) {
            if (swipe == null) {
                return false;
            }

            if (_listSwipe == null) {
                _listSwipe = new List<GestureSwipe>();
            }

            _listSwipe.Add(swipe);

            return true;
        }

        public bool RemoveSwipe(GestureSwipe swipe) {
            if (swipe == null || _listSwipe == null) {
                return false;
            }

            return _listSwipe.Remove(swipe);
        }

        public bool AddFlick(GestureFlick flick) {
            if (flick == null) {
                return false;
            }

            if (_listFlick == null) {
                _listFlick = new List<GestureFlick>();
            }

            _listFlick.Add(flick);

            return true;
        }

        public bool RemoveFlick(GestureFlick flick) {
            if (flick == null || _listFlick == null) {
                return false;
            }

            return _listFlick.Remove(flick);
        }

        public bool AddPinch(GesturePinch pinch) {
            if (pinch == null) {
                return false;
            }

            if (_listPinch == null) {
                _listPinch = new List<GesturePinch>();
            }

            _listPinch.Add(pinch);

            return true;
        }

        public bool RemovePinch(GesturePinch pinch) {
            if (pinch == null || _listPinch == null) {
                return false;
            }

            return _listPinch.Remove(pinch);
        }
    }
}
