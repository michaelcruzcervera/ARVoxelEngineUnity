using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PopulateGrid : MonoBehaviour
{
    public GameObject prefab;

    [SerializeField]
    List<Material> materials;

    // Start is called before the first frame update
    void Start()
    {
        Populate();
    }

    private void Populate()
    {
        GameObject newObj;

        for (int i = 0;  i < materials.Count; i++)
        {
            newObj = Instantiate<GameObject>(prefab, transform);
            newObj.GetComponentInChildren<MeshRenderer>().material = materials[i];
            newObj.GetComponentInChildren<TMP_Text>().text= materials[i].name;
        }
    }
}
