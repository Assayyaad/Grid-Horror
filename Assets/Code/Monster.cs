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
    }

    void Start()
    {
        this.map = MapGenerator.Instance;
        this.player = Player.Instance;

        this.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.Tiles[(int)RoomTileType.Skull];
    }

    public void WakeUp(Room room)
    {
        // Set the monster's starting position to a random room
        this.currentRoom = room;
        this.transform.position = new Vector3(room.position.x + 0.5f, room.position.y + 0.5f, 0);

        // Start the monster's behavior loop
        this.behaviorLoop = this.StartCoroutine(this.MonsterBehaviorLoop());
    }

    private IEnumerator MonsterBehaviorLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f / this.speed); // Wait based on monster speed

            switch (this.state)
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

    private void PatrolBehavior()
    {
        // Choose a random direction from available doors
        int randDirIndex = Random.Range(0, this.currentRoom.doors.Count);
        RoomDoorDirection randomDir = this.currentRoom.doors[randDirIndex];
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

    private void SearchBehavior()
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

    private void ChaseBehavior()
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

    private void MoveToRoom(Room room)
    {
        this.currentRoom = room;
        this.transform.position = new Vector3(room.position.x + 0.5f, room.position.y + 0.5f, 0);
    }

    private bool DetectPlayer()
    {
        // Calculate the distance to the player
        float distance = Vector2Int.Distance(this.currentRoom.position, new Vector2Int((int)this.PPos.x, (int)this.PPos.y));
        return distance <= detectionRange;
    }

    private Room GetRoomInDirection(Vector2Int position, RoomDoorDirection direction)
    {
        Vector2Int nextPos = position;
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
        return this.map.allRooms.Find(room => room.position == nextPos);
    }

    private RoomDoorDirection GetDirectionToTarget(Vector2Int currentPos, Vector2Int targetPos)
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