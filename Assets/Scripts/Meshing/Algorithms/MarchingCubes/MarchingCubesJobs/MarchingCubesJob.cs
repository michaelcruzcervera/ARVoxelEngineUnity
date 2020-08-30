using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;


[BurstCompile]
public struct MarchingCubesJob : IJobParallelFor
{
    public int _chunkSize;

    public int _chunkResolution;

    public bool _flatShading { get; set; }
    public bool _interpolation { get; set; }

    public float _isoLevel;

    [NativeDisableParallelForRestriction, WriteOnly]  public NativeArray<float3> _vertices;

    [NativeDisableParallelForRestriction, WriteOnly]  public NativeArray<int> _triangles;


    [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<float> _chunkDensities;

    [WriteOnly] public Counter counter;


    public float3 _pos;

    public void Execute(int index)
    {
        //float3 id, int3 index;
        //get voxel position within chunk
        int3 localPos = IndexUtils.IndexToInt3(index, _chunkResolution);
        //_chunkSizeRelative = _chunkSize;
        //_chunkSize = _chunkSizeRelative / _chunkResolution;

        
        float fx = localPos.x * _chunkSize / ((float) _chunkResolution);
        float fy = localPos.y * _chunkSize / ((float)_chunkResolution);
        float fz = localPos.z * _chunkSize / ((float)_chunkResolution);


        float3 worldPos = new float3(fx, fy, fz) + _pos;
        

        // Densities
        corners<float> cellDensities = getDensities(localPos,  _chunkSize, _chunkResolution);

        //Calculate unique index for each cell configuration

        int cellIndex = calculateIndex(cellDensities, _isoLevel);

        if (cellIndex == 0 || cellIndex == 255)
        {
            return;
        }

        // Corners
        corners<float3> cellCorners = getCorners(worldPos, _chunkSize, _chunkResolution);


        int rowIndex = 15 * cellIndex;
        //Generate triangles for current cube configuration
        for (int i = 0; LookupTables.triangleTable[rowIndex + i] != -1 && i < 15; i += 3)
        {

            //get indices for each of the 3 edges to be joined to form the triangle
            int b0 = LookupTables.EdgeIndexTable1[LookupTables.triangleTable[rowIndex + i]];
            int e0 = LookupTables.EdgeIndexTable2[LookupTables.triangleTable[rowIndex + i]];

            int b1 = LookupTables.EdgeIndexTable1[LookupTables.triangleTable[rowIndex + i + 1]];
            int e1 = LookupTables.EdgeIndexTable2[LookupTables.triangleTable[rowIndex + i + 1]];

            int b2 = LookupTables.EdgeIndexTable1[LookupTables.triangleTable[rowIndex + i + 2]];
            int e2 = LookupTables.EdgeIndexTable2[LookupTables.triangleTable[rowIndex + i + 2]];

            float3 v0;
            float3 v1;
            float3 v2;

            if (_interpolation)
            {
                //interpolate Vertices to get a smoother terrain
                v0 = interpolateVerts(cellCorners[b0], cellCorners[e0], cellDensities[b0], cellDensities[e0], _isoLevel);
                v1 = interpolateVerts(cellCorners[b1], cellCorners[e1], cellDensities[b1], cellDensities[e1], _isoLevel);
                v2 = interpolateVerts(cellCorners[b2], cellCorners[e2], cellDensities[b2], cellDensities[e2], _isoLevel);
            }
            else
            {
                v0 = CombineVerts(cellCorners[b0], cellCorners[e0], _isoLevel);
                v1 = CombineVerts(cellCorners[b1], cellCorners[e1], _isoLevel);
                v2 = CombineVerts(cellCorners[b2], cellCorners[e2], _isoLevel);
            }

            int triangleIndex = counter.Increment() * 3;


            _vertices[triangleIndex + 0] = v0;
            _triangles[triangleIndex + 0] = (ushort)(triangleIndex + 0);

            _vertices[triangleIndex + 1] = v1;
            _triangles[triangleIndex + 1] = (ushort)(triangleIndex + 1);

            _vertices[triangleIndex + 2] = v2;
            _triangles[triangleIndex + 2] = (ushort)(triangleIndex + 2);

        }
    }
 
    private corners<float> getDensities(int3 localPos, int size, int res)
    {
        corners<float> densities = new corners<float>();
        for (int i = 0; i < 8; i++)
        {
            float density = _chunkDensities[IndexUtils.int3ToIndex(localPos + LookupTables.CubeCorners[i], res+1)];
            densities[i] = density;

        }
        return densities;
    }

    private corners<float3> getCorners(float3 id, float size, float res)
    {
        corners<float3> corners = new corners<float3>();
        for (int i = 0; i < 8; i++)
        {
            float3 cellCorner = new float3(id.x, id.y, id.z) + ((float3)LookupTables.CubeCorners[i] * (size / res));

            corners[i] = cellCorner;
        }
        return corners;

    }

    private int calculateIndex(corners<float> densities, float isoLevel)
    {
        int index = 0;

        if (densities[0] < isoLevel) { index |= 1; }
        if (densities[1] < isoLevel) { index |= 2; }
        if (densities[2] < isoLevel) { index |= 4; }
        if (densities[3] < isoLevel) { index |= 8; }
        if (densities[4] < isoLevel) { index |= 16; }
        if (densities[5] < isoLevel) { index |= 32; }
        if (densities[6] < isoLevel) { index |= 64; }
        if (densities[7] < isoLevel) { index |= 128; }

        return index;
    }


    float3 interpolateVerts(float3 v1, float3 v2, float d1, float d2, float isoLevel)
    {
        float t = (isoLevel - d1) / (d2 - d1);
        return v1 + t * (v2 - v1);
    }

    private float3 CombineVerts(float3 v1, float3 v2, float isoLevel)
    {
        float t = isoLevel;
        return (v1 + v2) / 2;
    }
}