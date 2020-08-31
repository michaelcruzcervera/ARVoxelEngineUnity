using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

public class Chunk : MonoBehaviour
{
    public int index;

    public Material mat;

    public MeshFilter meshFilter { get; set; }

    public MeshRenderer meshRenderer { get; set; }

    public MeshCollider meshCollider;

    public Mesh mesh;

    [SerializeField]
    public bool modified { get; set; }

    [SerializeField]
    public int size { get; set; }
    [SerializeField]
    public int resolution { get; set; }
    public float3 position { get; set; }

    [SerializeField]
    public float[] densities;

    public bool useSDF = false;

    public void Initialise()
    {
        modified = false;
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        if (meshCollider == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();

        }

        if (mesh == null)
        {
            mesh = new Mesh();
        }



        GetComponent<MeshFilter>().mesh = mesh;


        SetMaterial(mat);

        meshRenderer.shadowCastingMode = 0;

        densities = new float[(resolution + 1) * (resolution + 1) * (resolution + 1)];

        GenerateDensitites();
    }

    public void GenerateDensitites()
    {
        PointCloudManager pointCloudManager = FindObjectOfType<PointCloudManager>();
        float3[] positions = new float3[densities.Length];

        for (int x = 0; x < resolution + 1; x++)
        {
            for (int y = 0; y < resolution + 1; y++)
            {
                for (int z = 0; z < resolution + 1; z++)
                {

                    float fx = x * size / (resolution);
                    float fy = y * size / (resolution);
                    float fz = z * size / (resolution);
                    float3 id = new float3(position.x - ((float) size / 2) + fx, (float) position.y - ((float) size / 2) + fy, (float) position.z - ((float) size / 2) + fz);

                    if (useSDF)
                    {
                        densities[IndexUtils.int3ToIndex(x, y, z, resolution + 1)] = SDF.Density_Func(id);
                    }
                    else
                    {
                        positions[IndexUtils.int3ToIndex(x, y, z, resolution + 1)] = id;

                    }
                }
            }

        }

        if (!useSDF)
        {
            densities = pointCloudManager.SDF(positions);
        }

    }

    public void ModifyDensities()
    {
        float t = Time.time;
        for (int i = 0; i< densities.Length; i ++)
        {
            densities[i] += t;
        }
    }

    public void SetMaterial(Material m)
    {
        if (m != null)
        {
            mat = m;
            meshRenderer.material = mat;
        }

    }
    public void Destroy()
    {
        Destroy(gameObject);
    }

    public void IncreaseDensity(int3 localPos, float deform)
    {
        int index = IndexUtils.int3ToIndex(localPos.x, localPos.y, localPos.z, resolution+1);
        if (index >= 0 && index < densities.Length)
        {
            densities[index] = math.clamp(densities[index] + deform, -1, 1);

            modified = true;
        }
       
    }

    public bool Contains(float3 point)
    {
        if (point.x <= position.x + (size/2) && point.x >= position.x - (size/ 2)-1 )
        {
            if (point.y <= position.y + (size/2) && point.y >= position.y - (size/ 2)-1 )
            {
                if (point.z <= position.z + (size/2) && point.z >= position.z - (size/ 2)-1)
                {
                    return true;
                }
            }
            
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(position, 1);
    }
    //export
}
