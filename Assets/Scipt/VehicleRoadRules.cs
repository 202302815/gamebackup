using UnityEngine;
using UnityEngine.Tilemaps;

public class VehicleRoadRules : MonoBehaviour
{
    public bool vehicleMode;

    public PlayerMovement player;
    public GameObject wrongWayMessage;

    public Tilemap vehicleRoad;
    public Tilemap vehicleUp;
    public Tilemap vehicleDown;
    public Tilemap vehicleLeft;
    public Tilemap vehicleRight;
    public Tilemap[] parkingZones;

    private bool wrongWay;

    public bool AllowsDirection(Vector3Int cell, Vector3Int direction)
    {
        if (direction == Vector3Int.up)
            return vehicleUp != null && vehicleUp.HasTile(cell);

        if (direction == Vector3Int.down)
            return vehicleDown != null && vehicleDown.HasTile(cell);

        if (direction == Vector3Int.left)
            return vehicleLeft != null && vehicleLeft.HasTile(cell);

        if (direction == Vector3Int.right)
            return vehicleRight != null && vehicleRight.HasTile(cell);

        return false;
    }

    public int GetParkingZone(Vector3Int cell)
    {
        if (parkingZones == null || vehicleRoad == null)
            return -1;

        Vector3 worldPosition = vehicleRoad.GetCellCenterWorld(cell);

        for (int i = 0; i < parkingZones.Length; i++)
        {
            Tilemap zone = parkingZones[i];

            if (zone != null &&
                zone.HasTile(zone.WorldToCell(worldPosition)))
            {
                return i;
            }
        }

        return -1; // Main road, outside parking.
    }

    public bool CanTravelForRoute(
        Vector3Int from,
        Vector3Int to,
        int startZone,
        int goalZone)
    {
        if (!CanTravel(from, to))
            return false;

        int fromZone = GetParkingZone(from);
        int toZone = GetParkingZone(to);

        // Only endpoint parking areas can be used.
        if (fromZone >= 0 &&
            fromZone != startZone &&
            fromZone != goalZone)
            return false;

        if (toZone >= 0 &&
            toZone != startZone &&
            toZone != goalZone)
            return false;

        if (fromZone != toZone)
        {
            // Leave only the starting parking area.
            // Once destination parking is entered, stay inside.
            if (fromZone >= 0 &&
                (fromZone != startZone || fromZone == goalZone))
                return false;

            // Enter only destination parking.
            // Do not re-enter the starting parking area.
            if (toZone >= 0 &&
                (toZone != goalZone || toZone == startZone))
                return false;
        }

        return true;
    }

    public bool CanTravel(Vector3Int from, Vector3Int to)
    {
        if (vehicleRoad == null ||
            !vehicleRoad.HasTile(from) ||
            !vehicleRoad.HasTile(to))
            return false;

        Vector3Int movement = to - from;

        // Only adjacent horizontal or vertical tiles.
        if (Mathf.Abs(movement.x) + Mathf.Abs(movement.y) != 1 ||
            movement.z != 0)
            return false;

        // Forward travel or a permitted turn.
        if (AllowsDirection(from, movement))
        {
            bool forward = AllowsDirection(to, movement);
            bool backward = AllowsDirection(to, -movement);

            return forward || !backward;
        }

        // Do not treat backward movement as a lane change.
        if (AllowsDirection(from, -movement))
            return false;

        // Sideways movement requires matching traffic directions
        // on both the current and neighboring tiles.
        Vector3Int[] flows =
        {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right
    };

        foreach (Vector3Int flow in flows)
        {
            bool sideways =
                movement.x * flow.x + movement.y * flow.y == 0;

            if (sideways &&
                AllowsDirection(from, flow) &&
                AllowsDirection(to, flow))
            {
                return true;
            }
        }

        return false;
    }
    void LateUpdate()
    {
        if (player == null || vehicleRoad == null)
        {
            SetWarning(false);
            return;
        }

        Vector3Int cell =
            vehicleRoad.WorldToCell(player.transform.position);

        if (!vehicleMode || !vehicleRoad.HasTile(cell))
        {
            SetWarning(false);
            return;
        }

        Vector2 facing = player.FacingDirection;

        bool facingAllowed = false;
        bool facingAgainst = false;

        CheckDirection(vehicleUp, cell, Vector2.up, facing,
            ref facingAllowed, ref facingAgainst);

        CheckDirection(vehicleDown, cell, Vector2.down, facing,
            ref facingAllowed, ref facingAgainst);

        CheckDirection(vehicleLeft, cell, Vector2.left, facing,
            ref facingAllowed, ref facingAgainst);

        CheckDirection(vehicleRight, cell, Vector2.right, facing,
            ref facingAllowed, ref facingAgainst);

        if (facingAllowed)
            SetWarning(false);
        else if (facingAgainst)
            SetWarning(true);

        // Facing sideways keeps the current warning state.
    }

    void CheckDirection(
        Tilemap map,
        Vector3Int cell,
        Vector2 allowedDirection,
        Vector2 facing,
        ref bool facingAllowed,
        ref bool facingAgainst)
    {
        if (map == null || !map.HasTile(cell))
            return;

        float dot = Vector2.Dot(facing, allowedDirection);

        if (dot > 0.1f)
            facingAllowed = true;
        else if (dot < -0.1f)
            facingAgainst = true;
    }

    void SetWarning(bool show)
    {
        wrongWay = show;

        if (wrongWayMessage != null &&
            wrongWayMessage.activeSelf != wrongWay)
        {
            wrongWayMessage.SetActive(wrongWay);
        }
    }
}