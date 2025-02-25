using System.Collections.Generic;

using UnityEngine;

public static class AStar
{
    public static List<Room> FindPath(Room start, Room target)
    {
        PriorityQueue<Room> openSet = new PriorityQueue<Room>();
        Dictionary<Room, Room> cameFrom = new Dictionary<Room, Room>();
        Dictionary<Room, float> gScore = new Dictionary<Room, float>();
        Dictionary<Room, float> fScore = new Dictionary<Room, float>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, target);

        while (openSet.Count > 0)
        {
            Room current = openSet.Dequeue();

            if (current == target)
            { return ReconstructPath(cameFrom, current); }

            foreach (Room neighbor in GetNeighbors(current))
            {
                float tentativeGScore = gScore[current] + 1; // Assuming each step has a cost of 1

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Heuristic(neighbor, target);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    }
                }
            }
        }

        return null; // No path found
    }

    private static List<Room> ReconstructPath(Dictionary<Room, Room> cameFrom, Room current)
    {
        List<Room> path = new List<Room> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }

    private static float Heuristic(Room a, Room b)
    {
        // Manhattan distance
        return Mathf.Abs(a.position.x - b.position.x) + Mathf.Abs(a.position.y - b.position.y);
    }

    private static IEnumerable<Room> GetNeighbors(Room room)
    {
        List<Room> neighbors = new List<Room>();

        foreach (RoomDoorDirection door in room.doors)
        {
            Room neighbor = MapGenerator.Instance.roomDict.TryGetValue(GetNeighborPosition(room.position, door), out Room n) ? n : null;
            if (neighbor != null)
            { neighbors.Add(neighbor); }
        }

        return neighbors;
    }

    private static Vector2Int GetNeighborPosition(Vector2Int position, RoomDoorDirection door)
    {
        Vector2Int size = MapGenerator.Instance.RoomSize;

        return door switch
        {
            RoomDoorDirection.Up => position + (Vector2Int.up * size.y),
            RoomDoorDirection.Down => position + (Vector2Int.down * size.y),
            RoomDoorDirection.Right => position + (Vector2Int.right * size.x),
            RoomDoorDirection.Left => position + (Vector2Int.left * size.x),
            _ => position,
        };
    }
}
