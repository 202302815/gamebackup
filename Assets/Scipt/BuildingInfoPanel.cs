using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInfoPanel : MonoBehaviour
{
    public BuildingDatabase database;
    public TMP_Text titleText;
    public TMP_Text detailsText;

    public Button informationButton;
    public Button departmentsButton;
    public Button coursesButton;

    public ScrollRect detailsScroll;

    public Color selectedColor = new Color32(228, 190, 88, 255);
    public Color normalColor = Color.white;

    private string information;
    private string departments;
    private string courses;
    private Coroutine scrollRoutine;

    public bool DisplayBuilding(string buildingId)
    {
        if (database == null || titleText == null || detailsText == null)
        {
            Debug.LogWarning("Assign the database and panel text fields.");
            return false;
        }

        BuildingInfo building = database.GetBuilding(buildingId);

        if (building == null)
        {
            Debug.LogWarning("Building ID not found: " + buildingId);
            return false;
        }

        titleText.text = building.name;

        information = "";

        if (!string.IsNullOrWhiteSpace(building.description))
            information = building.description.Trim();

        if (!string.IsNullOrWhiteSpace(building.services))
        {
            if (information.Length > 0)
                information += "\n\n";

            information += "Services:\n" + building.services.Trim();
        }

        departments = building.departments;
        courses = building.coursesOffered;

        SetAvailable(informationButton, information);
        SetAvailable(departmentsButton, departments);
        SetAvailable(coursesButton, courses);

        // Reset to the first available tab for each building.
        if (!string.IsNullOrWhiteSpace(information))
            ShowInformation();
        else if (!string.IsNullOrWhiteSpace(departments))
            ShowDepartments();
        else if (!string.IsNullOrWhiteSpace(courses))
            ShowCourses();
        else
            ShowContent("No information available.", null);

        return true;
    }

    public void ShowInformation()
    {
        if (!string.IsNullOrWhiteSpace(information))
            ShowContent(information, informationButton);
    }

    public void ShowDepartments()
    {
        if (!string.IsNullOrWhiteSpace(departments))
            ShowContent(departments, departmentsButton);
    }

    public void ShowCourses()
    {
        if (!string.IsNullOrWhiteSpace(courses))
            ShowContent(courses, coursesButton);
    }

    void SetAvailable(Button button, string content)
    {
        if (button != null)
            button.gameObject.SetActive(
                !string.IsNullOrWhiteSpace(content));
    }

    void ShowContent(string content, Button selected)
    {
        detailsText.text = content;

        SetButtonColor(informationButton,
            selected == informationButton);
        SetButtonColor(departmentsButton,
            selected == departmentsButton);
        SetButtonColor(coursesButton,
            selected == coursesButton);

        RequestScrollReset();
    }

    void SetButtonColor(Button button, bool selected)
    {
        if (button == null)
            return;

        Color color = selected ? selectedColor : normalColor;
        ColorBlock colors = button.colors;

        colors.normalColor = color;
        colors.highlightedColor = color;
        colors.selectedColor = color;
        colors.pressedColor = color * 0.85f;

        button.colors = colors;
    }

    void OnEnable()
    {
        RequestScrollReset();
    }

    void OnDisable()
    {
        if (scrollRoutine != null)
            StopCoroutine(scrollRoutine);

        scrollRoutine = null;
    }

    void RequestScrollReset()
    {
        // Signs fill the panel before enabling it.
        // OnEnable handles the reset in that case.
        if (!isActiveAndEnabled)
            return;

        if (scrollRoutine != null)
            StopCoroutine(scrollRoutine);

        scrollRoutine = StartCoroutine(ResetScroll());
    }

    IEnumerator ResetScroll()
    {
        yield return null;

        if (detailsScroll != null && detailsScroll.content != null)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                detailsScroll.content);

            detailsScroll.StopMovement();
            detailsScroll.verticalNormalizedPosition = 1f;
        }

        scrollRoutine = null;
    }
}