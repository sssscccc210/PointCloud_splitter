using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEditor;

public class Manager : MonoBehaviour
{
    [Header("↓Minecraftのワールドで再現するか", order = 2)]
    [SerializeField] bool active_export_minecraft = true;

    [Header("↓区切った点群データを各区間ごとにmesh assetで出力するか", order = 3)]
    [SerializeField] bool active_export_mesh = true;

    private async void Start()
    {
        DATABASE db = this.GetComponent<DATABASE>();

        PointCloudData pointCloud = new PointCloudData();
        pointCloud.LoadPly(db);// plyデータを読み込ませる

        ClearChacheMethod();
        if (active_export_minecraft)
        {
            PCS_MinecraftExp.Start_PlaceBlock(db, pointCloud);// Minecraftのワールドにブロックを配置する
            ClearChacheMethod();
        }
        if (active_export_mesh)
        {
            PCS_MeshExp.Start_pcsme(db);// Meshでエクスポートする
            ClearChacheMethod();
        }

    }

    void ClearChacheMethod()
    {
        AssetDatabase.Refresh();
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
        Debug.Log("Cache cleared.");
    }
}
