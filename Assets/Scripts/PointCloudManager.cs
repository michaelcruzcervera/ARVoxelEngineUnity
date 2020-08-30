using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using KNN;
using KNN.Jobs;
using KNN.Internal;
using UnityEngine.XR.ARFoundation;

public class PointCloudManager : MonoBehaviour
{
    float3[] pointCloud;

    PointCloudVisualiser pointCloudVisualiser;
    PointCloudListener pointCloudListener;

    [SerializeField]
    public int pointIndex = 0;

    [SerializeField]
    bool fromFile = true;

    [SerializeField]
    bool loadAtStart = false;


    private NativeArray<float3> pointsNA;

    [SerializeField]
    private float radiusSDF = 5f;



    // Start is called before the first frame update
    void Start()
    {
        pointCloudVisualiser = GetComponent<PointCloudVisualiser>();
        pointCloudListener = GetComponent<PointCloudListener>();

        if (loadAtStart)
        {
            LoadPointCloud();
        }
    }

    public void LoadPointCloud()
    {
       

        if (fromFile)
        {
            pointCloud = PointCloudFile.DeserializeData("bunny", 100);
        }
        else
        {
            pointCloud = pointCloudListener.GetPoints();
        }



        pointCloudVisualiser.SetPointCloud(pointCloud);

        pointsNA = new NativeArray<float3>(pointCloud.Length, Allocator.Persistent);
        for (int i = 0; i < pointsNA.Length; ++i)
        {
            pointsNA[i] = pointCloud[i];
        }

        
    }

    public void SavePointCloud()
    {
        PointCloudFile.SerializeData("test", pointCloud);
    }

    public int GetPointCloudSize()
    {
        KnnContainer knnContainer = new KnnContainer(pointsNA, true, Allocator.TempJob);
        KdNodeBounds bounds = knnContainer.MakeBounds();
        float size = math.distance(bounds.Max, bounds.Min);
        knnContainer.Dispose();
        return (int) math.round(size);
    }

    public float3 GetPointCloudCentre()
    {
        float3 sum = float3.zero;
        for(int i = 0; i<pointCloud.Length; i++)
        {
            sum += pointCloud[i];
        }

        return sum / pointCloud.Length;
    }


    public float[] SDF(float3[] positions)
    {
        if (pointsNA != null && pointsNA.Length >= 0)
        {

            float[] densities = new float[positions.Length];

            var queryPositions = new NativeArray<float3>(positions.Length, Allocator.TempJob);

            var results = new NativeArray<int>(positions.Length, Allocator.TempJob);

            for (int i = 0; i < queryPositions.Length; ++i)
            {
                queryPositions[i] = positions[i];
            }

            KnnContainer knnContainer = new KnnContainer(pointsNA, true, Allocator.TempJob);

            var batchQueryJob = new QueryKNearestBatchJob(knnContainer, queryPositions, results);

            batchQueryJob.ScheduleBatch(positions.Length, positions.Length / 32).Complete();


            for (int i = 0; i < results.Length; i++)
            {
                densities[i] = math.clamp(math.distance(pointsNA[results[i]], positions[i]) - radiusSDF, -1, 1);
            }

            knnContainer.Dispose();
            results.Dispose();
            queryPositions.Dispose();

            return densities;
        }
        else
        {
            Debug.Log("Error: Points must be >=0");
            
        }

        return null;
    }
    
    private (float3, float3) PlaneFitting(int[] pointIndexes)
    {
        float3 sum = float3.zero;

        for (int i = 0; i < pointIndexes.Length; i++)
        {
            sum += pointCloud[pointIndexes[i]];
        }

        var centroid = sum * (float)(1.0 / pointIndexes.Length);

        float xx = 0.0f; var xy = 0.0f; var xz = 0.0f;
        float yy = 0.0f; var yz = 0.0f; var zz = 0.0f;

        for (int i = 0; i < pointIndexes.Length; i++)
        {
            float3 r = (float3)pointCloud[pointIndexes[i]] - centroid;

            xx += r.x * r.x;
            xy += r.x * r.y;
            xz += r.x * r.z;
            yy += r.y * r.y;
            yz += r.y * r.z;
            zz += r.z * r.z;
        }

        float det_x = yy * zz - yz * yz;
        float det_y = xx * zz - xz * xz;
        float det_z = xx * yy - xy * xy;

        var det_max = Mathf.Max(det_x, det_y, det_z);

        float3 dir;

        if (det_max == det_x)
        {
            dir = new float3(det_x, xz * yz - xy * zz, xy * yz - xz * yy);
        }
        else if (det_max == det_y)
        {
            dir = new float3(xz * yz - xy * zz, det_y, xy * xz - yz * xx);
        }
        else
        {
            dir = new float3(xy * yz - xz * yy, xy * xz - yz * xx, det_z);
        }

        return (centroid, math.normalize(dir));

    }

    void OnApplicationQuit()
    {
        if (pointsNA.IsCreated)
        {
            pointsNA.Dispose();
        }

    }
   
    private void OnDrawGizmos()
    {
        /*
        if (tree != null && drawKDBounds == true)
        {
            //tree.DrawAllBounds();
        }
        
        if (planes != null)
        {
            for (int i = 0; i < planes.Length; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(planes[i].Item1, (planes[i].Item2));
            }
        }
        */
        
    }
}
