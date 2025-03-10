// Room class to store room data
using System;
using System.Collections.Generic;

using UnityEngine;

public enum RoomType : byte { Normal, Home, Exit, Shard }

public enum RoomTileType : byte
{
    TopLeftWall = 0, TopWall = 1, TopRightWall = 2, LeftWall = 5, RightWall = 7, BottomLeftWall = 10, BottomWall = 11, BottomRightWall = 12,
    II = 3, III = 4, IIII = 8, IIIII = 9, Cross = 6,
    DoorOpenH = 16, DoorOpenV = 17, DoorCloseH = 21, DoorCloseV = 22,
    Dot = 13, Empty = 14, ExitOff = 15, ExitOn = 20,
    Skull = 18, Home = 19, S = 23, X = 24
}

public class Room
{
    public static event Action GameWon;
    public static event Action ShardCollected;

    public static RoomTileType[] roomTileTypes = new RoomTileType[]
    {
        RoomTileType.BottomLeftWall, RoomTileType.BottomWall, RoomTileType.BottomRightWall,
        RoomTileType.LeftWall, RoomTileType.Empty, RoomTileType.RightWall,
        RoomTileType.TopLeftWall, RoomTileType.TopWall, RoomTileType.TopRightWall,
    };

    public Vector2Int position;
    public List<RoomDoorDirection> doors = new List<RoomDoorDirection>(4);
    public RoomType type;

    private SpriteRenderer center => this.tiles[4];

    private GameObject obj;
    private SpriteRenderer[] tiles = new SpriteRenderer[9];

    public Room(Vector2Int position, RoomType type = RoomType.Normal)
    {
        this.position = position;
        this.type = type;
    }

    public void InitObj(int gridSize = 3)
    {
        int x = this.position.x;
        int y = this.position.y;

        this.obj = new GameObject($"Room_{x}_{y}");
        this.obj.transform.position = new Vector3(x, y, 0);
        int centerIndex = Mathf.FloorToInt(Room.roomTileTypes.Length * 0.5f);

        for (int i = 0; i < Room.roomTileTypes.Length; i++)
        {
            RoomTileType tileType = Room.roomTileTypes[i];
            if (i == centerIndex)
            {
                tileType = GetCenterTileType(this.type);
            }
            else
            {
                tileType = CheckDoorTiles(i, tileType);
            }

            int tileX = i % gridSize;
            int tileY = i / gridSize;

            GameObject rendObj = new GameObject(tileType.ToString());
            rendObj.transform.SetParent(this.obj.transform);
            rendObj.transform.localPosition = new Vector3(0.5f + (tileX - 1), 0.5f + (tileY - 1), 0);

            SpriteRenderer rend = rendObj.AddComponent<SpriteRenderer>();
            rend.sprite = GameManager.Instance.Tiles[(int)tileType];

            this.tiles[i] = rend;
        }

        RoomTileType CheckDoorTiles(int i, RoomTileType tileType)
        {
            if ((i == 7 && this.doors.Contains(RoomDoorDirection.Up)) || (i == 1 && this.doors.Contains(RoomDoorDirection.Down)))
            {
                tileType = RoomTileType.DoorOpenH;
            }
            else if ((i == 5 && this.doors.Contains(RoomDoorDirection.Right)) || (i == 3 && this.doors.Contains(RoomDoorDirection.Left)))
            {
                tileType = RoomTileType.DoorOpenV;
            }

            return tileType;
        }

        RoomTileType GetCenterTileType(RoomType type)
        {
            if (type == RoomType.Home)
            {
                return RoomTileType.Home;
            }
            else if (type == RoomType.Exit)
            {
                return RoomTileType.ExitOff;
            }
            else if (type == RoomType.Shard)
            {
                return RoomTileType.X;
            }

            return RoomTileType.Empty;
        }
    }

    public void Enter()
    {
        if (this.type == RoomType.Shard)
        {
            this.type = RoomType.Normal;
            this.center.sprite = GameManager.Instance.Tiles[(int)RoomTileType.Empty];

            Room.ShardCollected?.Invoke();
        }
        else if (this.type == RoomType.Exit && MapGenerator.Instance.IsExitOpen)
        {
            Room.GameWon?.Invoke();
        }
    }

    public void OpenExit()
    {
        if (this.type == RoomType.Exit)
        {
            this.center.sprite = GameManager.Instance.Tiles[(int)RoomTileType.ExitOn];
        }
    }
}