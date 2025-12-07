using UnityEngine;
using MinecraftClientPack;
using System.Collections.Generic;

public class PCS_MinecraftExp : MonoBehaviour
{
    // マイクラのblock情報を入れるための構造体
    public struct BlockColorInfo
    {
        public string technicalName;
        public byte r, g, b;// rgb色系（linear(≠sRGB)であると仮定）
        public float _l, _a, _b;// L*a*b* 色系 

        public BlockColorInfo(string technicalName, byte r, byte g, byte b)
        {
            this.technicalName = technicalName;
            this.r = r;
            this.g = g;
            this.b = b;

            (float l, float a, float b) t = ColorUtility.Srgb2Lab(r, g, b);
            this._l = t.l;
            this._a = t.a;
            this._b = t.b;
        }
    }


    // マップカラーに基づいたブロック色のリスト【色データはGeminiが生成】
    public static readonly List<BlockColorInfo> MinecraftBlockColors = new List<BlockColorInfo>
    {
        // --- 羊毛 (Wool) --- (Minecraft Wiki: "new" colors)
        new BlockColorInfo("minecraft:white_wool", 233, 236, 236),     // #E9ECEC
        new BlockColorInfo("minecraft:light_gray_wool", 142, 142, 134),// #8E8E86
        new BlockColorInfo("minecraft:gray_wool", 62, 68, 71),        // #3E4447
        new BlockColorInfo("minecraft:black_wool", 20, 21, 25),       // #141519
        new BlockColorInfo("minecraft:brown_wool", 114, 71, 40),     // #724728
        new BlockColorInfo("minecraft:red_wool", 161, 39, 34),       // #A12722
        new BlockColorInfo("minecraft:orange_wool", 240, 118, 19),   // #F07613
        new BlockColorInfo("minecraft:yellow_wool", 248, 198, 39),   // #F8C627
        new BlockColorInfo("minecraft:lime_wool", 112, 185, 25),     // #70B919
        new BlockColorInfo("minecraft:green_wool", 84, 109, 27),     // #546D1B
        new BlockColorInfo("minecraft:cyan_wool", 21, 137, 145),     // #158991
        new BlockColorInfo("minecraft:light_blue_wool", 58, 175, 217),// #3AAFD9
        new BlockColorInfo("minecraft:blue_wool", 53, 57, 157),      // #35399D
        new BlockColorInfo("minecraft:purple_wool", 121, 42, 172),   // #792AAC
        new BlockColorInfo("minecraft:magenta_wool", 189, 68, 179),  // #BD44B3
        new BlockColorInfo("minecraft:pink_wool", 237, 141, 172),    // #ED8DAC

        // --- テラコッタ (Terracotta) --- (MCreator / map-color ベース)
        new BlockColorInfo("minecraft:white_terracotta", 209, 177, 161),  //  D1 B1 A1
        new BlockColorInfo("minecraft:light_gray_terracotta", 135, 107, 98),// 87 6B 62
        new BlockColorInfo("minecraft:gray_terracotta", 57, 41, 35),       // 39 29 23
        new BlockColorInfo("minecraft:black_terracotta", 37, 22, 16),      // 25 16 10
        new BlockColorInfo("minecraft:brown_terracotta", 76, 50, 35),      // 4C 32 23
        new BlockColorInfo("minecraft:red_terracotta", 142, 60, 46),       // 8E 3C 2E
        new BlockColorInfo("minecraft:orange_terracotta", 159, 82, 36),    // 9F 52 24
        new BlockColorInfo("minecraft:yellow_terracotta", 186, 133, 36),   // BA 85 24
        new BlockColorInfo("minecraft:lime_terracotta", 103, 117, 53),     // 67 75 35
        new BlockColorInfo("minecraft:green_terracotta", 76, 82, 42),      // 4C 52 2A
        new BlockColorInfo("minecraft:cyan_terracotta", 87, 92, 92),       // 57 5C 5C
        new BlockColorInfo("minecraft:light_blue_terracotta", 112, 108, 138),// 70 6C 8A
        new BlockColorInfo("minecraft:blue_terracotta", 76, 62, 92),       // 4C 3E 5C
        new BlockColorInfo("minecraft:purple_terracotta", 122, 73, 88),    // 7A 49 58
        new BlockColorInfo("minecraft:magenta_terracotta", 149, 87, 108),  // 95 57 6C
        new BlockColorInfo("minecraft:pink_terracotta", 160, 77, 78),      // A0 4D 4E

        // --- コンクリート (Concrete) --- (コミュニティパレット参考: Lospec)
        new BlockColorInfo("minecraft:white_concrete", 207, 213, 214),    // #CFD5D6
        new BlockColorInfo("minecraft:light_gray_concrete", 125, 125, 115),// #7D7D73
        new BlockColorInfo("minecraft:gray_concrete", 55, 58, 62),        // #373A3E
        new BlockColorInfo("minecraft:black_concrete", 8, 10, 15),        // #080A0F
        new BlockColorInfo("minecraft:brown_concrete", 96, 60, 32),       // #603C20
        new BlockColorInfo("minecraft:red_concrete", 142, 33, 33),        // #8E2121
        new BlockColorInfo("minecraft:orange_concrete", 224, 97, 1),      // #E06101
        new BlockColorInfo("minecraft:yellow_concrete", 241, 175, 21),    // #F1AF15
        new BlockColorInfo("minecraft:lime_concrete", 94, 169, 24),       // #5EA918
        new BlockColorInfo("minecraft:green_concrete", 73, 91, 36),       // #495B24
        new BlockColorInfo("minecraft:cyan_concrete", 21, 119, 136),      // #157788
        new BlockColorInfo("minecraft:light_blue_concrete", 36, 137, 199),// #2489C7
        new BlockColorInfo("minecraft:blue_concrete", 45, 47, 143),       // #2D2F8F
        new BlockColorInfo("minecraft:purple_concrete", 100, 32, 156),    // #64209C
        new BlockColorInfo("minecraft:magenta_concrete", 169, 48, 159),   // #A9309F
        new BlockColorInfo("minecraft:pink_concrete", 213, 101, 143)      // #D5658F
    };


    // blockの配置をする関数
    public static void SetBlock(MinecraftClient client, Vector3 pos, string blockName = "minecraft:air")
    {
        Message resp;

        if (!client.SendCommand($"setblock {(int)pos.x} {(int)pos.y} {(int)pos.z} {blockName}", out resp))
        {
            Debug.Log("SetBlockのコマンド実行に失敗しました。");
        }
        else
        {
            Debug.Log("SetBlockからの返事：" + resp.Body);
        }
    }


    /// <summary>
    /// 【非推奨！】平均のrgbから，その色に最も近いブロックを返す
    /// </summary>
    /// <param name="aveR"></param>
    /// <param name="aveG"></param>
    /// <param name="aveB"></param>
    /// <returns>BlockColorInfo object.</returns>
    public static BlockColorInfo SelectBlockWithRgb(float aveR, float aveG, float aveB)
    {
        BlockColorInfo block = MinecraftBlockColors[0]; // とりあえず適当に入れておく
        float minDistanceMagnitude = Mathf.Infinity;

        MinecraftBlockColors.ForEach(_block =>
        {
            float distanceMagnitude = (_block.r - aveR) * (_block.r - aveR) + (_block.g - aveG) * (_block.g - aveG) + (_block.b - aveB) * (_block.b - aveB);
            if (distanceMagnitude < minDistanceMagnitude)
            {
                minDistanceMagnitude = distanceMagnitude; 
                block = _block;
            }
        });

        return block;
    }

    /// <summary>
    /// 平均のlabから，その色に最も近いブロックを返す
    /// </summary>
    /// <param name="aveL"></param>
    /// <param name="aveA"></param>
    /// <param name="aveB"></param>
    /// <returns>BlockColorInfo object.</returns>
    public static BlockColorInfo SelectBlockWithLab(float aveL, float aveA, float aveB)
    {
        BlockColorInfo block = MinecraftBlockColors[0]; // とりあえず適当に入れておく
        float minDistanceMagnitude = Mathf.Infinity;

        MinecraftBlockColors.ForEach(_block =>
        {
            float distanceMagnitude = (_block._l - aveL) * (_block._l - aveL) + (_block._a - aveA) * (_block._a - aveA) + (_block._b - aveB) * (_block._b - aveB);
            if (distanceMagnitude < minDistanceMagnitude)
            {
                minDistanceMagnitude = distanceMagnitude;
                block = _block;
            }
        });

        return block;
    }


    public static void Start_PlaceBlock(DATABASE db, PointCloudData pointCloud)
    {
        // Create a new client and connect to the server.
        MinecraftClient client = new MinecraftClient(db.rconIPAddress, db.rconPort);

        // Send some commands.
        // Commands use the Try-Parse pattern for error handling instead of throwing Exceptions.
        // Pass a Message by reference to get a bool return value indicating success or failure.
        // The Sockets library can still raise Exceptions you'd want to catch (e.g. connection failures).
        if (!client.Authenticate(db.rconPassword))
        {
            Debug.Log("Error Connecting to the server!!!");
            return;
        };


        //SetBlock(Vector3.left, "minecraft:glowstone");
        float aveL, aveA, aveB;
        for(int x = 0; x < pointCloud.sizeOfBlockedAllDataX; ++x)
        {
            for(int y = 0; y < pointCloud.sizeOfBlockedAllDataY; ++y)
            {
                for(int z = 0; z < pointCloud.sizeOfBlockedAllDataZ; ++z)
                {
                    int pointCountInBlock = pointCloud.blockedAllData[x, y, z].Count;
                    
                    if (pointCountInBlock > db.existenceThreshold)
                    {
                        // 点の数は閾値を超えていれば，ブロックを置くべきところである

                        // 平均の色を求める
                        aveL = aveA = aveB = 0f;
                        pointCloud.blockedAllData[x, y, z].ForEach(p =>
                        {
                            aveL += p._l;
                            aveA += p._a;
                            aveB += p._b;
                        });
                        aveL /= pointCountInBlock;
                        aveA /= pointCountInBlock;
                        aveB /= pointCountInBlock;

                        // 近い色のブロックをワールドに配置
                        SetBlock(client, new Vector3(x, y, z) + db.generatePosition, SelectBlockWithLab(aveL, aveA, aveB).technicalName);
                        //SetBlock(client, new Vector3(x, y - 62, z));// ブロックを削除（空気にする）ならこちら
                    }
                }
            }
        }


        // Cleanly disconnect when finished.
        client.Close();
    }
}
