using System.Collections;

using UnityEngine;

public enum MonsterState { Patrolling, Searching, Chasing }

public class Monster : Singleton<Monster>
{
    // Data
    public float speed = 1f; // Speed in rooms per second
    public int detectionRange = 3; // Detection range in rooms
    public float patience = 5f; // Time in seconds to wait before switching to Searching

    private Vector3 PPos => this.player.transform.position;

    private Room currentRoom;
    private Room targetRoom;
    private MonsterState state = MonsterState.Patrolling;
    private Vector2Int? lastSeen = null; // Last known player position
    private float waitTime = 0f; // Time spent waiting in a room

    private Coroutine behaviorLoop;
    private MapGenerator map;
    private Player player;

    protected override void Awake()
    {
        base.Awake();
        this.map = MapGenerator.Instance;
        this.player = Player.Instance;
    }

    void Start()
    {
        this.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.Tiles[(int)RoomTileType.Skull];

        // Initialize the world and rooms
        this.InitializeWorld();

        // Set the monster's starting position to a random room
        this.currentRoom = this.map.allRooms[Random.Range(0, this.map.allRooms.Count)];
        this.transform.position = new Vector3(this.currentRoom.position.x + 0.5f, this.currentRoom.position.y + 0.5f, 0);

        // Find the player in the scene
        this.player = GameObject.FindFirstObjectByType<Player>(FindObjectsInactive.Include);

        // Start the monster's behavior loop
        this.behaviorLoop = this.StartCoroutine(this.MonsterBehaviorLoop());
    }

    void InitializeWorld()
    {
        // Generate a simple grid of rooms
        for (int x = 0; x < this.map.WorldSize.x; x++)
        {
            for (int y = 0; y < this.map.WorldSize.y; y++)
            {
                Room room = new Room(new Vector2Int(x, y));

                // Add doors to all adjacent rooms (simplified for this example)
                if (x > 0)
                {
                    room.doors.Add(RoomDoorDirection.Left);
                }

                if (x < this.map.WorldSize.x - 1)
                {
                    room.doors.Add(RoomDoorDirection.Right);
                }

                if (y > 0)
                {
                    room.doors.Add(RoomDoorDirection.Down);
                }

                if (y < this.map.WorldSize.y - 1)
                {
                    room.doors.Add(RoomDoorDirection.Up);
                }

                this.map.allRooms.Add(room);
            }
        }
    }

    IEnumerator MonsterBehaviorLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f / speed); // Wait based on monster speed

            switch (state)
            {
            case MonsterState.Patrolling:
                this.PatrolBehavior();
                break;
            case MonsterState.Searching:
                this.SearchBehavior();
                break;
            case MonsterState.Chasing:
                this.ChaseBehavior();
                break;
            }
        }
    }

    void PatrolBehavior()
    {
        // Choose a random direction from available doors
        RoomDoorDirection randomDir = this.currentRoom.doors[Random.Range(0, this.currentRoom.doors.Count)];
        this.targetRoom = this.GetRoomInDirection(this.currentRoom.position, randomDir);

        // Move the monster to the target room
        this.MoveToRoom(targetRoom);

        // Check for player detection
        if (this.DetectPlayer())
        {
            this.state = MonsterState.Chasing;
            this.lastSeen = new Vector2Int((int)this.PPos.x, (int)this.PPos.y);
        }
        else if (this.waitTime >= this.patience)
        {
            this.state = MonsterState.Searching;
            this.waitTime = 0f;
        }
        else
        {
            this.waitTime += 1f / this.speed;
        }
    }

    void SearchBehavior()
    {
        if (lastSeen.HasValue)
        {
            // Move towards the last known player position
            RoomDoorDirection direction = this.GetDirectionToTarget(this.currentRoom.position, lastSeen.Value);
            this.targetRoom = this.GetRoomInDirection(this.currentRoom.position, direction);

            // Move the monster to the target room
            this.MoveToRoom(this.targetRoom);

            // Check for player detection
            if (this.DetectPlayer())
            {
                this.state = MonsterState.Chasing;
                this.lastSeen = new Vector2Int((int)this.PPos.x, (int)this.PPos.y);
            }
            else if (this.currentRoom.position == this.lastSeen.Value)
            {
                // Reached last known position, return to patrolling
                this.state = MonsterState.Patrolling;
            }
        }
    }

    void ChaseBehavior()
    {
        // Move towards the player's current position
        Vector2Int playerPos = new Vector2Int((int)this.PPos.x, (int)this.PPos.y);
        RoomDoorDirection direction = this.GetDirectionToTarget(this.currentRoom.position, playerPos);
        this.targetRoom = this.GetRoomInDirection(this.currentRoom.position, direction);

        // Move the monster to the target room
        this.MoveToRoom(this.targetRoom);

        // Check if the player is still in detection range
        if (!this.DetectPlayer())
        {
            this.state = MonsterState.Searching;
            this.lastSeen = playerPos;
        }
        else if (this.currentRoom.position == playerPos)
        {
            // Monster caught the player, trigger game over
            Debug.Log("Game Over! Monster caught the player.");
            // Implement game over logic here
        }
    }

    void MoveToRoom(Room room)
    {
        this.currentRoom = room;
        this.transform.position = new Vector3(room.position.x + 0.5f, room.position.y + 0.5f, 0);
    }

    bool DetectPlayer()
    {
        // Calculate the distance to the player
        float distance = Vector2Int.Distance(this.currentRoom.position, new Vector2Int((int)this.PPos.x, (int)this.PPos.y));
        return distance <= detectionRange;
    }

    Room GetRoomInDirection(Vector2Int position, RoomDoorDirection direction)
    {
        Vector2Int nextPos = position;
        switch (direction)
        {
        case RoomDoorDirection.Up:
            nextPos += new Vector2Int(0, 1);
            break;
        case RoomDoorDirection.Right:
            nextPos += new Vector2Int(1, 0);
            break;
        case RoomDoorDirection.Down:
            nextPos += new Vector2Int(0, -1);
            break;
        case RoomDoorDirection.Left:
            nextPos += new Vector2Int(-1, 0);
            break;
        }

        // Find the room at the next position
        return this.map.allRooms.Find(room => room.position == nextPos);
    }

    RoomDoorDirection GetDirectionToTarget(Vector2Int currentPos, Vector2Int targetPos)
    {
        Vector2Int delta = targetPos - currentPos;
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