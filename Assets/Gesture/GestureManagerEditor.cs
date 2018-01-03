using UnityEngine;
using UnityEngine.EventSystems;

namespace Gesture {

    /// <summary>
    /// partialで分割したGestureManagerクラス.
    /// UnityEditor時のシミュレートを実装.
    /// </summary>
    public partial class GestureManager {
#if UNITY_EDITOR
        private Vector3 _startPos;
        private Vector3 _lastPos;
        private float _time;
        private bool _mouseDown;

        private void MouseUpdate() {
            var gui = EventSystem.current;

            if (gui != null) {
                if (gui.IsPointerOverGameObject()) {
                    return;
                }
            }

            if (Input.GetMouseButtonDown(0)) {
                _mouseDown = true;

                _startPos = Input.mousePosition;
                _time = 0.0f;

                if (_listLongTap != null) { // ロングタップの初期化.
                    for (int i = 0, len = _listLongTap.Count; i < len; ++i) {
                        _listLongTap[i].longTap = false;
                    }
                }

                if (_listSwipe != null) { // スワイプの初期化.
                    for (int i = 0, len = _listSwipe.Count; i < len; ++i) {
                        _listSwipe[i].swipe = false;
                    }
                }

                if (_listPhase != null) {
                    var zero = Vector2.zero;

                    for (int i = 0, len = _listPhase.Count; i < len; ++i) {
                        if (_listPhase[i].callbackBegan != null) {
                            _listPhase[i].position = _startPos;
                            _listPhase[i].startPos = _startPos;
                            _listPhase[i].deltaPos = zero;
                            _listPhase[i].callbackBegan();
                        }
                    }
                }
            }

            if (Input.GetMouseButtonUp(0)) {
                _mouseDown = false;

                CheckTap();
                CheckSwipe(true);
                CheckFlick();
                CheckPhaseEnded();
            }

            if (_mouseDown) {
                CheckLongTap();
                CheckSwipe(false);
                CheckPhaseMoved();
            }

            CheckPinch(Input.GetAxis("Mouse ScrollWheel"));

            _lastPos = Input.mousePosition;
            _time += Time.deltaTime;
        }

        private void GetPosition(out Vector2 now, out Vector2 diff) {
            var pos = Input.mousePosition;

            now = new Vector2(pos.x, pos.y);
            diff = new Vector2(pos.x - _startPos.x, pos.y - _startPos.y);
        }

        private void CheckTap() {
            if (_listTap == null) {
                return;
            }

            Vector2 touchPos, diffPos;

            GetPosition(out touchPos, out diffPos);

            for (int i = 0, len = _listTap.Count; i < len; ++i) {
                var tap = _listTap[i];

                if (tap.enabled) {
                    if (tap.Check(diffPos, _time)) {
                        tap.position = touchPos;

                        tap.callback();
                    }
                }
            }
        }

        private void CheckLongTap() {
            if (_listLongTap == null) {
                return;
            }

            Vector2 touchPos, diffPos;

            GetPosition(out touchPos, out diffPos);

            for (int i = 0, len = _listLongTap.Count; i < len; ++i) {
                var tap = _listLongTap[i];

                if (!tap.longTap && tap.enabled) {
                    if (tap.Check(diffPos, _time)) {
                        tap.longTap = true;
                        tap.position = touchPos;

                        tap.callback();
                    }
                }
            }
        }

        private void CheckSwipe(bool end) {
            if (_listSwipe == null) {
                return;
            }

            var pos = Input.mousePosition;
            var touchPos = new Vector2(pos.x, pos.y);
            var deltaPos = new Vector2(pos.x - _lastPos.x, pos.y - _lastPos.y);
            var diffPos = new Vector2(pos.x - _startPos.x, pos.y - _startPos.y);
            var stationary = (deltaPos.sqrMagnitude < 1.0f) ? true : false;

            for (int i = 0, len = _listSwipe.Count; i < len; ++i) {
                var swipe = _listSwipe[i];

                if (swipe.enabled) {
                    if (!end && stationary && !swipe.stationary) {
                        continue;
                    }

                    if (swipe.Check(diffPos)) {
                        swipe.startPos = _startPos;
                        swipe.position = touchPos;
                        swipe.deltaPos = deltaPos;
                        swipe.end = end;

                        swipe.callback();
                    }
                }
            }
        }

        private void CheckFlick() {
            if (_listFlick == null) {
                return;
            }

            Vector2 touchPos, diffPos;

            GetPosition(out touchPos, out diffPos);

            for (int i = 0, len = _listFlick.Count; i < len; ++i) {
                var flick = _listFlick[i];

                if (flick.enabled) {
                    if (flick.Check(diffPos, _time)) {
                        flick.position = touchPos;

                        flick.callback();
                    }
                }
            }
        }

        private void CheckPinch(float wheelAxis) {
            if (_listPinch == null) {
                return;
            }

            for (int i = 0, len = _listPinch.Count; i < len; ++i) {
                var pinch = _listPinch[i];

                pinch._wheel += wheelAxis;

                if (pinch.pinchIn) {
                    if (pinch.threshold < pinch._wheel) {
                        pinch._wheel = 0.0f;
                        pinch.callback();
                    }
                }
                else {
                    if (pinch.threshold < -pinch._wheel) {
                        pinch._wheel = 0.0f;
                        pinch.callback();
                    }
                }
            }
        }

        private void CheckPhaseEnded() {
            if (_listPhase == null) {
                return;
            }

            Vector2 touchPos, diffPos;

            GetPosition(out touchPos, out diffPos);

            for (int i = 0, len = _listPhase.Count; i < len; ++i) {
                if (_listPhase[i].callbackEnded != null) {
                    _listPhase[i].position = touchPos;
                    _listPhase[i].deltaPos = diffPos;
                    _listPhase[i].callbackEnded();
                }
            }
        }

        private void CheckPhaseMoved() {
            if (_listPhase == null) {
                return;
            }

            Vector2 touchPos, diffPos;

            GetPosition(out touchPos, out diffPos);

            if (diffPos == Vector2.zero) {
                for (int i = 0, len = _listPhase.Count; i < len; ++i) {
                    if (_listPhase[i].callbackStationary != null) {
                        _listPhase[i].position = touchPos;
                        _listPhase[i].deltaPos = diffPos;
                        _listPhase[i].callbackStationary();
                    }
                }
            }
            else {
                for (int i = 0, len = _listPhase.Count; i < len; ++i) {
                    if (_listPhase[i].callbackMoved != null) {
                        _listPhase[i].position = touchPos;
                        _listPhase[i].deltaPos = diffPos;
                        _listPhase[i].callbackMoved();
                    }
                }
            }
        }
#endif
    }
}