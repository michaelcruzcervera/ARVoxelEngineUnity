using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[ExecuteInEditMode]
public class GenerateGrid : MonoBehaviour
{
    Vector3 coord;
    [SerializeField, Range(1, 16)]
    int chunkSize = 10;
    [SerializeField, Range(1, 50)]
    int resolution = 10;

    int _chunkSize = 0;
    int _resolution = 0;

    public Color gizmoColour = Color.white;

    List<Vector3> points = new List<Vector3>();



    private void Awake()
    {
        coord = transform.position;

        _chunkSize = chunkSize;
        _resolution = resolution;
    }

    void Start()
    {
        PopulatePoints(_chunkSize, _resolution, coord);
    }

    // Update is called once per frame
    void Update()
    {
        if(_chunkSize != chunkSize || _resolution != resolution)
        {
            _chunkSize = chunkSize;
            _resolution = resolution;

            PopulatePoints(_chunkSize, _resolution, coord);
            
        }
    }

    void PopulatePoints(int size, int res, Vector3 pos)
    {
        
        points.Clear();

        for (int x = -(res/2); x < res/2; x++)
        {
            for (int y = -(res / 2); y < res/2; y++)
            {
                for (int z = -(res / 2); z < res/2; z++)
                {

                    float fx = x * size / (res);
                    float fy = y * size / (res);
                    float fz = z * size / (res);
                    Vector3 cellPosition = new Vector3(pos.x + fx, pos.y + fy, pos.z + fz);
                    points.Add(cellPosition);
                    
                    
                }
            }

        }
    }

    private void OnDrawGizmos()
    {

        Vector3 transform = this.transform.position;

        
        Debug.Log(points.Count);
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 id = points[i];
            

            Gizmos.color = Color.red;


            for (int j = 0; j < 8; j++)
            {
                Vector3 n = (new Vector3(id.x-(_chunkSize / _resolution)/2, id.y- (_chunkSize / _resolution)/2, id.z- (_chunkSize / _resolution)/2))+((Vector3)(float3)LookupTables.CubeCorners[j]*(_chunkSize/_resolution));
                Gizmos.DrawSphere(transform + n, 0.05f);
            }
            
            Gizmos.color = gizmoColour;

            Gizmos.DrawSphere(transform+id, 0.1f);
            Gizmos.DrawWireCube(transform+id,new Vector3(_chunkSize/_resolution, _chunkSize / _resolution, _chunkSize / _resolution));
            //Gizmos.DrawWireCube(transform + id, new Vector3(1, 1, 1)*(chunkSize/resolution));
        }
    }
}
