using UnityEngine;

//public enum MonsterState { Patrolling, Searching, Chasing }
public enum MonsterState { Patrolling, Chasing }

public class Monster : Unit<Monster>
{
    // Data
    public int detectionRange = 3; // Detection range in rooms
    //public float patience = 5f; // Time in seconds to wait before switching to Searching

    private Room PRoom => this.player.currentRoom;
    private Vector3 PPos => this.player.transform.position;
    private Vector2Int PPosInt => new Vector2Int((int)this.PPos.x, (int)this.PPos.y);

    private MonsterState state = MonsterState.Patrolling;
    //private Room lastSeen = null; // Last known player position
    //private float waitTime = 0f; // Time spent waiting in a room
    public override RoomTileType type => RoomTileType.Skull;

    private MapGenerator map;
    private Player player;

    protected override void Start()
    {
        base.Start();

        this.map = MapGenerator.Instance;
        this.player = Player.Instance;
    }

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
            //MonsterState.Searching => this.SearchBehavior(),
            MonsterState.Chasing => this.ChaseBehavior(),
            _ => null,
        };
    }

    private Room PatrolBehavior()
    {
        Room room = null;

        // Choose a random direction from available doors
        int randDirIndex = Random.Range(0, this.currentRoom.doors.Count);
        RoomDoorDirection randomDir = this.currentRoom.doors[randDirIndex];
        room = this.GetRoomInDirection(randomDir);

        // Check for player detection
        if (this.LookForPlayer())
        {
            this.state = MonsterState.Chasing;
            //this.lastSeen = this.PRoom;
        }
        //else if (this.waitTime >= this.patience)
        //{
        //    this.state = MonsterState.Searching;
        //    this.waitTime = 0f;
        //}
        else
        {
            //this.waitTime += 1f / this.speed;
        }

        return room;
    }

    //private void SearchBehavior()
    //{
    //    if (this.lastSeen == null)
    //    { return; }

    //    // Move towards the last known player position
    //    RoomDoorDirection dir = this.GetDirectionToTarget(this.lastSeen);
    //    this.targetRoom = this.GetRoomInDirection(dir);

    //    // Check for player detection
    //    if (this.DetectPlayer())
    //    {
    //        this.state = MonsterState.Chasing;
    //        this.lastSeen = this.PRoom;
    //    }
    //    else if (this.currentRoom == this.lastSeen)
    //    {
    //        // Reached last known position, return to patrolling
    //        this.state = MonsterState.Patrolling;
    //    }
    //}

    private Room ChaseBehavior()
    {
        Room room = null;

        // Move towards the player's current position
        RoomDoorDirection dir = this.GetDirectionToTarget(this.PRoom);
        room = this.GetRoomInDirection(dir);

        // Check if the player is still in detection range
        if (!this.LookForPlayer())
        {
            //this.state = MonsterState.Searching;
            this.state = MonsterState.Patrolling;
            //this.lastSeen = this.PRoom;
        }
        else if (this.currentRoom == this.PRoom)
        {
            // Monster caught the player, trigger game over
            Debug.Log("Game Over! Monster caught the player.");
            //Player.PlayerDied
        }

        return room;
    }

    private bool LookForPlayer()
    {
        // Calculate the distance to the player
        float distance = Vector2Int.Distance(this.currentRoom.position, this.PPosInt);
        return distance <= detectionRange;
    }

    private Room GetRoomInDirection(RoomDoorDirection direction)
    {
        Vector2Int nextPos = this.currentRoom.position;
        switch (direction)
        {
        case RoomDoorDirection.Up:
            nextPos += Vector2Int.up * this.map.RoomSize.y;
            break;
        case RoomDoorDirection.Down:
            nextPos += Vector2Int.down * this.map.RoomSize.y;
            break;
        case RoomDoorDirection.Right:
            nextPos += Vector2Int.right * this.map.RoomSize.x;
            break;
        case RoomDoorDirection.Left:
            nextPos += Vector2Int.left * this.map.RoomSize.x;
            break;
        }

        // Find the room at the next position
        return this.map.roomDict[nextPos];
    }

    private RoomDoorDirection GetDirectionToTarget(Room targetRoom)
    {
        Vector2Int delta = targetRoom.position - this.currentRoom.position;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            return delta.x > 0 ? RoomDoorDirection.Right : RoomDoorDirection.Left;
        }
        else
        {
            return delta.y > 0 ? RoomDoorDirection.Up : RoomDoorDirection.Down;
        }
    }
}