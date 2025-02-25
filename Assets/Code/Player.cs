using System;

using UnityEngine;
public class Player : CameraUnit<Player>
{
    public static event Action PlayerDied;

    public override RoomTileType type => RoomTileType.S;

    protected override Room ChooseTargetRoom()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            if (this.currentRoom.doors.Contains(RoomDoorDirection.Up))
            {
                return MapGenerator.Instance.roomDict[this.currentRoom.position + (Vector2Int.up * MapGenerator.Instance.RoomSize.y)];
            }
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            if (this.currentRoom.doors.Contains(RoomDoorDirection.Down))
            {
                return MapGenerator.Instance.roomDict[this.currentRoom.position + (Vector2Int.down * MapGenerator.Instance.RoomSize.y)];
            }
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            if (this.currentRoom.doors.Contains(RoomDoorDirection.Right))
            {
                return MapGenerator.Instance.roomDict[this.currentRoom.position + (Vector2Int.right * MapGenerator.Instance.RoomSize.x)];
            }
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            if (this.currentRoom.doors.Contains(RoomDoorDirection.Left))
            {
                return MapGenerator.Instance.roomDict[this.currentRoom.position + (Vector2Int.left * MapGenerator.Instance.RoomSize.x)];
            }
        }

        return null;
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
        if (!UnityEditor.EditorApplication.isPlaying || MapGenerator.Instance == null)
        {
            return;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(this.transform.position, MapGenerator.Instance.RoomSize.x * 0.5f);
    }
#endif
}