using UnityEngine;
using UnityEngine.Events;
using Gesture;

namespace Common {

    public class ScreenToXZPlane : MonoBehaviour {
        private const float LowYFromCamera = 100.0f;

        [SerializeField]
        private Camera _camera;

        private UnityAction<Vector2, float> _onXZPlane;

        public UnityAction<Vector2, float> onXZPlane {
            set {
                _onXZPlane = value;
            }
        }

        private UnityAction _onXZPlaneEnd;

        public UnityAction onXZPlaneEnd {
            set {
                _onXZPlaneEnd = value;
            }
        }

        private GestureSwipe _gestureSwipe;

        private float _diagonalLen;

        void Start() {
            _gestureSwipe = new GestureSwipe(GestureSwiteCallback, stationary:true);

            if (_camera == null) {
                _camera = Camera.main;
            }

            var diagonal = new Vector2(Screen.width, Screen.height);

            _diagonalLen = diagonal.magnitude;
        }

        void OnDestroy() {
            _onXZPlane = null;
            _onXZPlaneEnd = null;

            if (_gestureSwipe != null) {
                _gestureSwipe.Destroy();
                _gestureSwipe = null;
            }
        }

        private void GestureSwiteCallback() {
            if (_gestureSwipe.swipe) {
                if (_gestureSwipe.end) {
                    if (_onXZPlaneEnd != null) {
                        _onXZPlaneEnd();
                    }
                }
                else {
                    if (_onXZPlane != null) {
                        Vector2 dir;
                        float power;

                        if (GetXZPlaneDirection(out dir, out power)) {
                            _onXZPlane(dir, power);
                        }
                    }
                }
            }
        }

        private bool GetXZPlaneDirection(out Vector2 direction, out float power) {
            direction = Vector2.zero;
            power = 0.0f;

            if (_camera == null) {
                return false;
            }

            var cameraTransform = _camera.transform;
            var cameraPos = cameraTransform.position;

            // 適当にカメラより低い平面(カメラは見下ろし前提).
            cameraPos.y -= LowYFromCamera;

            var plane = new Plane(Vector3.up, cameraPos);

            // スクリーン座標 -> World座標.
            var start = _gestureSwipe.startPos;
            var now = _gestureSwipe.position;
            var near = 1.0f;
            var startWorld = _camera.ScreenToWorldPoint(new Vector3(start.x, start.y, near));
            var nowWorld = _camera.ScreenToWorldPoint(new Vector3(now.x, now.y, near));

            // ドラック開始位置のRay.
            var cameraDir = cameraTransform.forward;
            var startRay = new Ray(startWorld, cameraDir);
            var enter = 0.0f;

            if (plane.Raycast(startRay, out enter)) {
                var startPlane = startRay.GetPoint(enter);
                var nowRay = new Ray(nowWorld, cameraDir); // ドラック現在位置のRay.

                if (plane.Raycast(nowRay, out enter)) {
                    var nowPlane = nowRay.GetPoint(enter);
                    var startXZ = new Vector2(startPlane.x, startPlane.z);
                    var nowXZ = new Vector2(nowPlane.x, nowPlane.z);
                    var worldDir = nowXZ - startXZ;

                    power = (now - start).magnitude / _diagonalLen;
                    direction = worldDir.normalized;

                    return true;
                }
            }

            return false;
        }
    }
}
