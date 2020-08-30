using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class ChunkOctreeNode<T>
{
    //coord of chunk
    public float3 position { get; set; }
    //size
    public int size { get; set; }

    //min size for a node
    int minSize = 4;

    public ChunkOctreeNode<T>[] children { get; set; }

    public bool hasChildren { get { return children != null; } }

    public T chunk { get; set; }

    public bool modifed = false;

    //constructor
    //length of this node
    //min size
    //coord
    public ChunkOctreeNode(int size, int minSize, float3 position)
    {
        setSize(size);
        setMinSize(minSize);
        this.position = position;
    }

    void setSize(int size)
    {
        if (size >= minSize && size % 2 == 0)
        {
            this.size = size;
        }
    }

    void setMinSize(int size)
    {
        if (size > 0)
        {
            this.minSize = size;
        }
    }

    //set children

    void SetChildren(ChunkOctreeNode<T>[] children)
    {
        if (children.Length == 8)
        {
            this.children = children;
        }
    }

    //draw bounds
    public void DrawAllBounds(float depth = 0)
    {
        float tintVal = depth / 7; // Will eventually get values > 1. Color rounds to 1 automatically
        Gizmos.color = new Color(tintVal, 0, 1.0f - tintVal);

        Bounds thisBounds = new Bounds(position, new Vector3(size, size, size));
        Gizmos.DrawWireCube(thisBounds.center, thisBounds.size);

        if (children != null)
        {
            depth++;
            for (int i = 0; i < 8; i++)
            {
                children[i].DrawAllBounds(depth);
            }
        }
        Gizmos.color = Color.white;
    }

    //subdivide

    public void Subdivide()
    {
        SetChildren(new ChunkOctreeNode<T>[8]);

        int quarter = size / 4;
        int newLength = size / 2;

        children[0] = new ChunkOctreeNode<T>(newLength, minSize, position + new float3(-quarter, quarter, -quarter));
        children[1] = new ChunkOctreeNode<T>(newLength, minSize, position + new float3(quarter, quarter, -quarter));
        children[2] = new ChunkOctreeNode<T>(newLength, minSize, position + new float3(-quarter, quarter, quarter));
        children[3] = new ChunkOctreeNode<T>(newLength, minSize, position + new float3(quarter, quarter, quarter));
        children[4] = new ChunkOctreeNode<T>(newLength, minSize, position + new float3(-quarter, -quarter, -quarter));
        children[5] = new ChunkOctreeNode<T>(newLength, minSize, position + new float3(quarter, -quarter, -quarter));
        children[6] = new ChunkOctreeNode<T>(newLength, minSize, position + new float3(-quarter, -quarter, quarter));
        children[7] = new ChunkOctreeNode<T>(newLength, minSize, position + new float3(quarter, -quarter, quarter));
    }

    //merge

    public void Merge()
    {
        for (int i = 0; i < 8; i++)
        {
            children[i] = null;
        }
        children = null;

    }



    //check if far enough to merge
    public bool ShouldMerge()
    {
        var camPosition = Camera.main.transform.position;
        var magnitude = math.distance(position, camPosition);

        if (magnitude > (size * 1.2) )
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ShouldSubdivide()
    {
        var camPosition = Camera.main.transform.position;
        var magnitude = math.distance(position, camPosition);

        if (magnitude < (size * 1.2) && size > minSize)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


}

