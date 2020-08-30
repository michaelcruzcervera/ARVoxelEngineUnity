using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class DCMeshExtraction : MonoBehaviour, IMeshExtraction
{
    [SerializeField]
    private const int MAX_THRESHOLDS = 5;
    static float[] THRESHOLDS = new float[MAX_THRESHOLDS] { -1.0f, 0.1f, 1.0f, 10.0f, 50.0f };
    static int thresholdIndex = -1;

    static OctreeNode root = null;

    //multiple of 2
    [SerializeField]
    private int octreeSize = 32;

    public void MeshGeneration(Chunk chunk, List<MeshVertex> vertexBuffer, List<int> indexBuffer)
    {
        octreeSize = chunk.size;

        thresholdIndex = (thresholdIndex + 1) % MAX_THRESHOLDS;
        root = NaiveDualContouring.BuildOctree(new float3(-octreeSize / 2, -octreeSize / 2, -octreeSize / 2), octreeSize, THRESHOLDS[thresholdIndex]);

        if (root == null)
            Debug.Log("root is null");

        NaiveDualContouring.GenerateMeshFromOctree(root, vertexBuffer, indexBuffer);
    }
    void OnApplicationQuit()
    {
        NaiveDualContouring.DestroyOctree(root);
    }
    private void OnDestroy()
    {
        NaiveDualContouring.DestroyOctree(root);
    }
}
