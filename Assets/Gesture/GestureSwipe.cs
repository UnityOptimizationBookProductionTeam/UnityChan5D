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
            set {
                _stationary = value;
            }
        }

        private bool _swipe;

#if UNITY_EDITOR
        // UnityEditor時シミュレート用.
        public bool swipe {
            get {
                return _swipe;
            }
            set {
                _swipe = value;
            }
        }
#endif

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
            // 明示的に登録を削除する.
            GestureManager.instance.RemoveSwipe(this);
        }

        public override void SetTouch(ref Touch touch, int count, float deltaTime) {
            // 無効指定されているなら処理しない.
            // 2本指のタッチがあった時点で処理しない.
            if (!_enabled || count > 0) {
                return;
            }

            TouchPhase phase = touch.phase;

            if (phase == TouchPhase.Began) {
                _startPos = touch.position;
                _swipe = false;
            }

            if (phase == TouchPhase.Moved || phase == TouchPhase.Stationary ||
                phase == TouchPhase.Ended || phase == TouchPhase.Canceled) {

                if (!_stationary && phase == TouchPhase.Stationary) {
                    return;
                } 

                Vector2 now = touch.position;
                Vector2 diffPos = new Vector2(now.x - _startPos.x, now.y - _startPos.y);

                if (Check(diffPos)) {
                    _position = touch.position;
                    _deltaPos = touch.deltaPosition;
                    _end = (phase == TouchPhase.Ended || phase == TouchPhase.Canceled) ? true : false;

                    _callback();
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
