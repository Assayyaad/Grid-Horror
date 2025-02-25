using System.Collections.Generic;

using UnityEngine;

//public enum MonsterState { Patrolling, Searching, Chasing }
public enum MonsterState { Patrolling, Chasing }

public class Monster : Unit<Monster>
{
    // Data
    public int detectionRange = 3; // Detection range in rooms
    //public float patience = 5f; // Time in seconds to wait before switching to Searching

    private Room PRoom => Player.Instance.currentRoom;

    private MonsterState state = MonsterState.Patrolling;
    private Room lastSeen = null;

    public override RoomTileType type => RoomTileType.Skull;

    public void WakeUp(Room room)
    {
        // Set the monster's starting position to a random room
        this.currentRoom = room;
        this.transform.position = new Vector3(room.position.x + 0.5f, room.position.y + 0.5f, 0);
    }

    protected override Room ChooseTargetRoom()
    {
        return this.state switch
        {
            MonsterState.Patrolling => this.PatrolBehavior(),
            MonsterState.Chasing => this.ChaseBehavior(),
            _ => null,
        };
    }

    private Room PatrolBehavior()
    {
        Room room = null;

        if (this.LookForPlayer())
        {
            this.state = MonsterState.Chasing;
            this.lastSeen = this.PRoom;
            return this.ChaseBehavior();
        }

        // Choose a random direction from available doors
        int randDirIndex = Random.Range(0, this.currentRoom.doors.Count);
        RoomDoorDirection randomDir = this.currentRoom.doors[randDirIndex];
        room = this.GetRoomInDirection(randomDir);

        return room;
    }

    private Room ChaseBehavior()
    {
        Room room = null;

        // Check if the player is still in detection range
        if (this.LookForPlayer())
        {
            this.lastSeen = this.PRoom;
        }
        else
        {
            this.state = MonsterState.Patrolling;
            this.lastSeen = null;
            return this.PatrolBehavior();
        }

        // Move towards the player's current position
        RoomDoorDirection dir = this.GetDirectionToTarget(this.lastSeen);
        room = this.GetRoomInDirection(dir);

        return room;
    }

    private bool LookForPlayer()
    {
        Vector2Int size = MapGenerator.Instance.RoomSize;

        float dis = Vector2Int.Distance(this.currentRoom.position, this.PRoom.position) / size.magnitude;
        int dis2 = Mathf.RoundToInt(dis);

        return dis2 <= detectionRange;
    }

    private Room GetRoomInDirection(RoomDoorDirection direction)
    {
        if (!this.currentRoom.doors.Contains(direction))
        {
            return null;
        }

        Vector2Int nextPos = this.currentRoom.position;
        switch (direction)
        {
        case RoomDoorDirection.Up:
            nextPos += Vector2Int.up * MapGenerator.Instance.RoomSize.y;
            break;
        case RoomDoorDirection.Down:
            nextPos += Vector2Int.down * MapGenerator.Instance.RoomSize.y;
            break;
        case RoomDoorDirection.Right:
            nextPos += Vector2Int.right * MapGenerator.Instance.RoomSize.x;
            break;
        case RoomDoorDirection.Left:
            nextPos += Vector2Int.left * MapGenerator.Instance.RoomSize.x;
            break;
        }

        return MapGenerator.Instance.roomDict[nextPos];
    }

    private RoomDoorDirection GetDirectionToTarget(Room targetRoom)
    {
        List<RoomDoorDirection> doors = this.currentRoom.doors;

        Vector2Int delta = targetRoom.position - this.currentRoom.position;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x > 0)
            {
                return RoomDoorDirection.Right;
            }
            else
            {
                return RoomDoorDirection.Left;
            }
        }
        else
        {
            if (delta.y > 0)
            {
                return RoomDoorDirection.Up;
            }
            else
            {
                return RoomDoorDirection.Down;
            }
        }
    }
}