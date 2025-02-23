using UnityEngine;

public class Player : Singleton<Player>
{
    public float PlayerSpeed = 5f; // Player movement speed
    public float PlayerPatience = 5f; // Time in seconds to wait before switching to Searching
    private Room currentRoom;
    private Room targetRoom;

    private new Camera camera;

    protected override void Awake()
    {
        base.Awake();
        this.camera = Camera.main;
    }

    private void Start()
    {
        this.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.Tiles[(int)RoomTileType.S];
    }

    private void Update()
    {
        // Move the player towards the target room
        if (this.targetRoom != null)
        {
            //this.currentRoom.Exit();

            this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(this.targetRoom.position.x + 0.5f, this.targetRoom.position.y + 0.5f, 0), Time.deltaTime * this.PlayerSpeed);
            // Check if the player has reached the target room
            if (Vector3.Distance(this.transform.position, new Vector3(this.targetRoom.position.x + 0.5f, this.targetRoom.position.y + 0.5f, 0)) < 0.1f)
            {
                this.currentRoom = this.targetRoom;
                this.targetRoom = null;

                this.currentRoom.Enter();
            }

            return;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            if (this.currentRoom.doors.Contains(RoomDoorDirection.Up))
            {
                this.targetRoom = MapGenerator.Instance.roomDict[this.currentRoom.position + (Vector2Int.up * MapGenerator.Instance.RoomSize.y)];
            }
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            if (this.currentRoom.doors.Contains(RoomDoorDirection.Down))
            {
                this.targetRoom = MapGenerator.Instance.roomDict[this.currentRoom.position + (Vector2Int.down * MapGenerator.Instance.RoomSize.y)];
            }
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            if (this.currentRoom.doors.Contains(RoomDoorDirection.Right))
            {
                this.targetRoom = MapGenerator.Instance.roomDict[this.currentRoom.position + (Vector2Int.right * MapGenerator.Instance.RoomSize.x)];
            }
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            if (this.currentRoom.doors.Contains(RoomDoorDirection.Left))
            {
                this.targetRoom = MapGenerator.Instance.roomDict[this.currentRoom.position + (Vector2Int.left * MapGenerator.Instance.RoomSize.x)];
            }
        }
    }

    private void LateUpdate()
    {
        Vector3 temp = this.transform.position;
        temp.z = -10;
        this.camera.transform.position = temp;
    }

    public void WakeUp(Room room)
    {
        // Set the player's starting position to the starting room
        this.currentRoom = room;
        this.transform.position = new Vector3(room.position.x + 0.5f, room.position.y + 0.5f, 0);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(this.transform.position, MapGenerator.Instance.RoomSize.x * 0.5f);
    }
#endif
}