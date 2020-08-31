using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.XR.ARFoundation;

public class WorldManager : MonoBehaviour
{
    [SerializeField]
    public MeshGenerator meshGenerator;

    //World Variables
    [SerializeField]
    public int worldSize = 16;

    private int maxWorldSize = 64;
    [SerializeField]
    private float3 worldPosition = new float3(0,0,0);
    [SerializeField]
    public int minChunkSize = 16;

    //Chunk Variables
    [SerializeField]
    public int chunkResolution = 16;

    [SerializeField]
    private Material material = null;

    public bool useLOD = false;

    public Transform test;

    public bool showBoundsGizmo;
    public Color boundsGizmoCol = Color.grey;

    [SerializeField]
    public ChunkOctree<Chunk> world;

    [SerializeField]
    PointCloudManager pointCloudManager = null;

    [SerializeField]
    VoxelEditor voxelEditor;

    [SerializeField]
    private bool editing = true;

    List<Chunk> leafs;

    List<int> ModifiedChunks = new List<int>();

    public bool modified = false;

    ARSessionOrigin aRSessionOrigin;

    void InstantiateWorldOctree(int size, float3 position, int minChunkSize)
    {
        if (worldSize % minChunkSize == 0)
        {
            world = new ChunkOctree<Chunk>(size, position, minChunkSize);
        }
        else
        {
            Debug.Log("Error: Incompatible Octree Parameters");
        }
    }
   
    public void StartMeshing()
    {
        aRSessionOrigin = FindObjectOfType<ARSessionOrigin>();

        voxelEditor = FindObjectOfType<VoxelEditor>();

        if (pointCloudManager != null)
        {

            DestroyWorld();

            worldPosition = pointCloudManager.GetPointCloudCentre();

            int pointCloudSize = pointCloudManager.GetPointCloudSize();

            while (pointCloudSize > worldSize && worldSize < maxWorldSize)
            {

                    worldSize = worldSize * 2;

            }
            
            InstantiateWorldOctree(worldSize, worldPosition, minChunkSize);

            if (voxelEditor != null)
            {
                voxelEditor.worldManager = this;
            }

            leafs = new List<Chunk>();

            SubdivideChunks(world.root);

            aRSessionOrigin.MakeContentAppearAt(gameObject.transform,new Vector3(0,0,0), gameObject.transform.rotation);

        }
        

    }

    public void DestroyWorld()
    {
        if (leafs != null)
        {
            for (int i = 0; i < leafs.Count; i++)
            {
                if (leafs[i].gameObject.transform != null)
                {
                    Destroy(leafs[i].gameObject);
                }

                
            }
            leafs.Clear();
            leafs = null;
        }
        if (world != null)
        {
            world = null;
            GameObject[] chunks = GameObject.FindGameObjectsWithTag("Terrain");
            for (int i = 0; i < chunks.Length; i++)
            {
                Destroy(chunks[i]);
            }
        }
    }

    private void Update()
    {
        if (editing && modified)
        {
            for (int i = 0; i < ModifiedChunks.Count; i++)
            {
                meshGenerator.StartMeshGeneration(leafs[ModifiedChunks[i]]);
                leafs[ModifiedChunks[i]].modified = false;
            }
            
            ModifiedChunks.Clear();
            modified = false;
        }

        if (useLOD)
        {
            if (world != null)
            {
                SubdivideChunks(world.root);
            }
        }
    }

    void SubdivideChunks(ChunkOctreeNode<Chunk> node)
    {
        if (useLOD)
        {
            Chunk[] oldChunks = new Chunk[10];


            if (node.hasChildren)
            {
                if (node.ShouldMerge())
                {
                    for (int i = 1; i < 9; i++)
                    {
                        oldChunks[i + 1] = node.children[i - 1].chunk;
                    }
                    node.Merge();
                    node.modifed = true;
                }
                else
                {
                    oldChunks[1] = node.chunk;

                    for (int i = 0; i < 8; i++)
                    {
                        SubdivideChunks(node.children[i]);
                    }
                }

                for (int i = 0; i < oldChunks.Length; i++)
                {
                    if (oldChunks[i] != null)
                    {
                        Destroy(oldChunks[i].gameObject);
                    }

                }

            }
            else
            {

                if (node.ShouldSubdivide())
                {
                    node.Subdivide();
                    node.modifed = true;
                }
                else
                {
                    if (node.chunk == null || node.chunk.modified == true)
                    {
                        if (node.chunk != null)
                        {
                            oldChunks[0] = node.chunk;
                        }

                        node.chunk = CreateChunk(node.position, node.size, chunkResolution);
                        meshGenerator.StartMeshGeneration(node.chunk);

                        for (int i = 0; i < oldChunks.Length; i++)
                        {
                            if (oldChunks[i] != null)
                            {
                                Destroy(oldChunks[i].gameObject);
                            }

                        }

                    }

                }
            }
        }
        else
        {
            if (node.size > minChunkSize)
            {
                node.Subdivide();
                node.modifed = true;
                for (int i = 0; i < 8; i++)
                {
                    SubdivideChunks(node.children[i]);
                }
            }
            else
            {
                node.chunk = CreateChunk(node.position, node.size, chunkResolution);
                leafs.Add(node.chunk);
                meshGenerator.StartMeshGeneration(node.chunk);
            }
        }

    }

    public Chunk CreateChunk(float3 coord, int size, int res)
    {
        GameObject chunk = new GameObject($"Chunk ({coord.x}, {coord.y}, {coord.z})");
        Chunk newChunk = chunk.AddComponent<Chunk>();

        chunk.transform.parent = gameObject.transform;
        newChunk.index = IndexUtils.int3ToIndex((int)coord.x, (int)coord.y, (int)coord.z, worldSize / minChunkSize);
        chunk.tag = "Terrain";
        newChunk.position =coord;
        newChunk.size = size;
        newChunk.resolution = res;
        newChunk.mat = material;
        newChunk.Initialise();
        return newChunk;

    }

    public void SetWorldMaterial(Material mat)
    {
        material = mat;
        if(leafs != null)
        {
            for(int i =0; i < leafs.Count; i++)
            {
                leafs[i].mat = mat;
                leafs[i].gameObject.GetComponent<MeshRenderer>().material = mat;
            }
        }
    }

    public void IncreaseDensity(int3 globalPos, float deform)
    {

        for (int i = 0; i < leafs.Count; i++)
        {
            Chunk chunk = leafs[i];
           
            if (chunk.Contains(globalPos))
            {

                int3 pos = (int3) math.floor(chunk.position);
    
                //int3 localPos = (globalPos - pos + (chunk.size / 2))/ ((chunk.size/chunk.resolution));
                int3 localPos =(((globalPos - pos +(chunk.size/2))) % (chunk.resolution + 1) + (chunk.resolution +1))% (chunk.resolution+1);

                chunk.IncreaseDensity(localPos, deform);

                if (!ModifiedChunks.Contains(i))
                {
                    ModifiedChunks.Add(i);
                }
            }
        }
    }

    public void ToggleLOD()
    {
        if (useLOD)
        {
            useLOD = false;
        }
        else
        {
            useLOD = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (leafs != null)
        {
            for (int i = 0; i < leafs.Count; i++)
            {
                if (leafs[i].Contains(transform.position))
                {
                    //Debug.Log(leafs[i].index + " : " + leafs[i].Contains(transform.position));
                }

            }

        }
        Gizmos.color = boundsGizmoCol;
        if (world != null && showBoundsGizmo)
        {
            world.DrawAllBounds();
        }

    }

}

