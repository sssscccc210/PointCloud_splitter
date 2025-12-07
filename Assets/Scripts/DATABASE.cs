using UnityEngine;

public class DATABASE : MonoBehaviour
{
    [Header("↓plyファイルのフルパス", order = 1)]
    public string originalPath;// 点群データ（txt or ply）があるフォルダ

    [Header("↓Minecraft RCONのipアドレス", order = 2)]
    public string rconIPAddress;

    [Header("↓Minecraft RCONのポート", order = 3)]
    public int rconPort;

    [Header("↓Minecraft RCONのパスワード", order = 4)]
    public string rconPassword;

    [Header("↓区切る際の1ブロックの範囲", order = 5)]
    public float splitRange = 5;// 1meshの大きさ

    [Header("↓1ブロックあたりに存在すべき点の数（ブロックとしての認識閾値）", order = 6)]
    public float existenceThreshold = 700;

    [Header("↓生成位置（基点）", order = 7)]
    public Vector3 generatePosition = Vector3.down * 60;

    [Header("↓Mesh作成フォルダ名（Assets/XXX）のXXX部分", order = 8)]
    public string meshAssetName;// 連番前の名前兼メッシュ作成用フォルダ名（sc_ex, 2j_exなど）

    [Header("↓LOWの時の間引き率", order = 9)]
    public int mabikiRate = 5;// LOWの間引き率
}
