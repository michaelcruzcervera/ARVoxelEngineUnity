using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

[RequireComponent(typeof(ParticleSystem))]
public sealed class PointCloudVisualiser : MonoBehaviour
{
    ParticleSystem m_ParticleSystem;

    ParticleSystem.Particle[] m_Particles;

    [SerializeField]
    int m_NumParticles;

    static List<float3> s_Vertices = new List<float3>();

    [SerializeField]
    public bool DrawPointCloud = true;

    public void SetPointCloud(float3[] pointCloud)
    {
        var points = s_Vertices;
        points.Clear();

        foreach (var point in pointCloud)
            s_Vertices.Add(point);
        
        int numParticles = points.Count;
        if (m_Particles == null || m_Particles.Length < numParticles)
            m_Particles = new ParticleSystem.Particle[numParticles];

        var color = m_ParticleSystem.main.startColor.color;
        var size = m_ParticleSystem.main.startSize.constant;
        
        for (int i = 0; i < numParticles; ++i)
        {
            m_Particles[i].startColor = color;
            m_Particles[i].startSize = size;
            m_Particles[i].position = points[i];
            m_Particles[i].remainingLifetime = 1f;
        }
        
        
        m_ParticleSystem.SetParticles(m_Particles, Math.Max(numParticles, m_NumParticles));
        m_NumParticles = numParticles;
    }

    public void Clear()
    {
        s_Vertices.Clear();
        m_Particles = new ParticleSystem.Particle[0];
        m_ParticleSystem.SetParticles(m_Particles,0);
    }

    void Awake()
    {
        m_ParticleSystem = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        m_ParticleSystem = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        UpdateVisibility();
    }

    void UpdateVisibility()
    {
        SetVisible(DrawPointCloud);
    }

    public void ToggleVisible()
    {
        if (DrawPointCloud)
        {
            DrawPointCloud = false;
        }
        else
        {
            DrawPointCloud = true;
        }
    }
    void SetVisible(bool visible)
    {
        if (m_ParticleSystem == null)
            return;

        var renderer = m_ParticleSystem.GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = visible;
    }
}
