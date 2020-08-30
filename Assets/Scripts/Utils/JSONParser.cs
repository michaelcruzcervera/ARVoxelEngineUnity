using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static  class JSONParser
{
    public static string path= Path.Combine(Application.absoluteURL, "Assets\\Data", "data.json");

    static public void SerializeData(JsonData data)
    {
        string jsonDataString = JsonUtility.ToJson(data, true);

        File.WriteAllText(path, jsonDataString);

        Debug.Log(jsonDataString);
    }

    static public void DeserializeData(JsonData data)
    {
        string loadedJsonDataString = File.ReadAllText(path);

        data = JsonUtility.FromJson<JsonData>(loadedJsonDataString);


        for (int i = 0; i < data.points.Length; i++)
        {
            Debug.Log("index: " + i + "| position: " + data.points[i]);
        }
    }
}


public class JsonData
{
    public Vector3[] points;

    public JsonData(Vector3[] points)
    {
        this.points = points;
    }

    public JsonData() { }
}
