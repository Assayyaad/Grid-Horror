using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Random = UnityEngine.Random;

public enum RoomDoorDirection : byte { Up = 0, Right = 1, Down = 2, Left = 3 }

public class MapGenerator : MonoBehaviour
{
    // Data
    public Vector2Int WorldSize = new Vector2Int(20, 20); // Boundaries of the world
    public Vector2Int RoomSize = new Vector2Int(1, 1); // Size of each room
    [Min(1)]
    public int MaxSameDirTimes = 2; // Maximum times the same direction can be chosen consecutively
    public int[] MainPaths = new int[] { 7, 5, 5 }; // Length of each main path

    private Dictionary<Vector2Int, Room> roomDict = new Dictionary<Vector2Int, Room>();
    private List<Room> allRooms = new List<Room>();
    private List<Room> finalRooms = new List<Room>();

    private RoomDoorDirection RandomDirection => (RoomDoorDirection)Random.Range(0, 4);
    private bool IsSafe => this.MainPaths.Sum() < this.WorldSize.x * this.WorldSize.y;

    // Main generation function
    private void Start()
    {
        this.allRooms = new List<Room>();
        this.roomDict = new Dictionary<Vector2Int, Room>(this.MainPaths.Sum());
        this.finalRooms = new List<Room>(this.MainPaths.Length);
        this.GenerateMap();
    }

    [ContextMenu("Generate Map")]
    private void RegenerateMap()
    {
        this.allRooms = new List<Room>();
        this.roomDict = new Dictionary<Vector2Int, Room>(this.MainPaths.Sum());
        this.finalRooms = new List<Room>(this.MainPaths.Length);
        this.GenerateMap();
    }

    private void GenerateMap()
    {
        if (!this.IsSafe)
        {
            Debug.LogError("Room count is more than what the world can hold");
            return;
        }

        if (this.MainPaths.Length < 1 || this.MainPaths.Length > 4)
        {
            Debug.LogError("Main paths count should be between 1 and 4");
            return;
        }

        Vector2Int startPos = new Vector2Int(this.WorldSize.x / 2, this.WorldSize.y / 2);
        Room startRoom = new Room(startPos);
        this.allRooms.Add(startRoom);
        this.roomDict.Add(startPos, startRoom);

        // Generate main paths
        for (int i = 0; i < this.MainPaths.Length; i++)
        {
            Room[] path = this.CreatePath(this.allRooms[0], this.MainPaths[i]);

            if (path.Length > 0)
            {
                // Store the final room of the path
                this.finalRooms.Add(path[path.Length - 1]);
            }
        }
    }

    private Room[] CreatePath(Room startRoom, int len)
    {
        List<Room> path = new List<Room>(len);
        Room currRoom = startRoom;
        RoomDoorDirection? prevDir = null;
        int sameDirTimes = 0;

        for (int i = 0; i < len; i++)
        {
            Vector2Int nextPos = Vector2Int.zero * -1;
            RoomDoorDirection dir = 0;
            RoomDoorDirection[] directions = this.RandomDirections(prevDir.HasValue ? prevDir.Value : null);

            // Keep trying to find a valid direction
            int k = 0;
            for (; k < directions.Length; k++)
            {
                dir = directions[k];
                nextPos = this.GetNextPosition(currRoom.Position, dir);

                // Check if the next position is within world boundaries
                bool isOutside = nextPos.x < 0 || nextPos.x >= WorldSize.x || nextPos.y < 0 || nextPos.y >= WorldSize.y;
                if (isOutside)
                { continue; }

                // Check if the next position is occupied by another room
                bool isOccupied = this.roomDict.ContainsKey(nextPos);
                if (isOccupied)
                { continue; }

                // Check if the same direction has been chosen too many times
                bool isSameDir = prevDir.HasValue && dir == prevDir.Value;
                if (isSameDir)
                {
                    sameDirTimes++;
                    if (sameDirTimes >= this.MaxSameDirTimes)
                    { continue; }
                }
                else
                { sameDirTimes = 0; }

                break;
            }

            if (k >= directions.Length)
            { continue; }

            // Create the next room
            Room nextRoom = new Room(nextPos);

            // Add doors to both rooms
            currRoom.Doors.Add(dir);
            nextRoom.Doors.Add(this.GetOppositeDirection(dir));

            // Update current room and previous direction
            currRoom = nextRoom;
            prevDir = dir;

            // Add the new room to the list of all rooms
            path.Add(currRoom);
            this.allRooms.Add(currRoom);
            this.roomDict.Add(nextPos, currRoom);
        }

        return path.ToArray();
    }

    private Vector2Int GetNextPosition(Vector2Int currentPos, RoomDoorDirection dir)
    {
        return dir switch
        {
            RoomDoorDirection.Up => currentPos + Vector2Int.up,
            RoomDoorDirection.Right => currentPos + Vector2Int.right,
            RoomDoorDirection.Down => currentPos + Vector2Int.down,
            RoomDoorDirection.Left => currentPos + Vector2Int.left,
            _ => currentPos,
        };
    }
    private RoomDoorDirection GetOppositeDirection(RoomDoorDirection dir)
    {
        return (RoomDoorDirection)(((int)dir + 2) % 4);
    }
    private RoomDoorDirection[] RandomDirections(RoomDoorDirection? backwards = null)
    {
        Stack<RoomDoorDirection> directions = new Stack<RoomDoorDirection>(4);
        while (directions.Count < 4)
        {
            directions.Push(this.RandomDirection);
        }

        if (backwards.HasValue)
        {
            List<RoomDoorDirection> list = directions.ToList();
            list.Remove(backwards.Value);
            directions = new Stack<RoomDoorDirection>(list);
            //directions = new Stack<RoomDoorDirection>(directions.Where(d => d != backwards));
        }

        return directions.ToArray();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (allRooms == null || allRooms.Count == 0)
        { return; }

        Gizmos.color = Color.blue;
        Vector3 worldCenter = new Vector3(this.WorldSize.x * 0.5f, this.WorldSize.y * 0.5f, 0);
        Gizmos.DrawWireCube(worldCenter, new Vector3(this.WorldSize.x, this.WorldSize.y, 0));

        // Draw all rooms and their doors
        foreach (Room room in allRooms)
        {
            // Draw the room as a square
            Vector3 roomCenter = new Vector3(room.Position.x + 0.5f, room.Position.y + 0.5f, 0);
            Gizmos.color = Color.white; // Room color
            Gizmos.DrawWireCube(roomCenter, new Vector3(this.RoomSize.x, this.RoomSize.y, 0));

            Gizmos.color = Color.magenta; // Room color
            Gizmos.DrawWireSphere(roomCenter, this.RoomSize.x * 0.15f);

            // Draw doors
            foreach (RoomDoorDirection door in room.Doors)
            {
                Vector3 doorPosition = roomCenter;
                switch (door)
                {
                case RoomDoorDirection.Up:
                    doorPosition += new Vector3(0, 0.5f, 0);
                    break;
                case RoomDoorDirection.Right:
                    doorPosition += new Vector3(0.5f, 0, 0);
                    break;
                case RoomDoorDirection.Down:
                    doorPosition += new Vector3(0, -0.5f, 0);
                    break;
                case RoomDoorDirection.Left:
                    doorPosition += new Vector3(-0.5f, 0, 0);
                    break;
                }

                // Draw a line to represent the door
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(doorPosition, 0.1f);
            }
        }

        Vector3 startRoomCenter = new Vector3(this.allRooms[0].Position.x + 0.5f, this.allRooms[0].Position.y + 0.5f, 0);
        Gizmos.color = Color.green; // Room color
        Gizmos.DrawSphere(startRoomCenter, this.RoomSize.x * 0.25f);

        foreach (Room room in finalRooms)
        {
            Vector3 roomCenter = new Vector3(room.Position.x + 0.5f, room.Position.y + 0.5f, 0);
            Gizmos.color = Color.red; // Room color
            Gizmos.DrawSphere(roomCenter, this.RoomSize.x * 0.25f);
        }
    }
#endif
}