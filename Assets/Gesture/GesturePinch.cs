using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Gesture {
    public sealed class GesturePinch : GestureBase {
        private bool _pinchIn;

#if UNITY_EDITOR
        // UnityEditor時シミュレート用.
        public bool pinchIn {
            get {
                return _pinchIn;
            }
        }

        public float _wheel;
#endif

        private Vector2 _firstPos;
        private Vector2 _firstLastPos;

        public GesturePinch(UnityAction callback, bool pinchIn, float threshold = 5.0f) {
            Assert.IsNotNull(callback, "GesturePinch : callback is null");

            _pinchIn = pinchIn;

            _callback = callback;
            _threshold = threshold;
            _sqrThreshold = _threshold * _threshold;

            _enabled = true; // 有効化.

            // コンストラクタでManagerに登録する設計.
            GestureManager.instance.AddPinch(this);
        }

        public override void Destroy() {
            // 明示的に登録を削除する.
            GestureManager.instance.RemovePinch(this);
        }

        public override void SetTouch(ref Touch touch, int count, float deltaTime) {
            // 無効指定されているなら処理しない.
            // 3本指のタッチがあった時点で処理しない.
            if (!_enabled || count > 1) {
                return;
            }

            if (count == 0) {
                _firstPos = touch.position;
                _firstLastPos = _firstPos - touch.deltaPosition; // 前回の座標.
            }
            else {
                Vector2 pos = touch.position;
                Vector2 delta = touch.deltaPosition;
                Vector2 nowLastPos = pos - delta; // 前回の座標.
                // 前回と今回で2点間の同時のベクトルを求める.
                Vector2 last = nowLastPos - _firstLastPos;
                Vector2 now = pos - _firstPos;
                float sqrLast = last.sqrMagnitude;
                float sqrNow = now.sqrMagnitude;

                // 大きさを比べる.
                if (sqrLast > sqrNow) {
                    // PinchIn.
                    if (_pinchIn) {
                        if (sqrLast - sqrNow > _sqrThreshold) {
                            _callback();
                        }
                    }
                }
                else if (sqrLast < sqrNow) {
                    // PinchOut.
                    if (!_pinchIn) {
                        if (sqrNow - sqrLast > _sqrThreshold) {
                            _callback();
                        }
                    }
                }
            }
        }
    }
}
