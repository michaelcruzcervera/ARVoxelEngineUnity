using System.Collections.Generic;
using Unity.Collections;

internal interface IMeshExtraction
{
    void MeshGeneration(Chunk chunk, List<MeshVertex> vertexBuffer, List<int> indexBuffer);
}