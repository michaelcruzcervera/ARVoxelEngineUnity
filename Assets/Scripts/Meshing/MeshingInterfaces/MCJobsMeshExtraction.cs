using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

public class MCJobsMeshExtraction : MonoBehaviour, IMeshExtraction
{

    [SerializeField]
    private bool interpolation = false;

    [SerializeField, Range(-1.0f, 1.0f)]
    float isoLevel = 0;


    public void MeshGeneration(Chunk chunk, List<MeshVertex> vertexBuffer, List<int> indexBuffer)
    {
        int chunkResolution = chunk.resolution;

        var outputVertices = new NativeArray<float3>(15 * chunkResolution * chunkResolution * chunkResolution, Allocator.TempJob);
        var outputTriangles = new NativeArray<int>(15 * chunkResolution * chunkResolution * chunkResolution, Allocator.TempJob);

        Counter counter = new Counter(Allocator.TempJob);

        var densities = new NativeArray<float>(chunk.densities.Length, Allocator.TempJob);


        for (int i = 0; i < densities.Length; i++)
        {
            densities[i] = chunk.densities[i];
        }

        var marchingCubesJob = new MarchingCubesJob
        {
             _isoLevel = isoLevel,
            _chunkSize = chunk.size,
            _chunkResolution = chunkResolution,
            _interpolation = interpolation,
            _vertices = outputVertices,
            _triangles = outputTriangles,
            _chunkDensities = densities,
            _pos = chunk.position - chunk.size / 2f,
            counter = counter
        };

        JobHandle jobHandle = marchingCubesJob.Schedule(chunkResolution * chunkResolution * chunkResolution, 128);

        jobHandle.Complete();


        //get number of triangles
        int triCount = outputTriangles.Length;

        for (int i = 0; i < triCount; i++)
        {
            indexBuffer.Add(outputTriangles[i]);

        }
        int vertCount = outputVertices.Length;
        MeshVertex m = new MeshVertex();
        for (int i = 0; i < vertCount; i++)
        {
            m.xyz = outputVertices[i];
            vertexBuffer.Add(m);
        }


        counter.Dispose();
        densities.Dispose();
        outputVertices.Dispose();
        outputTriangles.Dispose();


    }
}