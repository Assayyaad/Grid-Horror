using System.Collections;

using UnityEngine;

//public enum MonsterState { Patrolling, Searching, Chasing }
public enum MonsterState { Patrolling, Chasing }

public class Monster : Singleton<Monster>
{
    // Data
    public float speed = 1f; // Speed in rooms per second
    public int detectionRange = 3; // Detection range in rooms
    //public float patience = 5f; // Time in seconds to wait before switching to Searching

    private Room PRoom => this.player.currentRoom;
    private Vector3 PPos => this.player.transform.position;
    private Vector2Int PPosInt => new Vector2Int((int)this.PPos.x, (int)this.PPos.y);

    private MonsterState state = MonsterState.Patrolling;
    private Room currentRoom;
    private Room targetRoom;
    private Room lastSeen = null; // Last known player position
    private float waitTime = 0f; // Time spent waiting in a room

    private Coroutine behaviorLoop;
    private MapGenerator map;
    private Player player;

    private void Start()
    {
        this.map = MapGenerator.Instance;
        this.player = Player.Instance;

        this.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.Tiles[(int)RoomTileType.Skull];
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        this.StopCoroutine(this.behaviorLoop);
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
            //case MonsterState.Searching:
            //    this.SearchBehavior();
            //    break;
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
        this.targetRoom = this.GetRoomInDirection(randomDir);

        // Move the monster to the target room
        this.MoveToRoom(targetRoom);

        // Check for player detection
        if (this.DetectPlayer())
        {
            this.state = MonsterState.Chasing;
            this.lastSeen = this.PRoom;
        }
        //else if (this.waitTime >= this.patience)
        //{
        //    this.state = MonsterState.Searching;
        //    this.waitTime = 0f;
        //}
        else
        {
            this.waitTime += 1f / this.speed;
        }
    }

    //private void SearchBehavior()
    //{
    //    if (this.lastSeen == null)
    //    { return; }

    //    // Move towards the last known player position
    //    RoomDoorDirection dir = this.GetDirectionToTarget(this.lastSeen);
    //    this.targetRoom = this.GetRoomInDirection(dir);

    //    // Move the monster to the target room
    //    this.MoveToRoom(this.targetRoom);

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

    private void ChaseBehavior()
    {
        // Move towards the player's current position
        RoomDoorDirection dir = this.GetDirectionToTarget(this.PRoom);
        this.targetRoom = this.GetRoomInDirection(dir);

        // Move the monster to the target room
        this.MoveToRoom(this.targetRoom);

        // Check if the player is still in detection range
        if (!this.DetectPlayer())
        {
            //this.state = MonsterState.Searching;
            this.state = MonsterState.Patrolling;
            this.lastSeen = this.PRoom;
        }
        else if (this.currentRoom == this.PRoom)
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