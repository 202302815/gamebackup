using UnityEngine;

public class SearchMenuUI : MonoBehaviour
{
    public GameObject searchMenu;
    void Start()
    {
        searchMenu.SetActive(false);
    }


    public void ToggleMenu()
    {
        searchMenu.SetActive(!searchMenu.activeSelf);
    }
}