using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UI;

public class VoxelEditor : MonoBehaviour
{
    [SerializeField]
    float deformSpeed = 1.5f;

    [SerializeField]
    float deformRadius = 1f;

    [SerializeField]
    public WorldManager worldManager { get; set; }

    [SerializeField]
    private new Camera camera = null;

    List<int3> drawPositions = new List<int3>();

    bool adding = false;

    bool removing = false;

    [SerializeField]
    Slider deformSpeedSlider;

    [SerializeField]
    Slider deformRadiusSlider;

    public void StartAdding()
    {
        if (!removing)
        {
            adding = true;
        }
    }
    public void EndAdding()
    {
        Debug.Log("Stop");
        adding = false;
    }

    public void StartRemoving()
    {
        if (!adding)
        {
            removing = true;
        }

    }

    public void EndRemoving()
    {
        removing = false;
    }

    public void SetDeformSpeed()
    {
        float speed = deformSpeedSlider.value;

        if (speed > 0 && speed < 6)
        {
            deformSpeed = speed;
        }

    }

    public void SetDeformRadius()
    {
        float radius = deformRadiusSlider.value;

        if (radius > 1 && radius < 6)
        {
            deformRadius = radius;
        }

    }

    private void Start()
    {
        deformRadiusSlider.value = deformRadius;
        deformSpeedSlider.value = deformSpeed;
    }

    // Update is called once per frame
    private void Update()
    {

        if (adding)
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
        else if (removing)
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

        int3 centre = new int3(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));

        int intRange = Mathf.CeilToInt(radius);

        drawPositions.Clear();

        for (int x = -intRange; x <= intRange; x++)
        {
            for (int y = -intRange; y <= intRange; y++)
            {
                for (int z = -intRange; z <= intRange; z++)
                {

                    int3 offset = new int3(centre.x + x, centre.y + y, centre.z + z);

                    float amount = deformSpeed;

                    float distance = math.distance(offset, centre);

                    if (distance < radius)
                    {
                        float deform = deformSpeed / distance;

                        if (add)
                        {
                            deform *= -1;
                        }

                        //drawPositions.Add(offset);

                        worldManager.IncreaseDensity(offset, deform);
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
