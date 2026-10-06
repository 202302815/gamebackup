using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingSearch : MonoBehaviour
{
    public TMP_InputField searchInput;
    public Transform content;
    public ScrollRect scrollView;

    void OnEnable()
    {
        if (searchInput == null || content == null)
            return;

        searchInput.onValueChanged.AddListener(FilterBuildings);
        FilterBuildings(searchInput.text);
    }

    void OnDisable()
    {
        if (searchInput != null)
            searchInput.onValueChanged.RemoveListener(FilterBuildings);
    }

    void FilterBuildings(string searchText)
    {
        string query = searchText.Trim();

        foreach (Transform child in content)
        {
            Button button = child.GetComponent<Button>();

            if (button == null)
                continue;

            TMP_Text label = child.GetComponentInChildren<TMP_Text>(true);

            bool matches = query.Length == 0 ||
                (label != null &&
                 label.text.IndexOf(
                     query,
                     StringComparison.OrdinalIgnoreCase) >= 0);

            child.gameObject.SetActive(matches);
        }

        // Update the list height, then return to the top.
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            content.GetComponent<RectTransform>());

        if (scrollView != null)
            scrollView.verticalNormalizedPosition = 1f;
    }
}