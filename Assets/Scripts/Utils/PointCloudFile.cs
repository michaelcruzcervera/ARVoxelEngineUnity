using UnityEngine;
using System.IO;
using Unity.Mathematics;

public static class PointCloudFile
{
   
    static public void SerializeData(string fileName, float3[] points)
    {
        if (fileName != null && points.Length!=0)
        {
            string path = Path.Combine(Application.absoluteURL, "Assets\\Data\\Example", fileName+".pts");

            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter file = File.CreateText(path))
                {
                    file.WriteLine(points.Length);
                    for (int i = 0; i < points.Length; i++)
                    {
                        file.WriteLine(Vector3ToString(points[i]));
                        //Debug.Log(Vector3ToString(points[i]));
                    }

                    Debug.Log("File Saved: " + fileName);

                }
            }
            else
            {
                Debug.Log("Error: The file " + fileName + " already exists.");
            }

            
        }
        else
        {
            Debug.Log("Error: Values cannot be null");
        }
        
    }

        public static float3[] DeserializeData(string fileName, int scale)
    {
        if (fileName!=null)
        {
            string path = Path.Combine(Application.absoluteURL, "Assets\\Data\\Example", fileName + ".pts");
            Debug.Log(path);

            System.IO.StreamReader file = new System.IO.StreamReader(@path);
            int length = int.Parse(file.ReadLine());
            float3[] points = new float3[length];


            string line = "";
            for (int i = 0; i < length; i++)
            {
                line = file.ReadLine();
                points[i] = StringToVector3(line) * scale;
                //Debug.Log(line);
            }

            return points;
        }
        else
        {
            Debug.Log("Error: File Name cannot be null");
            return null;
        }
        
    }

    public static float3 StringToVector3(string sVector)
    {
        //Split the string into substrings
        string[] sArray = sVector.Split(' ');

        // store as a float3
        float3 result = new float3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

    public static string Vector3ToString(float3 vector)
    {
        return vector.x + " " + vector.y + " " + vector.z;
    }
}
