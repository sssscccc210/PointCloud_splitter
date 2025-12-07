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

    private readonly static Dictionary<string, byte> SizeOfTypes = new()
    {
        /*
        char       character                 1
        uchar      unsigned character        1
        short      short integer             2
        ushort     unsigned short integer    2
        int        integer                   4
        uint       unsigned integer          4
        float      single-precision float    4
        double     double-precision float    8 
        */
        {"char", 1 },
        {"uchar", 1 },
        {"short", 2 },
        {"ushort", 2 },
        {"int", 4 },
        {"uint", 4 },
        {"float", 4 },
        {"double", 8 }
    };

    public struct Point
    {
        public float x, y, z; // 4 bytes
        public byte r, g, b; // 1 byte
        public float _l, _a, _b; // 4 bytes
    }


    public PointCloudData()
    {
        maxX = maxY = maxZ = -Mathf.Infinity;
        minX = minY = minZ = Mathf.Infinity;
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
    private void CheckMaxAndMin(Point p)
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
        if (!File.Exists(db.originalPath))
        {
            throw new FileNotFoundException("指定されたファイルが見つかりません。", db.originalPath);
        }

        // FileStream を開く。これがファイルの生データへのアクセスを提供する。
        FileStream fs = new FileStream(db.originalPath, FileMode.Open, FileAccess.Read);


        // ヘッダー部を読み取る
        bool isAscii = false;
        int vertexCount = -1;
        int countBeforeX = 0;
        int bytesBeforeX = 0;
        int bytesAfterBlue = 0;

        // 第5引数の `leaveOpen` を true に設定すると，StreamReaderを閉じても，underlying (元の) FileStreamは開いたままにできる。
        using (StreamReader reader = new StreamReader(fs, Encoding.ASCII, true, 1024, true))
        {
            string tmp;
            string[] tmpCell;// tmpをさらに' 'で区切るときの格納先
            bool isBeforeX = true;
            while ( !( tmp = reader.ReadLine() ).Contains("end_header") )
            {
                if (tmp.StartsWith("format"))
                {
                    if (tmp.Contains("ascii")) isAscii = true;
                    else if (tmp.Contains("binary_big_endian"))
                    {
                        Debug.LogError("big endianなファイルには対応していません。");
                        return;
                    }
                    // binary_little_endianならisAscii = falseですでに初期化の値
                }
                else if (tmp.StartsWith("element vertex"))
                {
                    vertexCount = int.Parse(tmp.Split(' ')[2]);
                }
                else if (tmp.StartsWith("property"))
                {
                    tmpCell = tmp.Split(' ');
                    if (tmpCell[2] == "x")
                    {
                        // x, y, z, r, g, bは連続する（という仮定）ので，スキップ
                        for(int i = 0; i < 5; ++i) reader.ReadLine();
                        isBeforeX = false;
                    }
                    else
                    {
                        // xに対するpropertyで無い場合
                        if (isBeforeX){
                            bytesBeforeX += SizeOfTypes[tmpCell[1]];
                            countBeforeX++;
                        }
                        else bytesAfterBlue += SizeOfTypes[tmpCell[1]];
                    }
                }
            }

            if (vertexCount <= 0)
            {
                Debug.Log("plyから頂点の数が読み取れませんでした");
                return;
            }
            else Debug.Log($"{vertexCount}個の点を読み込みます");


            if (isAscii)
            {
                // binaryの場合のデータ読み込みはこの後，別で行う

                // 本体を読んでいく
                for (int i = 0; i < vertexCount; ++i)
                {
                    // 点群 ⇄ minecraft: x ⇄ z, y ⇄ x, z ⇄ y
                    Point p = new();
                    string[] cell = reader.ReadLine().Split(' ');
                    p.z = float.Parse(cell[countBeforeX]);
                    p.x = float.Parse(cell[countBeforeX + 1]);
                    p.y = float.Parse(cell[countBeforeX + 2]);
                    p.r = byte.Parse(cell[countBeforeX + 3]);
                    p.g = byte.Parse(cell[countBeforeX + 4]);
                    p.b = byte.Parse(cell[countBeforeX + 5]);


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
            
        }

        
        // binaryなら，別途readerを用意してデータの中身を見ていく
        if (!isAscii)
        {
            fs.Position = 0;// 読み込み位置を0にする
            using (BinaryReader reader = new BinaryReader(fs))
            {
                string tmp;
                char c;
                // ヘッダー部を読み飛ばす
                do
                {
                    tmp = "";
                    while ((c = reader.ReadChar()) != '\n') tmp += c;

                } while (tmp != "end_header");


                // 本体を読んでいく
                for (int i = 0; i < vertexCount; ++i)
                {
                    // 点群 ⇄ minecraft: x ⇄ z, y ⇄ x, z ⇄ y
                    Point p = new();
                    reader.ReadBytes(bytesBeforeX);
                    p.z = reader.ReadSingle();
                    p.x = reader.ReadSingle();
                    p.y = reader.ReadSingle();
                    p.r = reader.ReadByte();
                    p.g = reader.ReadByte();
                    p.b = reader.ReadByte();
                    reader.ReadBytes(bytesAfterBlue);


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
