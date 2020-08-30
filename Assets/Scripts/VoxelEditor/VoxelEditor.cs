using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class VoxelEditor : MonoBehaviour
{
    [SerializeField]
    float deformSpeed = 5.0f;

    [SerializeField]
    float deformRadius = 4.0f;

    [SerializeField]
    public WorldManager worldManager { get; set; }

    [SerializeField]
    private new Camera camera = null;

    List<int3> drawPositions = new List<int3>();

    // Update is called once per frame
    private void Update()
    {
        
        if (Input.GetMouseButton(0))
        {
            
             Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                
                if (hit.transform.tag.Equals("Terrain"))
                {

                    Vector3 hitPoint = hit.point;

                    EditTerrain(hitPoint, true, deformSpeed, deformRadius);

                }

            }
        }
        else if (Input.GetMouseButton(1))
        {
            Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));
            
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag.Equals("Terrain"))
                {
                    Vector3 hitPoint = hit.point;

                    EditTerrain(hitPoint, false, deformSpeed, deformRadius);

                }
            }
        }
    }

    void EditTerrain(float3 position, bool add, float deformSpeed, float radius)
    {

        int mult = (worldManager.minChunkSize) / worldManager.chunkResolution;

        int3 v3Int = new int3(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));

        int intRange = Mathf.CeilToInt(radius);

        drawPositions.Clear();

        for (int x = -intRange; x <= intRange; x++)
        {
            for (int y = -intRange; y <= intRange; y++)
            {
                for (int z = -intRange; z <= intRange; z++)
                {

                    int3 offset = new int3(v3Int.x + x, v3Int.y + y, v3Int.z + z);

                    if (offset.x % mult == 0 && offset.y % mult == 0 && offset.z % mult == 0)
                    {
                        offset -= mult / 2;

                        float amount = deformSpeed;

                        float distance = math.distance(offset, radius);

                        if (distance > radius)
                        {
                            float deform = deformSpeed / distance;

                            if (add)
                            {
                                deform = -deform;
                            }

                            drawPositions.Add(offset);

                            worldManager.IncreaseDensity(offset, deform);
                        }
                    }
                }
            }
        }
        
        worldManager.modified = true;
    }


    private void OnDrawGizmos()
    {
        /*
        Gizmos.color = Color.red;
        
        for (int i = 0; i< drawPositions.Count; i++)
        {
            Gizmos.DrawSphere((float3)drawPositions[i], 1f);
        }
        */
    }
}
