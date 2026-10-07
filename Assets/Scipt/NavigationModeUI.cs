using UnityEngine;
using UnityEngine.UI;

public class NavigationModeUI : MonoBehaviour
{
    public VehicleRoadRules vehicleRules;
    public Button walkingButton;
    public Button vehicleButton;

    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;

    void OnEnable()
    {
        RefreshButtons();
    }

    public void SelectWalking()
    {
        if (vehicleRules == null)
            return;

        vehicleRules.vehicleMode = false;
        RefreshButtons();
    }

    public void SelectVehicle()
    {
        if (vehicleRules == null)
            return;

        vehicleRules.vehicleMode = true;
        RefreshButtons();
    }

    void RefreshButtons()
    {
        if (vehicleRules == null)
            return;

        SetButtonColor(walkingButton, !vehicleRules.vehicleMode);
        SetButtonColor(vehicleButton, vehicleRules.vehicleMode);
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
}