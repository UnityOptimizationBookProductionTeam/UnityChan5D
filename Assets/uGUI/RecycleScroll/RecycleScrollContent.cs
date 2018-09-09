using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace uGUI {
    /// <summary>
    /// ・通常の UI Scroll View を拡張して軽量なスクロールリストを実現する.
    /// ・個々のリストはセル(Cell)と定義する.
    /// ・スクロールは縦か横のどちらかのみ.
    /// ・Viewportのサイズの自動取得はアンカーの指定等で複雑になるので数値を設定する方式.
    /// ・処理概要.
    ///   Viewportで見えている分だけのセル(+2)を生成(インスタンス化)して、
    ///   スクロール処理に合わせてセルを再処理(位置変更、内容更新)することで描画的な負荷を減らす.
    ///   (本来のScrollRectはスクロール内の全描画対象を作成しマスクで見える部分を制御している)
    /// </summary>
    public class RecycleScrollContent : UIBehaviour {

        [SerializeField, TooltipAttribute("対象のScrollRect")]
        private ScrollRect _scrollRect;

        [SerializeField, TooltipAttribute("ScrollRectのViewportの大きさを設定(縦か横のスクロールにしか対応しません)")]
        private float _viewportSize;

        [SerializeField, TooltipAttribute("スクロール内のセルのオリジナル(Instantiateされます)")]
        private RectTransform _cell;

        [SerializeField, TooltipAttribute("セル(スクロールリスト)の総数")]
        private int _cellNum;

        [SerializeField, TooltipAttribute("セル間のスペーシングサイズ")]
        private float _spacingSize;

        private int _cellInstanceNum; // 内部で使用するリサイクル用のインスタンス化するセル数.

        private int _currentCellIndex; // 見えているセルの先頭インデックス値(スクロールにより可変).
        private int _lastCellIndex; // セルの最終インデックス値(不変、_cellNum値ではない点に注意).

        private float _cellSize; // セルのサイズ(縦か横のスクロール指定で扱う方向が変わる).

        private bool _isVertical; // スクロールの方向が縦か(違う場合は横).

        private RectTransform _content; // ScrollRectのContent領域.

        private LinkedList<RectTransform> _cellList; // セルの追加削除を頻繁に行うためリンクリスト(LinkedList)にする(処理速度優先).

        protected override void Start() {
            // 処理の条件と仕様をチェック.
            if (_scrollRect == null || _scrollRect.vertical == _scrollRect.horizontal || _cell == null || _cellNum <= 0) {
                Debug.LogError("RecycleScrollContent : 設定エラー");
                return;
            }

            // スクロール方向をキャッシュする.
            _isVertical = _scrollRect.vertical;

            // ScrollRectのContentにAddComponentされている前提.
            _content = gameObject.transform as RectTransform;

            // cellのサイズと数からコンテンツ領域のサイズを算出.
            var contentSize = _content.sizeDelta;
            var cellSize = _cell.sizeDelta;
            var cellRange = 0.0f;

            if (_isVertical) {
                _cellSize = cellSize.y;
                cellRange = _cellSize + _spacingSize;
                contentSize.y = cellRange * _cellNum;
            }
            else {
                _cellSize = cellSize.x;
                cellRange = _cellSize + _spacingSize;
                contentSize.x = cellRange * _cellNum;
            }

            // ScrollRectを機能させるためにコンテンツ領域(スクロール)サイズをセットする.
            _content.sizeDelta = contentSize;

            // viewportで見えているセルの数.
            var visibleNum = Mathf.CeilToInt(_viewportSize / cellRange);

            // viewportのサイズからcellのInstantiateを行う(見えている数 +2がリサイクルするには必要).
            _cellInstanceNum = visibleNum + 2;

            // セルインデックスの最後(見える分を引く).
            _lastCellIndex = _cellNum - visibleNum;

            // リサイクル処理用にInstantiateするセルをリンクリストにする.
            _cellList = new LinkedList<RectTransform>();

            // オリジナルのセルはコピーするだけ.
            _cell.gameObject.SetActive(false);

            for (var i = 0; i < _cellInstanceNum; ++i) {
                var clone = Instantiate<RectTransform>(_cell, _content);
                var gameObject = clone.gameObject;

                gameObject.SetActive(true);
                clone.anchoredPosition = _isVertical ? new Vector2(0.0f, -cellRange * i) : new Vector2(cellRange * i, 0.0f);
                _cellList.AddLast(clone);

                // セルの初期化処理.
                var iface = gameObject.GetComponent<IRecycleScrollCell>();

                iface.InitCell(i);
            }
        }

        void Update() {
            // セルのリサイクル処理を行うかチェック.
            if (_content == null) {
                return;
            }

            // 今のセルのインデックスを算出.
            var now = _content.anchoredPosition;
            var pos = _isVertical ? now.y : -now.x;
            var cellIndex = 0;
            var cellRange = _cellSize + _spacingSize;

            if (pos > 0.0f) {
                cellIndex = Mathf.FloorToInt(pos / cellRange);

                if (cellIndex > _lastCellIndex) {
                    cellIndex = _lastCellIndex;
                }
            }

            // 前回のセルのインデックスとの差分でセルをリサイクルする.
            var diffIndex = cellIndex - _currentCellIndex;

            if (diffIndex > 0) {
                // スクロールが進んでいる.
                var startIndex = _currentCellIndex + _cellInstanceNum;

                for (var i = 0; i < diffIndex; ++i) {
                    // 先頭のセルを最後尾のセルにする.
                    var first = _cellList.First.Value;
                    var last = _cellList.Last.Value.anchoredPosition;

                    first.anchoredPosition = _isVertical ? new Vector2(last.x, last.y - cellRange) : new Vector2(last.x + cellRange, last.y);

                    _cellList.RemoveFirst();
                    _cellList.AddLast(first);

                    var index = startIndex + i;
                    var gobject = first.gameObject;

                    // セルの総数を超えるなら消す.
                    if (index >= _cellNum) {
                        gobject.SetActive(false);
                    }
                    else {
                        // セルの更新.
                        if (!gobject.activeInHierarchy) {
                            gobject.SetActive(true);
                        }

                        var iface = gobject.GetComponent<IRecycleScrollCell>();

                        iface.UpdateCell(index);
                    }
                }
            }
            else if (diffIndex < 0) {
                // スクロールが戻っている.
                var startIndex = _currentCellIndex - 1;

                diffIndex = -diffIndex; // ループ用に正の数にする.

                for (var i = 0; i < diffIndex; ++i) {
                    // 最後尾のセルを先頭のセルにする.
                    var first = _cellList.First.Value.anchoredPosition;
                    var last = _cellList.Last.Value;

                    last.anchoredPosition = _isVertical ? new Vector2(first.x, first.y + cellRange) : new Vector2(first.x - cellRange, first.y);

                    _cellList.RemoveLast();
                    _cellList.AddFirst(last);

                    var index = startIndex - i;
                    var gobject = last.gameObject;

                    // 0より前は処理しない.
                    if (index < 0) {
                        gobject.SetActive(false);
                    }
                    else {
                        // セルの更新.
                        if (!gobject.activeInHierarchy) {
                            gobject.SetActive(true);
                        }

                        var iface = gobject.GetComponent<IRecycleScrollCell>();

                        iface.UpdateCell(index);
                    }
                }
            }

            // 処理した分のセルインデックスにする.
            _currentCellIndex = cellIndex;
        }
    }
}