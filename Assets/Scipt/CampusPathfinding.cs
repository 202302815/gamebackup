using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

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
    private LineRenderer trail;
    private Vector3Int lastPlayerCell;
    private bool arrived = false;
    private Coroutine arrivalRoutine;

    public float arrivalDistance = 0.3f;

    IEnumerator HideArrivalAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        
        if (arrivalMessage != null)
            arrivalMessage.SetActive(false);

        arrivalRoutine = null;
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
        trail.startWidth = 0.12f;
        trail.endWidth = 0.12f;
        trail.startColor = Color.yellow;
        trail.endColor = Color.yellow;
        trail.sortingOrder = 100;
        trail.positionCount = 0;
    }

    void Start()
    {
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
        if (destination == null || arrived)
            return;

        float distance = Vector2.Distance(
            player.position,
            destination.position
        );

        if (distance <= arrivalDistance)
        {
            arrived = true;
            trail.positionCount = 0;
            if (cancelRouteButton != null)
                cancelRouteButton.SetActive(false);
            Debug.Log("You have arrived!");
            ClearArrivalMessage();

            if (arrivalMessage != null)
            {
                arrivalMessage.SetActive(true);
                arrivalRoutine = StartCoroutine(HideArrivalAfterDelay());
            }
            return;
        }

        Vector3Int currentCell = walkable.WorldToCell(player.position);

        if (currentCell != lastPlayerCell)
        {
            lastPlayerCell = currentCell;
            ShowPath();
        }
    }

    void LateUpdate()
    {
        if (directionArrow == null)
            return;

        bool navigating =
            destination != null &&
            !arrived &&
            trail.positionCount > 0;

        directionArrow.gameObject.SetActive(navigating);

        if (!navigating)
            return;

        Vector3 target;

        if (trail.positionCount > 1)
        {
            // Point toward the next route tile.
            target = trail.GetPosition(1);
        }
        else
        {
            // In the final tile, point to the actual entrance.
            target = destination.position;
        }

        Vector2 direction = (Vector2)(target - player.position);

        if (direction.sqrMagnitude < 0.0001f)
            return;

        // The triangle's original direction is up.
        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        directionArrow.rotation = Quaternion.RotateTowards(
            directionArrow.rotation,
            targetRotation,
            arrowTurnSpeed * Time.deltaTime
        );
    }

    public void CancelRoute()
    {
        if (cancelRouteButton != null)
            cancelRouteButton.SetActive(false);
        ClearArrivalMessage();
        destination = null;
        arrived = false;
        trail.positionCount = 0;
    }
    public void SelectDestination(Transform newDestination)
    {
        if (newDestination == null || walkable == null || player == null)
            return;

        ClearArrivalMessage();

        destination = newDestination;
        arrived = false;
        lastPlayerCell = walkable.WorldToCell(player.position);

        ShowPath();

        if (cancelRouteButton != null)
            cancelRouteButton.SetActive(trail.positionCount > 0);
    }

    public void ShowPath()
    {
        trail.positionCount = 0;

        if (walkable == null || player == null || destination == null)
        {
            Debug.LogWarning("Assign Walkable, Player, and Destination.");
            return;
        }

        Vector3Int start = walkable.WorldToCell(player.position);
        Vector3Int goal = walkable.WorldToCell(destination.position);

        if (!walkable.HasTile(start) || !walkable.HasTile(goal))
        {
            Debug.LogWarning(
                "Player and destination must both be on Walkable tiles.");
            return;
        }

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

                if (!walkable.HasTile(next) || closed.Contains(next))
                    continue;

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

        Debug.LogWarning(
            "No route found. Check for gaps in your Walkable tiles.");
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
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int current = goal;
        path.Add(current);

        while (current != start)
        {
            current = parents[current];
            path.Add(current);
        }

        path.Reverse();
        trail.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 point = walkable.GetCellCenterWorld(path[i]);
            point.z = 0;
            trail.SetPosition(i, point);
        }

        Debug.Log("Route found!");
    }
}