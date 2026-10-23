using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using TMPro;

[RequireComponent(typeof(LineRenderer))]
public class CampusPathfinding : MonoBehaviour
{
    public Tilemap walkable;
    public Transform player;
    public Transform destination;
    public GameObject arrivalMessage;
    public GameObject cancelRouteButton;
    public Transform directionArrow;
    public float arrowTurnSpeed = 360f;
    public TMP_Text routeErrorMessage;
    private LineRenderer trail;
    private Vector3Int lastPlayerCell;
    private bool arrived = false;
    private Coroutine arrivalRoutine;
    private BuildingDestination selectedBuilding;
    
    public float arrivalDistance = 0.3f;

    public VehicleRoadRules vehicleRules;

    private bool lastVehicleMode;

    public UISoundManager soundManager;

    private readonly List<Vector3Int> routeCells =
    new List<Vector3Int>();

    private readonly List<Vector3> routePoints =
        new List<Vector3>();

    private int nextWaypoint;

    // Measured in tile widths, rather than world units.
    public float rerouteDistanceInTiles = 1.5f;

    bool IsVehicleMode =>
        vehicleRules != null && vehicleRules.vehicleMode;

    Tilemap ActiveMap =>
        IsVehicleMode ? vehicleRules.vehicleRoad : walkable;

    IEnumerator HideArrivalAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        
        if (arrivalMessage != null)
            arrivalMessage.SetActive(false);

        arrivalRoutine = null;
    }
    void ShowRouteError(string message)
    {
        if (routeErrorMessage != null)
        {
            routeErrorMessage.text = message;
            routeErrorMessage.gameObject.SetActive(true);
        }

        if (cancelRouteButton != null)
            cancelRouteButton.SetActive(destination != null);
    }

    void ClearRouteError()
    {
        if (routeErrorMessage != null)
            routeErrorMessage.gameObject.SetActive(false);
    }

    float TileSize()
    {
        Tilemap map = ActiveMap;
        Vector3Int cell = map.WorldToCell(player.position);
        Vector3 center = map.GetCellCenterWorld(cell);

        float width = Vector2.Distance(
            center, map.GetCellCenterWorld(cell + Vector3Int.right));

        float height = Vector2.Distance(
            center, map.GetCellCenterWorld(cell + Vector3Int.up));

        return Mathf.Min(width, height);
    }

    void AdvanceWaypoints(Vector3Int currentCell)
    {
        if (routePoints.Count < 2)
            return;

        float tileSize = TileSize();
        float cornerTolerance = tileSize * 0.15f;
        float sidewaysTolerance = tileSize * rerouteDistanceInTiles;
        float comparisonMargin = tileSize * 0.05f;

        Vector2 position = player.position;

        while (nextWaypoint < routePoints.Count - 1)
        {
            Vector2 start = routePoints[nextWaypoint - 1];
            Vector2 corner = routePoints[nextWaypoint];
            Vector2 nextEnd = routePoints[nextWaypoint + 1];

            Vector2 segment = corner - start;
            float length = segment.magnitude;

            if (length < 0.001f)
            {
                nextWaypoint++;
                continue;
            }

            Vector2 forward = segment / length;
            Vector2 offset = position - start;

            float progress = Vector2.Dot(offset, forward);
            float sideways = Mathf.Abs(
                offset.x * forward.y - offset.y * forward.x);

            bool reachedCorner =
                Vector2.Distance(position, corner) <= cornerTolerance;

            bool passedCorner =
                progress >= length - cornerTolerance &&
                sideways <= sidewaysTolerance;

            // Also detect progress along the next connected segment.
            Vector2 nextSegment = nextEnd - corner;
            float nextLength = nextSegment.magnitude;
            bool followingNextSegment = false;

            if (nextLength > 0.001f)
            {
                Vector2 nextForward = nextSegment / nextLength;

                float nextProgress = Vector2.Dot(
                    position - corner, nextForward);

                Vector2 currentNearest =
                    start + forward * Mathf.Clamp(progress, 0f, length);

                Vector2 nextNearest =
                    corner + nextForward *
                    Mathf.Clamp(nextProgress, 0f, nextLength);

                float currentDistance =
                    Vector2.Distance(position, currentNearest);

                float nextDistance =
                    Vector2.Distance(position, nextNearest);

                followingNextSegment =
                    nextProgress > cornerTolerance &&
                    nextProgress <= nextLength + cornerTolerance &&
                    nextDistance <= sidewaysTolerance &&
                    nextDistance + comparisonMargin < currentDistance;
            }

            if (!reachedCorner &&
                !passedCorner &&
                !followingNextSegment)
            {
                break;
            }

            nextWaypoint++;
        }
    }

    Vector3 GetArrowTarget()
    {
        if (routePoints.Count < 2)
            return destination.position;

        Vector2 start = routePoints[nextWaypoint - 1];
        Vector2 end = routePoints[nextWaypoint];
        Vector2 segment = end - start;
        float length = segment.magnitude;

        if (length < 0.001f)
            return end;

        Vector2 forward = segment / length;
        Vector2 position = player.position;

        float progress = Vector2.Dot(position - start, forward);
        float remaining = length - progress;

        // At the final segment's end, guide to the actual destination.
        if (nextWaypoint == routePoints.Count - 1 &&
            remaining <= TileSize() * 0.15f)
        {
            return destination.position;
        }

        // Aim ahead of current progress, not at a tile behind the player.
        float lookAhead = TileSize();
        float targetProgress = Mathf.Clamp(
            progress + lookAhead, 0f, length);

        Vector2 target = start + forward * targetProgress;

        return new Vector3(target.x, target.y, 0f);
    }

    void RefreshRemainingTrail()
    {
        if (routePoints.Count == 0)
        {
            trail.positionCount = 0;
            return;
        }

        int remaining = routePoints.Count - nextWaypoint;
        trail.positionCount = remaining + 1;

        Vector3 position = player.position;
        position.z = 0f;
        trail.SetPosition(0, position);

        for (int i = 0; i < remaining; i++)
            trail.SetPosition(i + 1, routePoints[nextWaypoint + i]);
    }

    float DistanceFromRemainingRoute()
    {
        Vector2 position = player.position;
        float closest = float.PositiveInfinity;

        // Include the segment leading to the next waypoint.
        int first = Mathf.Max(0, nextWaypoint - 1);

        for (int i = first; i < routePoints.Count; i++)
        {
            Vector2 a = routePoints[i];

            closest = Mathf.Min(
                closest,
                Vector2.Distance(position, a)
            );

            if (i + 1 >= routePoints.Count)
                continue;

            Vector2 b = routePoints[i + 1];
            Vector2 segment = b - a;

            float t = segment.sqrMagnitude > 0f
                ? Mathf.Clamp01(
                    Vector2.Dot(position - a, segment) /
                    segment.sqrMagnitude)
                : 0f;

            Vector2 nearest = a + segment * t;

            closest = Mathf.Min(
                closest,
                Vector2.Distance(position, nearest)
            );
        }

        return closest;
    }

    public void SelectBuilding(BuildingDestination building)
    {
        if (building == null)
            return;

        Transform target = building.GetDestination(IsVehicleMode);

        if (target == null)
        {
            Debug.LogWarning("Assign both destination points for this building.");
            CancelRoute();
            return;
        }

        SelectDestination(target);
        selectedBuilding = building;
    }

    void ClearArrivalMessage()
    {
        if (arrivalRoutine != null)
        {
            StopCoroutine(arrivalRoutine);
            arrivalRoutine = null;
        }

        if (arrivalMessage != null)
            arrivalMessage.SetActive(false);
    }

    private readonly Vector3Int[] directions =
    {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right
    };

    void Awake()
    {
        trail = GetComponent<LineRenderer>();
        trail.useWorldSpace = true;
        trail.startWidth = 0.3f;
        trail.endWidth = 0.3f;
        trail.startColor = Color.yellow;
        trail.endColor = Color.yellow;
        trail.sortingOrder = 100;
        trail.positionCount = 0;
    }

    void Start()
    {
        ClearRouteError();
        if (cancelRouteButton != null)
            cancelRouteButton.SetActive(false);
        if (walkable == null || player == null)
        {
            Debug.LogWarning("Assign Walkable and Player.");
            enabled = false;
            return;
        }

        destination = null;
        trail.positionCount = 0;
        lastPlayerCell = walkable.WorldToCell(player.position);
    }

    void Update()
    {
        bool modeChanged = lastVehicleMode != IsVehicleMode;
        lastVehicleMode = IsVehicleMode;

        if (modeChanged && selectedBuilding != null)
        {
            Transform target =
                selectedBuilding.GetDestination(IsVehicleMode);

            if (target == null)
            {
                Debug.LogWarning("This building has no destination for this mode.");
                CancelRoute();
                return;
            }

            destination = target;
        }

        if (destination == null)
            return;

        if (modeChanged)
        {
            arrived = false;
            ClearArrivalMessage();
        }

        if (arrived)
            return;

        Tilemap map = ActiveMap;

        if (map == null)
        {
            routePoints.Clear();
            trail.positionCount = 0;
            ShowRouteError("Navigation is unavailable for this mode.");
            return;
        }
        float distance = Vector2.Distance(
            player.position,
            destination.position
        );

        if (map.HasTile(map.WorldToCell(player.position)) &&
            map.HasTile(map.WorldToCell(destination.position)) &&
            distance <= arrivalDistance)
        {
            
            arrived = true;
            if (soundManager != null)
                soundManager.PlayArrivalSound();
            ClearRouteError();
            trail.positionCount = 0;

            if (cancelRouteButton != null)
                cancelRouteButton.SetActive(false);

            ClearArrivalMessage();

            if (arrivalMessage != null)
            {
                arrivalMessage.SetActive(true);
                arrivalRoutine = StartCoroutine(HideArrivalAfterDelay());
            }

            return;
        }

        Vector3Int currentCell = map.WorldToCell(player.position);
        bool cellChanged = currentCell != lastPlayerCell;
        bool parkingBoundaryCrossed =
            IsVehicleMode &&
            cellChanged &&
            vehicleRules.GetParkingZone(currentCell) !=
            vehicleRules.GetParkingZone(lastPlayerCell);
        if (modeChanged)
        {
            lastPlayerCell = currentCell;
            ShowPath();
            return;
        }

        AdvanceWaypoints(currentCell);

        if (cellChanged)
        {
            lastPlayerCell = currentCell;

            Vector3 center = map.GetCellCenterWorld(currentCell);

            float tileWidth = Vector2.Distance(
                center,
                map.GetCellCenterWorld(currentCell + Vector3Int.right)
            );

            float tileHeight = Vector2.Distance(
                center,
                map.GetCellCenterWorld(currentCell + Vector3Int.up)
            );

            float rerouteDistance =
                Mathf.Min(tileWidth, tileHeight) * rerouteDistanceInTiles;

            bool needsNewRoute =
                routePoints.Count == 0 ||
                !map.HasTile(currentCell) ||
                parkingBoundaryCrossed ||
                DistanceFromRemainingRoute() > rerouteDistance;

            if (needsNewRoute)
                ShowPath();
        }
    }

    void LateUpdate()
    {
        bool navigating =
            destination != null &&
            !arrived &&
            routePoints.Count > 0 &&
            ActiveMap != null;

        if (navigating)
            RefreshRemainingTrail();

        if (directionArrow == null)
            return;

        directionArrow.gameObject.SetActive(navigating);

        if (!navigating)
            return;

        Vector2 direction =
            (Vector2)(GetArrowTarget() - player.position);

        if (direction.sqrMagnitude < 0.0001f)
            return;

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        directionArrow.rotation = Quaternion.RotateTowards(
            directionArrow.rotation,
            Quaternion.Euler(0f, 0f, angle),
            arrowTurnSpeed * Time.deltaTime
        );
    }

    public void CancelRoute()
    {
        ClearRouteError();
        selectedBuilding = null;
        if (cancelRouteButton != null)
            cancelRouteButton.SetActive(false);
        ClearArrivalMessage();
        destination = null;
        arrived = false;
        trail.positionCount = 0;
    }
    public void SelectDestination(Transform newDestination)
    {
        selectedBuilding = null;
        if (newDestination == null || ActiveMap == null || player == null)
            return;

        ClearArrivalMessage();

        destination = newDestination;
        arrived = false;
        lastPlayerCell = ActiveMap.WorldToCell(player.position);

        ShowPath();

        if (cancelRouteButton != null)
            cancelRouteButton.SetActive(destination != null);   
    }

    public void ShowPath()
    {
        trail.positionCount = 0;

        routeCells.Clear();
        routePoints.Clear();
        nextWaypoint = 0;

        Tilemap map = ActiveMap;

        if (player == null || destination == null)
        {
            ShowRouteError("Select a building to start navigation.");
            return;
        }

        if (map == null)
        {
            ShowRouteError("Navigation is unavailable for this mode.");
            return;
        }

        Vector3Int start = map.WorldToCell(player.position);
        Vector3Int goal = map.WorldToCell(destination.position);

        if (!map.HasTile(start))
        {
            ShowRouteError(IsVehicleMode
                ? "Move onto a vehicle road to start navigation."
                : "Move onto a walking path to start navigation.");
            return;
        }

        if (!map.HasTile(goal))
        {
            ShowRouteError(IsVehicleMode
                ? "This destination has no valid vehicle arrival point."
                : "This destination has no valid walking arrival point.");
            return;
        }
        int startParkingZone = IsVehicleMode
            ? vehicleRules.GetParkingZone(start)
            : -1;

        int goalParkingZone = IsVehicleMode
            ? vehicleRules.GetParkingZone(goal)
            : -1;
        List<Vector3Int> open = new List<Vector3Int> { start };
        HashSet<Vector3Int> closed = new HashSet<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> parents =
            new Dictionary<Vector3Int, Vector3Int>();
        Dictionary<Vector3Int, int> costs =
            new Dictionary<Vector3Int, int>();

        costs[start] = 0;

        while (open.Count > 0)
        {
            Vector3Int current = open[0];

            foreach (Vector3Int cell in open)
            {
                int score = costs[cell] + Distance(cell, goal);
                int bestScore = costs[current] + Distance(current, goal);

                if (score < bestScore)
                    current = cell;
            }

            if (current == goal)
            {
                DrawPath(parents, start, goal);
                return;
            }

            open.Remove(current);
            closed.Add(current);

            foreach (Vector3Int direction in directions)
            {
                Vector3Int next = current + direction;

                if (!map.HasTile(next) || closed.Contains(next))
                    continue;

                if (IsVehicleMode &&
                    !vehicleRules.CanTravelForRoute(
                        current,
                        next,
                        startParkingZone,
                        goalParkingZone))
                {
                    continue;
                }

                int newCost = costs[current] + 1;

                if (!costs.ContainsKey(next) || newCost < costs[next])
                {
                    costs[next] = newCost;
                    parents[next] = current;

                    if (!open.Contains(next))
                        open.Add(next);
                }
            }
        }

        ShowRouteError(IsVehicleMode
    ? "No permitted vehicle route to this destination."
    : "No walking route to this destination.");
    }

    int Distance(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    void DrawPath(
    Dictionary<Vector3Int, Vector3Int> parents,
    Vector3Int start,
    Vector3Int goal)
    {
        ClearRouteError();
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int current = goal;
        path.Add(current);

        while (current != start)
        {
            current = parents[current];
            path.Add(current);
        }

        path.Reverse();
        routeCells.Clear();
        routePoints.Clear();

        for (int i = 0; i < path.Count; i++)
        {
            bool keep = i == 0 || i == path.Count - 1;

            if (!keep)
            {
                Vector3Int incoming = path[i] - path[i - 1];
                Vector3Int outgoing = path[i + 1] - path[i];
                keep = incoming != outgoing;
            }

            if (!keep)
                continue;

            routeCells.Add(path[i]);

            Vector3 point = ActiveMap.GetCellCenterWorld(path[i]);
            point.z = 0f;
            routePoints.Add(point);
        }

        nextWaypoint = routePoints.Count > 1 ? 1 : 0;
        lastPlayerCell = ActiveMap.WorldToCell(player.position);

        RefreshRemainingTrail();

        if (cancelRouteButton != null)
            cancelRouteButton.SetActive(true);
    }
}