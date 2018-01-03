using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Gesture {
    public class GesturePhase : GestureBase {
        private UnityAction _callbackBegan;
        private UnityAction _callbackMoved;
        private UnityAction _callbackStationary;
        private UnityAction _callbackEnded;
        private UnityAction _callbackCanceled;

#if UNITY_EDITOR
        public UnityAction callbackBegan {
            get {
                return _callbackBegan;
            }
        }
        public UnityAction callbackMoved {
            get {
                return _callbackMoved;
            }
        }
        public UnityAction callbackStationary {
            get {
                return _callbackStationary;
            }
        }
        public UnityAction callbackEnded {
            get {
                return _callbackEnded;
            }
        }
        public UnityAction callbackCanceled {
            get {
                return _callbackCanceled;
            }
        }
#endif

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

        public GesturePhase(UnityAction callbackBegan,
                            UnityAction callbackMoved,
                            UnityAction callbackStationary,
                            UnityAction callbackEnded,
                            UnityAction callbackCanceled) {
            _callbackBegan = callbackBegan;
            _callbackMoved = callbackMoved;
            _callbackStationary = callbackStationary;
            _callbackEnded = callbackEnded;
            _callbackCanceled = callbackCanceled;

            _enabled = true; // 有効化.

            // コンストラクタでManagerに登録する設計.
            GestureManager.instance.AddPhase(this);
        }

        public override void Destroy() {
            base.Destroy();
            // 明示的に登録を削除する.
            GestureManager.instance.RemovePhase(this);
        }

        public override void SetTouch(ref Touch touch, int count, float deltaTime) {
            // 無効指定されているなら処理しない.
            if (!_enabled) {
                return;
            }

            var phase = touch.phase;
            var now = touch.position;
            var id = touch.fingerId;

            _deltaPos = touch.deltaPosition;

            if (phase == TouchPhase.Began && _fingerId == InvalidFingerId) {
                _fingerId = id;

                if (_callbackBegan != null) {
                    _position = now;
                    _callbackBegan();
                }
            }
            else if (_fingerId == id) {
                switch (phase) {
                case TouchPhase.Moved:
                    if (_callbackMoved != null) {
                        _position = now;
                        _callbackMoved();
                    }
                    break;

                case TouchPhase.Stationary:
                    if (_callbackStationary != null) {
                        _position = now;
                        _callbackStationary();
                    }
                    break;

                case TouchPhase.Ended:
                    _fingerId = InvalidFingerId;

                    if (_callbackEnded != null) {
                        _position = now;
                        _callbackEnded();
                    }
                    break;

                case TouchPhase.Canceled:
                    _fingerId = InvalidFingerId;

                    if (_callbackCanceled != null) {
                        _position = now;
                        _callbackCanceled();
                    }
                    break;

                default:
                    break;
                }
            }
        }
    }
}
