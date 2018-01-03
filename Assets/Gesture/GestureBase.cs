using UnityEngine;
using UnityEngine.Events;

namespace Gesture {
    public abstract class GestureBase {
        // 全てのGestureで使用する変数(意味はGesture毎に違う).
        // Gesture固有の変数は継承先で宣言する.
        protected const int InvalidFingerId = -1;

        protected UnityAction _callback;

#if UNITY_EDITOR
        // UnityEditor時シミュレート用.
        public UnityAction callback {
            get {
                return _callback;
            }
        }
#endif

        protected Vector2 _position;

        public Vector2 position {
            get {
                return _position;
            }
#if UNITY_EDITOR
            // UnityEditor時シミュレート用.
            set {
                _position = value;
            }
#endif
        }

        protected Vector2 _startPos;

        public Vector2 startPos {
            get {
                return _startPos;
            }
#if UNITY_EDITOR
            // UnityEditor時シミュレート用.
            set {
                _startPos = value;
            }
#endif
        }

        protected float _threshold;

        public float threshold {
            get {
                return _threshold;
            }
            set {
                _threshold = value;
                _sqrThreshold = _threshold * _threshold;
            }
        }

        protected float _sqrThreshold;

        protected float _time;

        public float time {
            get {
                return _time;
            }
            set {
                _time = value; }
        }

        protected float _timer;

        protected int _fingerId = InvalidFingerId;

        protected bool _enabled; // 外部からの有効無効を設定する.

        public virtual bool enabled {
            get {
                return _enabled;
            }
            set {
                _enabled = value;
            }
        }

        public virtual void Destroy() {
            _callback = null;
        }

        public abstract void SetTouch(ref Touch touch, int count, float deltaTime);
    }
}
