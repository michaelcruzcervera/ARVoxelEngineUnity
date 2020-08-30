using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class IndexUtils
{
    public static int3 IndexToInt3(int index, int resolution)
    {
        int x = index % resolution;
        int y = (index / resolution) % resolution;
        int z = index / (resolution * resolution);
        return new int3(x, y, z);

        //return pos;
    }
    public static int int3ToIndex(int x, int y, int z, int resolution)
    {
        return x + y * resolution + z * resolution * resolution;
    }

    public static int int3ToIndex(int3 index, int size)
    {
        return int3ToIndex(index.x, index.y, index.z, size);
    }

}
