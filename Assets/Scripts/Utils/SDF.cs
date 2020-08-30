
using UnityEngine;
using Unity.Mathematics;


public static class SDF
{
    public static float Sphere(Vector3 worldPosition, Vector3 origin, float radius)
    {

        return Vector3.Magnitude(worldPosition - origin) - radius;
    }

    public static float Terrain(Vector3 worldPosition)
    {
        Vector3 vec = new Vector3(worldPosition.x, worldPosition.y, worldPosition.z);
        float density = worldPosition.y - 6.0f;

        density += Noise(worldPosition, 0.1f, 7f);

        return density;
    }

    public static float Noise(Vector3 worldPosition, float amplitude, float frequency)
    {
        
        float density = noise.snoise(worldPosition * amplitude) * frequency;

        return density;
    }

    public static float Box(float3 p, float3 b)
    {
        float3 q = math.abs(p) - b;
        return math.length(math.max(q, 0.0f)) + math.min(math.max(q.x, math.max(q.y, q.z)), 0.0f);
    }

    public static float Density_Func(Vector3 worldPosition)
    {
        float sphere = Sphere(worldPosition, new Vector3(0,0,0), 5);
        float terrain = Terrain(worldPosition);
        float noise = Noise(worldPosition, 0.09f, 100f);
        float box = Box(worldPosition, new float3(5,5,5));

        return terrain;
    }
    public static float Density_Func(float x, float y, float z)
    {
        
        return Density_Func(new Vector3(x,y,z));
    }
}