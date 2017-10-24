using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Gesture {
    public sealed class GestureTap : GestureBase {
        public GestureTap(UnityAction callback, float time = 0.0f, float threshold = 10.0f) {
            Assert.IsNotNull(callback, "GestureTap : callback is null");

            _callback = callback;
            _time = time; // タップと判定する時間(この時間より経過したらタップと判定しない).
            _threshold = threshold; // タップと判定する座標の閾値(この値よりズレたらタップと判定しない).
            _sqrThreshold = threshold * threshold;

            _enabled = true; // 有効化.

            // コンストラクタでManagerに登録する設計.
            GestureManager.instance.AddTap(this);
        }

        public override void Destroy() {
            // 明示的に登録を削除する.
            GestureManager.instance.RemoveTap(this);
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
                _timer = 0.0f;
            }

            if (phase == TouchPhase.Ended) {
                Vector2 now = touch.position;
                Vector2 diffPos = new Vector2(now.x - _startPos.x, now.y - _startPos.y);

                if (Check(diffPos, _timer)) {
                    _position = touch.position;
                    _callback();
                }
            }

            _timer += deltaTime;
        }

        /// <summary>
        /// タッチ判定メソッド.
        /// UnityEditor時用にpublic化.
        /// </summary>
        /// <param name="diffPos">タッチ開始座病からの差分</param>
        /// <param name="deltaTime">タッチ開始時間からの差分</param>
        /// <returns></returns>
        public bool Check(Vector2 diffPos, float deltaTime) {
            // 開始座標との位置差分を2乗の状態で判定.
            bool check = diffPos.sqrMagnitude > _sqrThreshold ? false : true;

            // 時間指定があるなら判定.
            if (check && _time > 0.0f) {
                check = deltaTime > _time ? false : true;
            }

            return check;
        }
    }
}
