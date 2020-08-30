
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Unity.Mathematics;
using Debug = UnityEngine.Debug;

public class MeshGenerator : MonoBehaviour
{

    private IMeshExtraction[] algorithms;
    private int algIndex = 0;

    public List<MeshVertex> vertices = new List<MeshVertex>();
    public List<int> triangles = new List<int>();

    private void Awake()
    {
        algorithms = GetComponents<IMeshExtraction>();
    }

    public void StartMeshGeneration(Chunk chunk)
    {
        if (algorithms[algIndex] != null)
        {

            Stopwatch stopwatch = new Stopwatch();

            /*
            for (int repeat = 0; repeat < 1; ++repeat)
            {
                stopwatch.Reset();
                stopwatch.Start();
                algorithms[algIndex].MeshGeneration(chunk, vertices, triangles);
                stopwatch.Stop();
                //UnityEngine.Debug.Log("Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);
            }*/

            algorithms[algIndex].MeshGeneration(chunk, vertices, triangles);

            CompleteMeshGeneration(chunk, vertices, triangles);
            vertices.Clear();
            triangles.Clear();
        }


    }

    public void CompleteMeshGeneration(Chunk chunk, List<MeshVertex> vertexBuffer, List<int> indexBuffer)
    {
        
        Vector3[] vertArray = new Vector3[vertexBuffer.Count];

        for (int i = 0; i < vertexBuffer.Count; i++)
        {
            vertArray[i] = vertexBuffer[i].xyz;
        }

        Mesh mesh = chunk.mesh;
        mesh.Clear();

            mesh.vertices = vertArray;
            mesh.triangles = indexBuffer.ToArray();


            chunk.meshCollider.sharedMesh = mesh;

            mesh.RecalculateNormals();

            Debug.Log("Mesh Generated");
        
        

    }

    void OnApplicationQuit()
    {

    }
}
