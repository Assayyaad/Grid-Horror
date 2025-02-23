// Room class to store room data
using System.Collections.Generic;

using UnityEngine;

public class Room
{
    public Vector2Int Position;
    public List<RoomDoorDirection> Doors = new List<RoomDoorDirection>();

    public Room(Vector2Int position)
    {
        Position = position;
    }
}