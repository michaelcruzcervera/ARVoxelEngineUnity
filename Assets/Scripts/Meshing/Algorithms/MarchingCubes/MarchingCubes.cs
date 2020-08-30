using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using UnityEditor;
using System;


public class MarchingCubes
{
    public int _chunkSize = 10;
    public int _chunkResolution = 10;

    public bool _flatShading { get; set; }
    public bool _interpolation {get; set;}

    public float _isoLevel = -1;



    public List<float3> _vertices = new List<float3>();

    public List<int> _triangles = new List<int>();

    public float3 _pos;

    public float[] _densities;

    //public void Execute(float3 id, int3 index)
    public void Execute(int index)
    {
        int3 localPos = IndexUtils.IndexToInt3(index, _chunkResolution);

        float fx = localPos.x * _chunkSize / ((float)_chunkResolution);
        float fy = localPos.y * _chunkSize / ((float)_chunkResolution);
        float fz = localPos.z * _chunkSize / ((float)_chunkResolution);

        float3 worldPos = new float3(fx, fy, fz) + _pos;
        // Densities
        corners<float> cellDensities = getDensities(localPos, worldPos, _chunkSize, _chunkResolution);


        //Calculate unique index for each cell configuration

        int cellIndex = calculateIndex(cellDensities, _isoLevel);

        if (cellIndex == 0 || cellIndex == 255)
        {
            return;
        }

        // Corners
        corners<float3> cellCorners = getCorners(worldPos, _chunkSize, _chunkResolution);

        //Debug.Log(cellIndex);
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
            


            //if flat shadin then add 3 vertices for every triangle otherwise only add unique vertices
            if (_flatShading)
            {
                _vertices.Add(v0);
                _triangles.Add(_vertices.Count-1);
                _vertices.Add(v1);
                _triangles.Add(_vertices.Count - 1);
                _vertices.Add(v2);
                _triangles.Add(_vertices.Count - 1);
            }
            else
            {
                _triangles.Add(vertexIndex(v0));
                _triangles.Add(vertexIndex(v1));
                _triangles.Add(vertexIndex(v2));
            }
            
        }
    }

    private int vertexIndex(float3 vertex)
    {
        for (int i = 0; i < _vertices.Count; i++)
        {
            if(_vertices[i].Equals(vertex))
            {
                return i;
            }
        }
        _vertices.Add(vertex);
        return _vertices.Count - 1;

    }
   
    private corners<float> getDensities(int3 index, float3 id, int size, int res)
    {

        corners<float> densities = new corners<float>();
        for (int i = 0; i < 8; i++)
        {

            float density = _densities[IndexUtils.int3ToIndex(index + LookupTables.CubeCorners[i], res+1)];
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
        if (densities[1] < isoLevel) {index |= 2; }
        if (densities[2] < isoLevel) { index |= 4; }
        if (densities[3] < isoLevel) { index |= 8; }
        if (densities[4] < isoLevel) { index |= 16; }
        if (densities[5] < isoLevel) { index |= 32; }
        if (densities[6] < isoLevel) { index |= 64; }
        if (densities[7] < isoLevel) { index |= 128; }


        //Debug.Log("index: "+index);
        return index;
    }


    private float3 interpolateVerts(float3 v1, float3 v2, float d1, float d2, float isoLevel)
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
