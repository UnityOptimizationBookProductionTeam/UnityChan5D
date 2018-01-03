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

        private List<GestureTap> _listTap;
        private List<GestureLongTap> _listLongTap;
        private List<GestureSwipe> _listSwipe;
        private List<GestureFlick> _listFlick;
        private List<GesturePinch> _listPinch;
        private List<GesturePhase> _listPhase;

        private GestureManager() {
        }

        public void Destroy() {
            InternalDestroy<GestureTap>(ref _listTap);
            InternalDestroy<GestureLongTap>(ref _listLongTap);
            InternalDestroy<GestureSwipe>(ref _listSwipe);
            InternalDestroy<GestureFlick>(ref _listFlick);
            InternalDestroy<GesturePinch>(ref _listPinch);
            InternalDestroy<GesturePhase>(ref _listPhase);
        }

        public void Update() {
#if UNITY_EDITOR
            MouseUpdate(); // UnityEditor実行時はマウスでシミュレートする.

#else
            TouchUpdate();
#endif
        }

        private void TouchUpdate() {
            var count = Input.touchCount;

            if (count == 0) {
                return;
            }

            var gui = EventSystem.current;
            var delta = Time.deltaTime;

            for (var i = 0; i < count; ++i) {
                var touch = Input.GetTouch(i);

                if (gui != null) {
                    if (gui.IsPointerOverGameObject(touch.fingerId)) {
                        continue;
                    }
                }

                SetTouch(ref touch, i, delta);
            }
        }

        private void SetTouch(ref Touch touch, int count, float delta) {
            InternalSetTouch<GestureTap>(ref _listTap, ref touch, count, delta);
            InternalSetTouch<GestureLongTap>(ref _listLongTap, ref touch, count, delta);
            InternalSetTouch<GestureSwipe>(ref _listSwipe, ref touch, count, delta);
            InternalSetTouch<GestureFlick>(ref _listFlick, ref touch, count, delta);
            InternalSetTouch<GesturePinch>(ref _listPinch, ref touch, count, delta);
            InternalSetTouch<GesturePhase>(ref _listPhase, ref touch, count, delta);
        }

        private void InternalSetTouch<T>(ref List<T> list, ref Touch touch, int count, float delta) where T : GestureBase {
            if (list != null) {
                for (int i = 0, len = list.Count; i < len; ++i) {
                    list[i].SetTouch(ref touch, count, delta);
                }
            }
        }

        private void InternalDestroy<T>(ref List<T> list) where T : GestureBase {
            if (list != null) {
                for (int i = 0, len = list.Count; i < len; ++i) {
                    list[i].Destroy();
                }
                list.Clear();
                list = null;
            }
        }
    }
}
