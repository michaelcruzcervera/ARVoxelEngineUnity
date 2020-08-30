using UnityEngine;
using UnityEditor;

public class Editor : EditorWindow
{
    [MenuItem("Tool/VoxelTerrain")]
    public static void ShowWindow()
    {
        GetWindow(typeof (Editor));
    }

    private void OnGUI()
    {
        GUILayout.Label("Voxel Terrain System", EditorStyles.boldLabel);
    }
}
