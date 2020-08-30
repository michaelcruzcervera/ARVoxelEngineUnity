using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

public class MCMeshExtraction : MonoBehaviour, IMeshExtraction
{

    [SerializeField]
    private bool flatShading = false;

    [SerializeField]
    private bool interpolation = false;

    [SerializeField, Range(-1.0f, 1.0f)]
    float isoLevel = -1;


    public void MeshGeneration(Chunk chunk, List<MeshVertex> vertexBuffer, List<int> indexBuffer)
    {
        
        var marchingCubes = new MarchingCubes
        {
            _chunkSize = chunk.size,
            _chunkResolution = chunk.resolution,
            _flatShading = flatShading,
            _interpolation = interpolation,
            _isoLevel = isoLevel,
            _pos = chunk.position - chunk.size/2f,
            _densities = chunk.densities
        };

        //March(marchingCubes, chunkSize, resolution, pos);
        for (int i =0; i < chunk.resolution*chunk.resolution*chunk.resolution; i++)
        {
            marchingCubes.Execute(i);
        }
        

        //get number of triangles
        int triCount = marchingCubes._triangles.Capacity;

        for (int i = 0; i < marchingCubes._triangles.Count; i++)
        {
            indexBuffer.Add(marchingCubes._triangles[i]);
            
        }

        MeshVertex m = new MeshVertex();
        for (int i = 0; i < marchingCubes._vertices.Count; i++)
        {
            m.xyz = marchingCubes._vertices[i];
            vertexBuffer.Add(m);
        }

    }
}
