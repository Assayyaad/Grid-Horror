using System.Collections.Generic;

using UnityEngine;

public enum MonsterState { Patrolling, Chasing }

public class Monster : Unit<Monster>
{
    // Data
    public int detectionRange = 3; // Detection range in rooms

    private List<Room> currentPath = new();
    private int currentPathIndex = 0;

    private Room PRoom => Player.Instance.currentRoom;

    private MonsterState state = MonsterState.Patrolling;
    private Room lastSeen = null;
    [SerializeField]
    private int scanDelay = 3;
    private int waitTime = 0;

    public override RoomTileType type => RoomTileType.Skull;

    public void WakeUp(Room room)
    {
        // Set the monster's starting position to a random room
        this.currentRoom = room;
        this.transform.position = new Vector3(room.position.x + 0.5f, room.position.y + 0.5f, 0);
    }

    protected override Room ChooseTargetRoom()
    {
        if (!Player.Instance.gameObject.activeSelf)
        {
            return null;
        }

        return this.state switch
        {
            MonsterState.Patrolling => this.PatrolBehavior(),
            MonsterState.Chasing => this.ChaseBehavior(),
            _ => null,
        };
    }

    private Room PatrolBehavior()
    {
        if (this.LookForPlayer())
        { return Chase(this.PRoom); }

        this.waitTime++;
        if (this.waitTime >= this.scanDelay)
        { return Chase(Player.Instance.currentRoom); }

        // Choose a random direction from available doors
        int randDirIndex = Random.Range(0, this.currentRoom.doors.Count);
        RoomDoorDirection randomDir = this.currentRoom.doors[randDirIndex];

        return this.GetRoomInDirection(randomDir);

        Room Chase(Room room)
        {
            this.waitTime = 0;
            this.lastSeen = room;

            this.currentPath = AStar.FindPath(this.currentRoom, this.lastSeen);
            this.currentPathIndex = 0;

            this.state = MonsterState.Chasing;
            return this.ChaseBehavior();
        }
    }

    private Room ChaseBehavior()
    {
        if (this.lastSeen != this.PRoom && this.LookForPlayer())
        {
            this.lastSeen = this.PRoom;

            this.currentPath = AStar.FindPath(this.currentRoom, this.lastSeen);
            this.currentPathIndex = 0;
        }
        else if (this.currentRoom == this.lastSeen)
        {
            this.lastSeen = null;
        }

        if (this.lastSeen == null || this.currentPath == null || this.currentPathIndex >= this.currentPath.Count)
        {
            this.state = MonsterState.Patrolling;
            return this.PatrolBehavior();
        }

        return this.currentPath[this.currentPathIndex++];
    }

    private bool LookForPlayer()
    {
        Vector2Int size = MapGenerator.Instance.RoomSize;

        float dis = Vector2Int.Distance(this.currentRoom.position, this.PRoom.position);
        dis /= size.magnitude;
        return dis <= detectionRange;
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
}