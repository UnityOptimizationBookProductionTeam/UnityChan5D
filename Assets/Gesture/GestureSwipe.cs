using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Gesture {
    public sealed class GestureSwipe : GestureBase {
        private Vector2 _deltaPos;

        public Vector2 deltaPos {
            get {
                return _deltaPos;
            }
#if UNITY_EDITOR
            // UnityEditor時シミュレート用.
            set {
                _deltaPos = value;
            }
#endif
        }

        private bool _end;

        public bool end {
            get {
                return _end;
            }
#if UNITY_EDITOR
            // UnityEditor時シミュレート用.
            set {
                _end = value;
            }
#endif
        }

        private bool _stationary;

        public bool stationary {
            get {
                return _stationary;
            }
        }

        private bool _swipe;

        public bool swipe {
            get {
                return _swipe;
            }
#if UNITY_EDITOR
            // UnityEditor時シミュレート用.
            set {
                _swipe = value;
            }
#endif
        }

        public GestureSwipe(UnityAction callback, float time = 0.0f, float threshold = 50.0f, bool stationary = false) {
            Assert.IsNotNull(callback, "GestureSwipe : callback is null");

            _callback = callback;
            _time = time;
            _threshold = threshold;
            _sqrThreshold = _threshold * _threshold;
            _stationary = stationary;

            _enabled = true; // 有効化.

            // コンストラクタでManagerに登録する設計.
            GestureManager.instance.AddSwipe(this);
        }

        public override void Destroy() {
            base.Destroy();
            // 明示的に登録を削除する.
            GestureManager.instance.RemoveSwipe(this);
        }

        public override void SetTouch(ref Touch touch, int count, float deltaTime) {
            // 無効指定されているなら処理しない.
            if (!_enabled) {
                return;
            }

            var phase = touch.phase;
            var id = touch.fingerId;

            if (phase == TouchPhase.Began) {
                _fingerId = id;
                _startPos = touch.position;
                _swipe = false;
                _end = false;
            }

            if (id == _fingerId) {
                if (phase == TouchPhase.Moved || phase == TouchPhase.Stationary ||
                phase == TouchPhase.Ended || phase == TouchPhase.Canceled) {

                    if (phase == TouchPhase.Stationary && !_stationary) {
                        return;
                    }

                    var now = touch.position;
                    var diffPos = new Vector2(now.x - _startPos.x, now.y - _startPos.y);

                    if (Check(diffPos)) {
                        _position = now;
                        _deltaPos = touch.deltaPosition;

                        if (phase == TouchPhase.Ended || phase == TouchPhase.Canceled) {
                            _fingerId = InvalidFingerId;
                            _swipe = false;
                            _end = true;
                        }

                        _callback();
                    }
                }
            }
        }

        public bool Check(Vector2 diffPos) {
            if (_swipe) { // 一度有効になったら以降は判定しない.
                return true;
            }

            _swipe = diffPos.sqrMagnitude > _sqrThreshold ? true : false;

            return _swipe;
        }
    }
}
