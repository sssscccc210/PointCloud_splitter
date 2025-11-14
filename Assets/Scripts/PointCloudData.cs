using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class PointCloudData : MonoBehaviour
{
    List<Point> allData = new(); // とりあえず読んだ各点を全部格納するリスト
    public List<Point>[,,] blockedAllData;
    float maxX, minX, maxY, minY, maxZ, minZ;
    public int sizeOfBlockedAllDataX, sizeOfBlockedAllDataY, sizeOfBlockedAllDataZ;



    public struct Point
    {
        public float x, y, z; // 4 bytes
        public byte r, g, b; // 1 byte
        public float _l, _a, _b; // 4 bytes
    }

    /// <summary>
    /// static: 与えられたPoint構造体について，情報を表示する。
    /// </summary>
    public static void PrintPointInfo(Point p)
    {
        Debug.Log($"x: {p.x}, y: {p.y}, z: {p.z}, r: {p.r}, g: {p.g}, b: {p.b}");
    }

    /// <summary>
    /// 与えられたPoint構造体について，x, y, zが現在の最大・最小を更新するなら更新する。
    /// </summary>
    void CheckMaxAndMin(Point p)
    {
        if (p.x > maxX) maxX = p.x;
        else if (p.x < minX) minX = p.x;
        if (p.y > maxY) maxY = p.y;
        else if (p.y < minY) minY = p.y;
        if (p.z > maxZ) maxZ = p.z;
        else if (p.z < minZ) minZ = p.z;
    }

    /// <summary>
    /// PLYを読み込む。
    /// 全ての点を読み込み，ユーザ指定のサイズ（splitRange）でそれらを区切ってデータをblockedAllDataに格納する。
    /// </summary>
    /// <param name="db">DATABASEインスタンス（必要なパラメータが格納されている）</param>
    public void LoadPly(DATABASE db)
    {
        int vertexCount = -1;

        if (!File.Exists(db.originalPath))
        {
            throw new FileNotFoundException("指定されたファイルが見つかりません。", db.originalPath);
        }

        // FileStream を開く。これがファイルの生データへのアクセスを提供する。
        FileStream fs = new FileStream(db.originalPath, FileMode.Open, FileAccess.Read);

        using (BinaryReader reader = new BinaryReader(fs))// 今度はバイナリを読む
        {
            string tmp;
            char c;
            // ヘッダー部を読み飛ばす（途中，vertexCountを見る）
            do
            {
                tmp = "";
                while ((c = reader.ReadChar()) != '\n') tmp += c;
                Debug.Log(tmp);

                if(tmp.Contains("element vertex"))
                {
                    vertexCount = int.Parse(tmp.Split(' ')[2]);
                }
            } while (tmp != "end_header");

            if (vertexCount <= 0)
            {
                Debug.Log("plyから頂点の数が読み取れませんでした");
                return;
            }
            

            Debug.Log($"{vertexCount}個の点を読み込みます");
            // 本体を読んでいく
            for (int i = 0; i < vertexCount; ++i)
            {

                Point p = new();
                p.x = reader.ReadSingle();
                p.z = reader.ReadSingle();// yとzは反転している
                p.y = reader.ReadSingle();
                p.r = reader.ReadByte();
                p.g = reader.ReadByte();
                p.b = reader.ReadByte();
                reader.ReadByte();// 以下，読み飛ばし部分。今は自分で手打ち
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();

                // rgb値からL*a*b*空間の値も求めておく
                (float l, float a, float b) t = ColorUtility.Srgb2Lab(p.r, p.g, p.b);
                p._l = t.l;
                p._a = t.a;
                p._b = t.b;

                //if(i % 500 == 0) PrintPointInfo(p);
                CheckMaxAndMin(p);// 最大値・最小値を更新可能なら，更新する
                allData.Add(p);
            }
        }

        // 全ての点がallDataに格納された＆サイズが分かるので，各区間に再配置

        // PointのListを格納する配列をサイズ分用意
        sizeOfBlockedAllDataX = Mathf.CeilToInt((maxX - minX) / db.splitRange);
        sizeOfBlockedAllDataY = Mathf.CeilToInt((maxY - minY) / db.splitRange);
        sizeOfBlockedAllDataZ = Mathf.CeilToInt((maxZ - minZ) / db.splitRange);
        Debug.Log($"point cloud box size: {sizeOfBlockedAllDataX}, {sizeOfBlockedAllDataY}, {sizeOfBlockedAllDataZ}");
        blockedAllData = new List<Point>[sizeOfBlockedAllDataX, sizeOfBlockedAllDataY, sizeOfBlockedAllDataZ];

        // 中身をList<Point>で初期化しないとね
        for (int x = 0; x < sizeOfBlockedAllDataX; x++)
        {
            for (int y = 0; y < sizeOfBlockedAllDataY; y++)
            {
                for (int z = 0; z < sizeOfBlockedAllDataZ; z++)
                {
                    blockedAllData[x, y, z] = new List<Point>();
                }
            }
        }

        // 対応する要素に全てのPointを振り分ける
        allData.ForEach((Point p) =>
        {
            blockedAllData[
                Mathf.FloorToInt((p.x - minX) / db.splitRange),
                Mathf.FloorToInt((p.y - minY) / db.splitRange),
                Mathf.FloorToInt((p.z - minZ) / db.splitRange)
            ].Add(p);
        });

        // allDataのメモリを解放したければ以下を入れておく
        allData = null;

        return;
    }
}
