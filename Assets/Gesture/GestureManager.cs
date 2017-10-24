using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gesture {

    /// <summary>
    /// タッチ処理を行うクラス.簡易的なシングルトン設計.
    /// インスタンスのUpdateを毎Frameコールする必要あり.
    /// partialでファイル分割あり(GestureManagerEditor, GestureManagerIF).
    /// </summary>
    public partial class GestureManager {

        // 簡易的なシングルトンにする.
        private static GestureManager _instance;

        public static GestureManager instance {
            get {
                if (_instance == null) {
                    _instance = new GestureManager();
                }
                return _instance;
            }
        }

        private List<GestureTap>     _listTap;
        private List<GestureLongTap> _listLongTap;
        private List<GestureSwipe>   _listSwipe;
        private List<GestureFlick>   _listFlick;
        private List<GesturePinch>   _listPinch;

        private GestureManager() {
        }

        public void Update() {
#if UNITY_EDITOR
            MouseUpdate(); // UnityEditor実行時はマウスでシミュレートする.
            
#else
            TouchUpdate();
#endif
        }

        private void TouchUpdate() {
            int count = Input.touchCount;

            if (count == 0) {
                return;
            }

            EventSystem gui = EventSystem.current;
            float delta = Time.deltaTime;

            for (int i = 0; i < count; ++i) {
                Touch touch = Input.GetTouch(i);

                if (gui != null) {
                    if (gui.IsPointerOverGameObject(touch.fingerId)) {
                        continue;
                    }
                }

                SetTouch(ref touch, i, delta);
            }
        }

        private void SetTouch(ref Touch touch, int count, float delta) {
            if (_listTap != null) {
                for (int i = 0, len = _listTap.Count; i < len; ++i) {
                    _listTap[i].SetTouch(ref touch, count, delta);
                }
            }

            if (_listLongTap != null) {
                for (int i = 0, len = _listLongTap.Count; i < len; ++i) {
                    _listLongTap[i].SetTouch(ref touch, count, delta);
                }
            }

            if (_listSwipe != null) {
                for (int i = 0, len = _listSwipe.Count; i < len; ++i) {
                    _listSwipe[i].SetTouch(ref touch, count, delta);
                }
            }

            if (_listFlick != null) {
                for (int i = 0, len = _listFlick.Count; i < len; ++i) {
                    _listFlick[i].SetTouch(ref touch, count, delta);
                }
            }

            if (_listPinch != null) {
                for (int i = 0, len = _listPinch.Count; i < len; ++i) {
                    _listPinch[i].SetTouch(ref touch, count, delta);
                }
            }
        }
    }
}
