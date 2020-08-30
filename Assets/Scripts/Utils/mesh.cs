using System.Collections.Generic;
using UnityEngine;

public struct MeshVertex
{
	public Vector3 xyz, normal;

	public MeshVertex(Vector3 _xyz, Vector3 _normal)
    {
		xyz = _xyz;
		normal = _normal;
	}
}



