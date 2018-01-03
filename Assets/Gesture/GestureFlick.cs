using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Gesture {
    public sealed class GestureFlick : GestureBase {

        public GestureFlick(UnityAction callback, float time = 0.3f, float threshold = 50.0f) {
            Assert.IsNotNull(callback, "GestureFlick : callback is null");

            _callback = callback;
            _time = time;
            _threshold = threshold;
            _sqrThreshold = _threshold * _threshold;

            _enabled = true; // 有効化.

            // コンストラクタでManagerに登録する設計.
            GestureManager.instance.AddFlick(this);
        }

        public override void Destroy() {
            base.Destroy();
            // 明示的に登録を削除する.
            GestureManager.instance.RemoveFlick(this);
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
                _timer = 0.0f;
            }

            if (id == _fingerId && phase == TouchPhase.Ended) {
                var now = touch.position;
                var diffPos = new Vector2(now.x - _startPos.x, now.y - _startPos.y);

                if (Check(diffPos, _timer)) {
                    _fingerId = InvalidFingerId;
                    _position = touch.position;
                    _callback();
                }
            }

            _timer += deltaTime;
        }

        public bool Check(Vector2 diffPos, float deltaTime) {
            if (deltaTime > _time) {
                return false;
            }

            return diffPos.sqrMagnitude > _sqrThreshold ? true : false;
        }
    }
}
