# Point Cloud Splitter (v5)
ply形式の点群データを分割して，minecraftのワールド内で再現（生成）することができます。

binary / ascii (little endian)の両者に対応しています（自動で識別します）。

データ（property）の並び順は`..., x, y, z, R, G, B, ...`と，x~Bまでは連続していると仮定しています。

# 使い方
SampleScene.unityを開き，`DATABASE`オブジェクトにアタッチされている，`DATABASE`コンポーネント・`Manager`コンポーネントをinspecterから確認してください。

両コンポーネントの各パラメータの説明を読んで，適切な値を設定してください（例外処理はほとんど含まれていないため，ミスがあるとエラーで強制停止となる場合があります）。

あとは実行するだけです。

※2025/11/15現在，分割した点群をunityのmesh assetに出力（変換）するプログラムは現在動作しません（`Active_export_mesh`をtrueにしても，中身では何も処理せずに即returnします）。過去に作ったプログラムをここに移行後，使えるようになる予定です。

# 開発環境
- Unity 6000.2.8f1
- Visual Studio Code

# 外部ライブラリ
- minecraft-client-csharp  
  License: GNU General Public License v3.0  
  License text: https://www.gnu.org/licenses/gpl-3.0.html
