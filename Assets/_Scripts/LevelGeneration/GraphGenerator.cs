using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GraphGenerator : MonoBehaviour
{
    [Header("Настройки генерации")]
    public int minRooms = 8;
    public int maxRooms = 12;
    public int gridWidth = 13;
    public int gridHeight = 13;

    private System.Random random;
    private int currentSeed;
    private List<Room> lastRooms;
    private Room[,] lastGrid;

    public List<Room> GenerateLevel(int seed)
    {
        currentSeed = seed;
        random = new System.Random(seed);
        int attempt = 0;

        while (true)
        {
            attempt++;
            List<Room> rooms = GenerateLevelInternal(attempt);
            if (rooms == null) continue;

            bool hasBoss = rooms.Any(r => r.roomType == RoomType.Boss);
            bool hasShop = rooms.Any(r => r.roomType == RoomType.Shop);
            bool hasGold = rooms.Any(r => r.roomType == RoomType.Gold);

            if (hasBoss && hasShop && hasGold)
            {
                lastRooms = rooms;
                return rooms;
            }
            else
            {
               currentSeed = random.Next();
                random = new System.Random(currentSeed);
            }
        }
    }

    private List<Room> GenerateLevelInternal(int attemptSeed)
    {
        List<Room> rooms = new List<Room>();
        Room[,] grid = new Room[gridWidth, gridHeight];
        List<Vector2Int> frontier = new List<Vector2Int>();

        Vector2Int startPos = new Vector2Int(gridWidth / 2, gridHeight / 2);
        Room start = CreateRoomAt(startPos, RoomType.Start, rooms, grid);
        frontier.Add(startPos);

        int targetRooms = random.Next(minRooms, maxRooms + 1);
        int currentRooms = 1;

        while (currentRooms < targetRooms && frontier.Count > 0)
        {
            if (ExpandFromFrontier(frontier, grid, rooms, ref currentRooms))
                continue;
        }

        List<Room> deadEnds = rooms.Where(r => r.connectedRooms.Count == 1 && r.roomType != RoomType.Start).ToList();
        int needed = 3 - deadEnds.Count;

        for (int i = 0; i < needed; i++)
        {
            List<Room> candidates = rooms.Where(r => GetFreeDirections(r.gridPosition, grid).Count > 0).ToList();
            if (candidates.Count == 0) break;

            Room anchor = candidates[random.Next(candidates.Count)];
            List<Direction> freeDirs = GetFreeDirections(anchor.gridPosition, grid);
            if (freeDirs.Count == 0) continue;

            Direction dir = freeDirs[random.Next(freeDirs.Count)];
            Vector2Int newPos = GetNextPosition(anchor.gridPosition, dir);

            if (CountNeighbors(newPos, grid) >= 2) continue;

            Room newRoom = CreateRoomAt(newPos, RoomType.Combat, rooms, grid);
            ConnectRooms(anchor, newRoom);
            currentRooms++;
        }

        AssignSpecialRooms(rooms, grid);
        lastGrid = grid;
        return rooms;
    }

    private bool ExpandFromFrontier(List<Vector2Int> frontier, Room[,] grid, List<Room> rooms, ref int currentRooms)
    {
        if (frontier.Count == 0) return false;

        int idx = random.Next(frontier.Count);
        Vector2Int parentPos = frontier[idx];
        Room parent = GetRoomAt(parentPos, grid);

        List<Direction> freeDirs = GetFreeDirections(parentPos, grid);
        if (freeDirs.Count == 0)
        {
            frontier.RemoveAt(idx);
            return false;
        }

        Direction dir = freeDirs[random.Next(freeDirs.Count)];
        Vector2Int newPos = GetNextPosition(parentPos, dir);

        if (CountNeighbors(newPos, grid) >= 2) return false;

        foreach (Direction checkDir in System.Enum.GetValues(typeof(Direction)))
        {
            if (checkDir == Direction.None) continue;
            Vector2Int neighborPos = GetNextPosition(newPos, checkDir);
            Room neighbor = GetRoomAt(neighborPos, grid);
            if (neighbor != null && neighbor.connectedRooms.Count >= 2)
                return false;
        }

        Room newRoom = CreateRoomAt(newPos, RoomType.Combat, rooms, grid);
        ConnectRooms(parent, newRoom);
        currentRooms++;

        if (newRoom.connectedRooms.Count == 1)
            frontier.Add(newPos);

        if (GetFreeDirections(parentPos, grid).Count == 0)
            frontier.Remove(parentPos);

        return true;
    }

    private void AssignSpecialRooms(List<Room> rooms, Room[,] grid)
    {
        List<Room> deadEnds = rooms.Where(r => r.connectedRooms.Count == 1 && r.roomType != RoomType.Start).ToList();
        Room start = rooms.Find(r => r.roomType == RoomType.Start);
        if (start == null) return;

        Dictionary<Room, int> distance = new Dictionary<Room, int>();
        Queue<Room> queue = new Queue<Room>();
        distance[start] = 0;
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            Room cur = queue.Dequeue();
            foreach (Room neighbor in cur.connectedRooms)
            {
                if (!distance.ContainsKey(neighbor))
                {
                    distance[neighbor] = distance[cur] + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }

        Room boss = deadEnds.OrderByDescending(r => distance.ContainsKey(r) ? distance[r] : -1).First();
        boss.roomType = RoomType.Boss;

        List<Room> otherDeadEnds = deadEnds.Where(r => r != boss).ToList();
        otherDeadEnds = otherDeadEnds.OrderBy(x => random.Next()).ToList();

        if (otherDeadEnds.Count >= 1) otherDeadEnds[0].roomType = RoomType.Shop;
        if (otherDeadEnds.Count >= 2) otherDeadEnds[1].roomType = RoomType.Gold;

        foreach (Room room in rooms)
        {
            if (room.roomType == RoomType.Shop || room.roomType == RoomType.Gold || room.roomType == RoomType.Boss)
            {
                if (room.connectedRooms.Count != 1)
                {
                    Room keep = room.connectedRooms[0];
                    for (int i = 1; i < room.connectedRooms.Count; i++)
                    {
                        Room other = room.connectedRooms[i];
                        if (other != null && other.connectedRooms.Contains(room))
                            other.connectedRooms.Remove(room);
                    }
                    room.connectedRooms.Clear();
                    room.connectedRooms.Add(keep);
                }
            }
        }

        AssignDoors(rooms, grid);
    }

    // ==================== Вспомогательные методы ====================
    private Room CreateRoomAt(Vector2Int pos, RoomType type, List<Room> rooms, Room[,] grid)
    {
        GameObject roomObj = new GameObject($"Room_{type}_{pos.x}_{pos.y}");
        Room room = roomObj.AddComponent<Room>();
        room.gridPosition = pos;
        room.roomType = type;
        room.connectedRooms = new List<Room>();
        rooms.Add(room);
        grid[pos.x, pos.y] = room;
        return room;
    }

    private void ConnectRooms(Room a, Room b)
    {
        if (!a.connectedRooms.Contains(b)) a.connectedRooms.Add(b);
        if (!b.connectedRooms.Contains(a)) b.connectedRooms.Add(a);
    }

    private List<Direction> GetFreeDirections(Vector2Int pos, Room[,] grid)
    {
        List<Direction> free = new List<Direction>();
        Vector2Int[] deltas = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };
        Direction[] dirs = { Direction.Left, Direction.Right, Direction.Up, Direction.Down };
        for (int i = 0; i < deltas.Length; i++)
        {
            Vector2Int newPos = pos + deltas[i];
            if (newPos.x >= 0 && newPos.x < gridWidth && newPos.y >= 0 && newPos.y < gridHeight && GetRoomAt(newPos, grid) == null)
                free.Add(dirs[i]);
        }
        return free;
    }

    private int CountNeighbors(Vector2Int pos, Room[,] grid)
    {
        int count = 0;
        if (GetRoomAt(pos + Vector2Int.left, grid) != null) count++;
        if (GetRoomAt(pos + Vector2Int.right, grid) != null) count++;
        if (GetRoomAt(pos + Vector2Int.up, grid) != null) count++;
        if (GetRoomAt(pos + Vector2Int.down, grid) != null) count++;
        return count;
    }

    private Vector2Int GetNextPosition(Vector2Int current, Direction dir)
    {
        switch (dir)
        {
            case Direction.Left: return current + Vector2Int.left;
            case Direction.Right: return current + Vector2Int.right;
            case Direction.Up: return current + Vector2Int.up;
            case Direction.Down: return current + Vector2Int.down;
            default: return current;
        }
    }

    private void AssignDoors(List<Room> rooms, Room[,] grid)
    {
        foreach (Room room in rooms)
        {
            room.availableExits = Direction.None;
            foreach (Room neighbor in room.connectedRooms)
            {
                Vector2Int delta = neighbor.gridPosition - room.gridPosition;
                if (delta == Vector2Int.left) room.availableExits |= Direction.Left;
                else if (delta == Vector2Int.right) room.availableExits |= Direction.Right;
                else if (delta == Vector2Int.up) room.availableExits |= Direction.Up;
                else if (delta == Vector2Int.down) room.availableExits |= Direction.Down;
            }
        }
    }

    private Room GetRoomAt(Vector2Int pos, Room[,] grid)
    {
        if (pos.x < 0 || pos.x >= gridWidth || pos.y < 0 || pos.y >= gridHeight) return null;
        return grid[pos.x, pos.y];
    }

    // ==================== Публичные методы для совместимости ====================
    public Room GetNeighborInDirection(Room currentRoom, Direction direction)
    {
        if (lastGrid == null) return null;
        Vector2Int target = currentRoom.gridPosition;
        switch (direction)
        {
            case Direction.Left: target += Vector2Int.left; break;
            case Direction.Right: target += Vector2Int.right; break;
            case Direction.Up: target += Vector2Int.up; break;
            case Direction.Down: target += Vector2Int.down; break;
            default: return null;
        }
        return GetRoomAt(target, lastGrid);
    }

    public Room GetRoomAtPosition(Vector2Int pos) => GetRoomAt(pos, lastGrid);
    public List<Room> GetAllRooms() => lastRooms;
}