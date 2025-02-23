// Room class to store room data
using System.Collections.Generic;

using UnityEngine;

public enum RoomType : byte { Start, End, Normal }

public enum RoomTileType : byte
{
    TopLeftWall = 0, TopWall = 1, TopRightWall = 2, LeftWall = 5, RightWall = 7, BottomLeftWall = 10, BottomWall = 11, BottomRightWall = 12,
    II = 3, III = 4, IIII = 8, IIIII = 9, Cross = 6,
    DoorOpenH = 16, DoorOpenV = 17, DoorCloseH = 21, DoorCloseV = 22,
    Dot = 13, Empty = 14, Other1 = 15, Other2 = 20,
    Skull = 18, Home = 19, S = 23, X = 24
}

public class Room
{
    public static RoomTileType[] roomTileTypes = new RoomTileType[]
    {
        RoomTileType.BottomLeftWall, RoomTileType.BottomWall, RoomTileType.BottomRightWall,
        RoomTileType.LeftWall, RoomTileType.Empty, RoomTileType.RightWall,
        RoomTileType.TopLeftWall, RoomTileType.TopWall, RoomTileType.TopRightWall,
    };
    public static RoomTileType[] roomDoorTileTypes = new RoomTileType[]
    {
        RoomTileType.BottomLeftWall, RoomTileType.DoorOpenH, RoomTileType.BottomRightWall,
        RoomTileType.DoorOpenV, RoomTileType.Empty, RoomTileType.DoorOpenV,
        RoomTileType.TopLeftWall, RoomTileType.DoorOpenH, RoomTileType.TopRightWall,
    };

    public Vector2Int position;
    public List<RoomDoorDirection> doors = new List<RoomDoorDirection>(4);
    public RoomType type;

    private GameObject obj;

    public Room(Vector2Int position, RoomType type = RoomType.Normal)
    {
        this.position = position;
        this.type = type;
    }

    public void AddDoor(RoomDoorDirection direction)
    {
        this.doors.Add(direction);
    }

    public void InitObj()
    {
        int x = this.position.x;
        int y = this.position.y;

        this.obj = new GameObject($"Room_{x}_{y}");
        this.obj.transform.position = new Vector3(x, y, 0);

        int gridSize = 3; // Assuming a 3x3 grid for the room tiles
        int tileOffset = 1; // Assuming each tile is 1 unit in size

        for (int i = 0; i < roomTileTypes.Length; i++)
        {
            RoomTileType tileType = Room.roomTileTypes[i];

            if (this.doors.Contains(RoomDoorDirection.Up) && i == 7)
            {
                tileType = roomDoorTileTypes[i];
            }
            else if (this.doors.Contains(RoomDoorDirection.Down) && i == 1)
            {
                tileType = roomDoorTileTypes[i];
            }
            else if (this.doors.Contains(RoomDoorDirection.Right) && i == 5)
            {
                tileType = roomDoorTileTypes[i];
            }
            else if (this.doors.Contains(RoomDoorDirection.Left) && i == 3)
            {
                tileType = roomDoorTileTypes[i];
            }

            if (tileType == RoomTileType.Empty)
            {
                if (this.type == RoomType.Start && i == 4)
                {
                    tileType = RoomTileType.Home;
                }
                else if (this.type == RoomType.End && i == 4)
                {
                    tileType = RoomTileType.Other2;
                }
            }

            int tileX = i % gridSize;
            int tileY = i / gridSize;

            GameObject rendObj = new GameObject(tileType.ToString());
            rendObj.transform.SetParent(this.obj.transform);
            rendObj.transform.localPosition = new Vector3(0.5f + ((tileX - 1) * tileOffset), 0.5f + ((tileY - 1) * tileOffset), 0);

            SpriteRenderer rend = rendObj.AddComponent<SpriteRenderer>();
            rend.sprite = Loader.Instance.tiles[(int)tileType];

        }
    }
}