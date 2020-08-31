using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenuToggle : MonoBehaviour
{
    [SerializeField]
    GameObject menu;
    [SerializeField]
    GameObject icon;

    public bool toggled = false;

    MenuToggle[] menus;

    [SerializeField]
    Color selected = Color.grey;
    [SerializeField]
    Color unselected = Color.white;

    private void Start()
    {
        menus = FindObjectsOfType<MenuToggle>();
    }

    public void ToggleMenu()
    {
        if (!menu.activeSelf)
        {
            UntoggleAllMenus();
            menu.SetActive(true);
            toggled = true;
            icon.GetComponent<Image>().color = selected;
        }
        else
        {
            menu.SetActive(false);
            toggled = false;
            icon.GetComponent<Image>().color = unselected;
        }
    }

    private void UntoggleAllMenus()
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].toggled)
            {
                menus[i].ToggleMenu();

            }
        }
    }
}
