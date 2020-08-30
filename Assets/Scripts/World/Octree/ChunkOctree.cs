using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class ChunkOctree<T>
{
    //number of chunks within octree
    public int count { get; set; }

    //root
    public ChunkOctreeNode<T> root;

    //initial octree size
    int initalSize;

    //min chunk size
    int minSize;

    //constructor
    public ChunkOctree(int initialWorldSize, float3 initialPos, int minSize)
    {
        count = 0;
        initalSize = initialWorldSize;
        this.minSize = minSize;
        root = new ChunkOctreeNode<T>(initalSize, minSize, initialPos);

    }

    public void DrawAllBounds()
    {
        root.DrawAllBounds();
    }

}

