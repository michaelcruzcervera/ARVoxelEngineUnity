using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.XR.ARFoundation;


public class PointCloudListener : MonoBehaviour
{
    [SerializeField]
    public ARPointCloudManager aRPointCloudManager;

    bool recording = false;

    float scale = 30;

    private List<float3> allPoints = new List<float3>();

    private void OnEnable()
    {
        aRPointCloudManager.pointCloudsChanged += PointCloudManager_pointCloudsChanged;
    }

    private void PointCloudManager_pointCloudsChanged(ARPointCloudChangedEventArgs obj)
    {
        if (recording)
        {
            List<ARPoint> addedPoints = new List<ARPoint>();

            foreach (var pointCloud in obj.added)
            {

                foreach (var pos in pointCloud.positions)
                {
                    ARPoint newPoint = new ARPoint(pos);
                    addedPoints.Add(newPoint);
                    allPoints.Add(pos);
                }
            }

            List<ARPoint> updatedPoints = new List<ARPoint>();
            foreach (var pointCloud in obj.updated)
            {
                foreach (var pos in pointCloud.positions)
                {
                    ARPoint newPoint = new ARPoint(pos);
                    updatedPoints.Add(newPoint);
                    allPoints.Add(pos);
                }
            }
        }
        
    }

    public void StartRecording()
    {
        allPoints.Clear();
        recording = true;

        aRPointCloudManager.enabled = true;
        aRPointCloudManager.SetTrackablesActive(true);
    }

    public void StopRecording()
    {
        recording = false;
        aRPointCloudManager.SetTrackablesActive(false);
        aRPointCloudManager.enabled = false;
    }

    public float3[] GetPoints()
    {
        float3[] points = new float3[allPoints.Count];
        for (int i = 0; i < allPoints.Count; i++)
        {
            points[i] = allPoints[i]*scale;
        }
        StopRecording();

        return points;
    }
}


public class ARPoint
{
    public float3 position;
    public float3 normal;

    public ARPoint(float3 pos)
    {
        position = pos;
    }

}

