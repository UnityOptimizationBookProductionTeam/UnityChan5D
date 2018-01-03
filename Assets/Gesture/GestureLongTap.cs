using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Gesture {
    public sealed class GestureLongTap : GestureBase {
        private bool _longTap;

#if UNITY_EDITOR
        // UnityEditor時シミュレート用.
        public bool longTap {
            get {
                return _longTap;
            }
            set {
                _longTap = value;
            }
        }
#endif

        public GestureLongTap(UnityAction callback, float time = 1.25f, float threshold = 10.0f) {
            Assert.IsNotNull(callback, "GestureLongTap : callback is null");

            _callback = callback;
            _time = time; // ロングタップと判定する時間(この時間より経過しないとロングタップと判定しない).
            _threshold = threshold; // タップと判定する座標の閾値(この値よりズレたらタップと判定しない).
            _sqrThreshold = _threshold * _threshold;

            _enabled = true; // 有効化.

            // コンストラクタでManagerに登録する設計.
            GestureManager.instance.AddLongTap(this);
        }

        public override void Destroy() {
            base.Destroy();
            // 明示的に登録を削除する.
            GestureManager.instance.RemoveLongTap(this);
        }

        public override void SetTouch(ref Touch touch, int count, float deltaTime) {
            // 無効指定されているなら処理しない.
            if (!_enabled) {
                return;
            }

            var phase = touch.phase;
            var id = touch.fingerId;

            // Beganで初期化.
            if (phase == TouchPhase.Began) {
                _fingerId = id;
                _startPos = touch.position;
                _timer = 0.0f;
                _longTap = false;
            }

            // ロングタップコールバックを行っておらず移動がステイなら.
            if (id == _fingerId && !_longTap && (phase == TouchPhase.Moved || phase == TouchPhase.Stationary)) {
                var now = touch.position;
                var diffPos = new Vector2(now.x - _startPos.x, now.y - _startPos.y);

                if (Check(diffPos, _timer)) {
                    _longTap = true;
                    _fingerId = InvalidFingerId;
                    _position = touch.position;
                    _callback();
                }
            }

            _timer += deltaTime;
        }

        public bool Check(Vector2 diffPos, float deltaTime) {
            // 指定時間未満ならロングタップではない.
            if (deltaTime < _time) {
                return false;
            }

            // 開始座標との位置差分を2乗の状態で判定.
            return diffPos.sqrMagnitude > _sqrThreshold ? false : true;
        }
    }
}
