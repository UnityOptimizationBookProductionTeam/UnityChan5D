
namespace uGUI {
    /// <summary>
    /// RecycleScrollContent用のセルのインターフェイス.
    /// セルの初期化と更新を実装する.
    /// 初期化(InitCell)はセルが生成(Instantiate)した時のみ呼び出されます.
    /// 更新(UpdateCell)はスクロール内で表示される時に呼び出されます.
    /// それぞれの引数のインデックス値はスクロールリスト内のインデックス値です.
    /// </summary>
    public interface IRecycleScrollCell {
        void InitCell(int index);
        void UpdateCell(int index);
    }
}
