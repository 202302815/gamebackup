using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementAreaRules : MonoBehaviour
{
    public VehicleRoadRules vehicleRules;
    public Tilemap walkable;
    public TMP_Text transitionMessage;

    private Rigidbody2D rb;
    private bool initialized;
    private bool lastVehicleMode;
    private bool transitioning;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    bool OnMap(Tilemap map, Vector2 position)
    {
        if (map == null)
            return false;

        Vector3 point = new Vector3(position.x, position.y, 0f);
        return map.HasTile(map.WorldToCell(point));
    }

    bool OnSelectedArea(Vector2 position)
    {
        return vehicleRules.vehicleMode
            ? OnMap(vehicleRules.vehicleRoad, position)
            : OnMap(walkable, position);
    }

    void RefreshState(Vector2 position)
    {
        bool mode = vehicleRules.vehicleMode;

        if (!initialized || mode != lastVehicleMode)
        {
            initialized = true;
            lastVehicleMode = mode;
            transitioning = !OnSelectedArea(position);
        }

        // Lock the new restriction only after actually arriving.
        if (transitioning && OnSelectedArea(position))
            transitioning = false;
    }

    void Update()
    {
        if (vehicleRules == null || walkable == null ||
            vehicleRules.vehicleRoad == null || rb == null)
            return;

        RefreshState(rb.position);

        if (transitionMessage == null)
            return;

        bool show = transitioning && Time.timeScale > 0f;

        transitionMessage.gameObject.SetActive(show);

        if (show)
        {
            transitionMessage.text = vehicleRules.vehicleMode
                ? "Move to a vehicle road."
                : "Move to a sidewalk or pedestrian area.";
        }
    }

    bool AllowedPosition(Vector2 position, ref bool pending)
    {
        bool selectedArea = OnSelectedArea(position);

        if (!pending)
            return selectedArea;

        bool onEitherArea =
            OnMap(walkable, position) ||
            OnMap(vehicleRules.vehicleRoad, position);

        if (!onEitherArea)
            return false;

        // Within this proposed movement, don't pass through
        // the target area and leave it again.
        if (selectedArea)
            pending = false;

        return true;
    }

    float SmallestTileSize(Tilemap map, Vector2 position)
    {
        Vector3Int cell = map.WorldToCell(
            new Vector3(position.x, position.y, 0f));

        Vector3 center = map.GetCellCenterWorld(cell);

        float width = Vector2.Distance(
            center,
            map.GetCellCenterWorld(cell + Vector3Int.right));

        float height = Vector2.Distance(
            center,
            map.GetCellCenterWorld(cell + Vector3Int.up));

        return Mathf.Min(width, height);
    }

    Vector2 CheckMovement(Vector2 start, Vector2 delta)
    {
        if (delta.sqrMagnitude < 0.000001f)
            return Vector2.zero;

        float tileSize = Mathf.Min(
            SmallestTileSize(walkable, start),
            SmallestTileSize(vehicleRules.vehicleRoad, start));

        float sampleDistance = Mathf.Max(tileSize * 0.1f, 0.001f);
        int steps = Mathf.Max(
            1, Mathf.CeilToInt(delta.magnitude / sampleDistance));

        Vector2 accepted = start;
        bool pending = transitioning;

        for (int i = 1; i <= steps; i++)
        {
            Vector2 candidate = start + delta * ((float)i / steps);
            bool nextPending = pending;

            // Check sideways edges too, preventing diagonal
            // shortcuts through an unpainted corner.
            if (delta.x != 0f && delta.y != 0f)
            {
                bool probeX = pending;
                bool probeY = pending;

                if (!AllowedPosition(
                        new Vector2(candidate.x, accepted.y),
                        ref probeX) ||
                    !AllowedPosition(
                        new Vector2(accepted.x, candidate.y),
                        ref probeY))
                    break;
            }

            if (!AllowedPosition(candidate, ref nextPending))
                break;

            accepted = candidate;
            pending = nextPending;
        }

        return accepted - start;
    }

    public Vector2 LimitMovement(
        Vector2 position,
        Vector2 velocity,
        float stepTime)
    {
        if (vehicleRules == null || walkable == null ||
            vehicleRules.vehicleRoad == null || stepTime <= 0f)
            return Vector2.zero;

        RefreshState(position);

        Vector2 requested = velocity * stepTime;
        Vector2 allowed = CheckMovement(position, requested);

        if ((allowed - requested).sqrMagnitude > 0.000001f)
        {
            // Try sliding horizontally or vertically.
            Vector2 horizontal = CheckMovement(
                position, new Vector2(requested.x, 0f));

            Vector2 vertical = CheckMovement(
                position, new Vector2(0f, requested.y));

            if (horizontal.sqrMagnitude > allowed.sqrMagnitude)
                allowed = horizontal;

            if (vertical.sqrMagnitude > allowed.sqrMagnitude)
                allowed = vertical;
        }

        return allowed / stepTime;
    }
}