using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlBar : MonoBehaviour
{
    [SerializeField]
    GameObject menuReconstruction;

    [SerializeField]
    GameObject menuEditing;

    [SerializeField]
    GameObject crosshair;

    [SerializeField]
    GameObject icon;

    [SerializeField]
    Color selected = Color.grey;
    [SerializeField]
    Color unselected = Color.white;

    public void ToggleMenu()
    {
        if (menuEditing.activeSelf)
        {
            menuReconstruction.SetActive(true);
            menuEditing.SetActive(false);
            crosshair.SetActive(false);
            icon.GetComponent<Image>().color = unselected;
        }
        else
        {
            menuEditing.SetActive(true);
            menuReconstruction.SetActive(false);
            crosshair.SetActive(true);
            icon.GetComponent<Image>().color = selected;
        }
    }
}
