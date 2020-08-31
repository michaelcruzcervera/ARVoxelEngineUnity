using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialCell : MonoBehaviour
{
    [SerializeField]
    WorldManager worldManager;
    public void SetWorldMaterial()
    {
        worldManager = FindObjectOfType<WorldManager>();

        worldManager.SetWorldMaterial(this.GetComponentInChildren<MeshRenderer>().material);
    }
}
