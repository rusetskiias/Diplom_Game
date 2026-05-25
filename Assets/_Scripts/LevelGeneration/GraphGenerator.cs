using UnityEngine;
using System.Collections.Generic;

public class GraphGenerator : MonoBehaviour
{
    [Header("Настройки генерации")]
    public int minRooms = 8;
    public int maxRooms = 12;

    private List<Room> rooms = new List<Room>();
    private System.Random random;

    private void DebugSpecialRooms()
    {
        Debug.Log("========== ОТЛАДКА ОСОБЫХ КОМНАТ (МАСКА ВЫХОДОВ) ==========");

        foreach (Room room in rooms)
        {
            if (room.roomType == RoomType.Boss || room.roomType == RoomType.Shop || room.roomType == RoomType.Gold)
            {
                // Подсчитываем, сколько битов установлено в маске availableExits
                int exitCount = 0;
                if ((room.availableExits & Direction.Left) != 0) exitCount++;
                if ((room.availableExits & Direction.Right) != 0) exitCount++;
                if ((room.availableExits & Direction.Up) != 0) exitCount++;
                if ((room.availableExits & Direction.Down) != 0) exitCount++;

                string exits = "";
                if ((room.availableExits & Direction.Left) != 0) exits += "Left ";
                if ((room.availableExits & Direction.Right) != 0) exits += "Right ";
                if ((room.availableExits & Direction.Up) != 0) exits += "Up ";
                if ((room.availableExits & Direction.Down) != 0) exits += "Down ";

                Debug.Log($"🔍 КОМНАТА: {room.roomType} | Позиция: {room.gridPosition}");
                Debug.Log($"   Соседей в графе (connectedRooms): {room.connectedRooms.Count}");
                Debug.Log($"   Выходов в маске (availableExits): {exitCount} | Направления: {exits}");

                if (exitCount > 1)
                {
                    Debug.LogError($"❌ ОШИБКА: У {room.roomType} в МАСКЕ {exitCount} выходов! Должен быть 1.");
                }
                else if (exitCount == 0)
                {
                    Debug.LogError($"❌ ОШИБКА: У {room.roomType} в МАСКЕ 0 выходов!");
                }
                else
                {
                    Debug.Log($"✅ {room.roomType}: в маске 1 выход");
                }
            }
        }

        Debug.Log("============================================================");
    }
    public List<Room> GenerateLevel(int seed)
    {
        random = new System.Random(seed);
        rooms.Clear();

        int roomCount = random.Next(minRooms, maxRooms + 1);
        CreateRoomsProcedural(roomCount);

        return rooms;
    }

    private void CreateRoomsProcedural(int count)
    {
        // Очищаем список комнат перед новой генерацией
        rooms.Clear();
        Vector2Int currentPos = Vector2Int.zero;

        // 1. Создаем Стартовую комнату
        GameObject startObj = new GameObject("Room_Start");
        Room startRoom = startObj.AddComponent<Room>();
        startRoom.gridPosition = currentPos;
        startRoom.roomType = RoomType.Start;
        rooms.Add(startRoom);


        // Вычисляем длину основного пути (минус Магазин и Золото)
        int mainPathCount = count - 2;
        if (mainPathCount < 3) mainPathCount = 3;

        // 2. Строим основной путь (Старт -> Боевые -> Босс)
        for (int i = 1; i < mainPathCount; i++)
        {
            List<Direction> validDirections = GetFreeDirections(currentPos);
            if (validDirections.Count == 0)
            {
                Debug.LogWarning("[Generator] Зашли в тупик при построении основного пути!");
                break;
            }

            Direction chosenDirection = validDirections[random.Next(validDirections.Count)];
            Vector2Int nextPos = GetNextPosition(currentPos, chosenDirection);

            GameObject roomObj = new GameObject($"Room_Main_{i}");
            Room newRoom = roomObj.AddComponent<Room>();
            newRoom.gridPosition = nextPos;
            newRoom.roomType = RoomType.Combat;

            // Если это строго ПОСЛЕДНЯЯ комната основного пути — это БОСС
            if (i == mainPathCount - 1)
            {
                newRoom.roomType = RoomType.Boss;
            }

            rooms.Add(newRoom);


            // Связываем текущую комнату с предыдущей
            Room prevRoom = rooms[i - 1];

            // Добавляем биты в маску availableExits обеих комнат
            prevRoom.availableExits |= chosenDirection;
            newRoom.availableExits |= GetOppositeDirection(chosenDirection);

            // Физически связываем их в графе (для логики перемещения DoorLinker)
            prevRoom.AddConnection(newRoom, chosenDirection);
            newRoom.AddConnection(prevRoom, GetOppositeDirection(chosenDirection));

            currentPos = nextPos;
        }

        // 3. Создаем тупиковые ответвления
        CreateSpecialDeadEndRoom(RoomType.Shop, "Room_Shop");
        CreateSpecialDeadEndRoom(RoomType.Gold, "Room_Gold");


        // 4. ЖЕСТКАЯ ПРОВЕРКА ТУПИКОВ (Гарантия одного входа для особых комнат)
        FixSpecialRoomsExits();

        // 4.5 Соединяем все соседние комнаты, у которых нет связи
        //ConnectAdjacentRooms();


        // 5. ОТЛАДКА: выводим информацию об особых комнатах
        DebugSpecialRooms();
    }

    // Метод принудительно оставляет особым комнатам только один вход, который ведет назад
    // Метод принудительно оставляет особым комнатам только один вход
    private void FixSpecialRoomsExits()
    {
        foreach (Room room in rooms)
        {
            if (room.roomType == RoomType.Boss || room.roomType == RoomType.Shop || room.roomType == RoomType.Gold)
            {
                // Если у комнаты больше одного соединения — обрезаем
                if (room.connectedRooms.Count > 1)
                {
                    Debug.LogWarning($"Особая комната {room.roomType} имеет {room.connectedRooms.Count} входа! Оставляем только первый.");

                    // Сохраняем только первую связь
                    Room onlyNeighbor = room.connectedRooms[0];
                    Direction directionToNeighbor = Direction.None;

                    // Находим направление к сохранённому соседу
                    if (onlyNeighbor.gridPosition.x < room.gridPosition.x) directionToNeighbor = Direction.Left;
                    else if (onlyNeighbor.gridPosition.x > room.gridPosition.x) directionToNeighbor = Direction.Right;
                    else if (onlyNeighbor.gridPosition.y < room.gridPosition.y) directionToNeighbor = Direction.Down;
                    else if (onlyNeighbor.gridPosition.y > room.gridPosition.y) directionToNeighbor = Direction.Up;

                    // Очищаем все связи
                    room.connectedRooms.Clear();
                    room.availableExits = Direction.None;

                    // Восстанавливаем только одну связь
                    room.connectedRooms.Add(onlyNeighbor);
                    room.availableExits = directionToNeighbor;

                    // Также нужно убрать ссылку на эту комнату у других комнат (кроме сохранённого соседа)
                    foreach (Room otherRoom in rooms)
                    {
                        if (otherRoom != room && otherRoom != onlyNeighbor)
                        {
                            if (otherRoom.connectedRooms.Contains(room))
                            {
                                otherRoom.connectedRooms.Remove(room);
                                // Обновляем маску выхода у otherRoom
                                Direction dirToRoom = Direction.None;
                                if (room.gridPosition.x < otherRoom.gridPosition.x) dirToRoom = Direction.Left;
                                else if (room.gridPosition.x > otherRoom.gridPosition.x) dirToRoom = Direction.Right;
                                else if (room.gridPosition.y < otherRoom.gridPosition.y) dirToRoom = Direction.Down;
                                else if (room.gridPosition.y > otherRoom.gridPosition.y) dirToRoom = Direction.Up;
                                otherRoom.availableExits &= ~dirToRoom;
                            }
                        }
                    }

                    Debug.Log($"Исправлено: у {room.roomType} остался только один вход {directionToNeighbor}");
                    DebugSpecialRooms();
                }
            }
        }
    }

    // Вспомогательный метод для создания истинного тупика
    private void CreateSpecialDeadEndRoom(RoomType type, string objName)
    {
        // Находим все комнаты, к которым можно пристроить тупик (ВКЛЮЧАЯ СТАРТОВУЮ)
        List<Room> candidates = new List<Room>();
        foreach (Room room in rooms)
        {
            // Исключаем только босса (к боссу не пристраиваем тупики)
            if (room.roomType == RoomType.Boss) continue;

            List<Direction> freeDirs = GetFreeDirections(room.gridPosition);
            foreach (Direction dir in freeDirs)
            {
                Vector2Int potentialPos = GetNextPosition(room.gridPosition, dir);

                // Проверяем, что все три другие стороны будущей комнаты пусты
                bool isValid = true;
                foreach (Direction checkDir in new Direction[] { Direction.Left, Direction.Right, Direction.Up, Direction.Down })
                {
                    if (checkDir == GetOppositeDirection(dir)) continue; // пропускаем сторону входа
                    Vector2Int checkPos = GetNextPosition(potentialPos, checkDir);
                    if (GetRoomAtPosition(checkPos) != null)
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                {
                    candidates.Add(room);
                    break; // достаточно одной валидной позиции для этой комнаты
                }
            }
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"Не найдено подходящего места для {type}");
            return;
        }

        // Выбираем случайную комнату
        Room anchorRoom = candidates[random.Next(candidates.Count)];
        List<Direction> validDirs = GetFreeDirections(anchorRoom.gridPosition);

        // Фильтруем направления, которые дают чистые три стороны
        Direction chosenDir = Direction.None;
        foreach (Direction dir in validDirs)
        {
            Vector2Int potentialPos = GetNextPosition(anchorRoom.gridPosition, dir);
            bool clean = true;
            foreach (Direction checkDir in new Direction[] { Direction.Left, Direction.Right, Direction.Up, Direction.Down })
            {
                if (checkDir == GetOppositeDirection(dir)) continue;
                Vector2Int checkPos = GetNextPosition(potentialPos, checkDir);
                if (GetRoomAtPosition(checkPos) != null)
                {
                    clean = false;
                    break;
                }
            }
            if (clean)
            {
                chosenDir = dir;
                break;
            }
        }

        if (chosenDir == Direction.None)
        {
            Debug.LogWarning($"Не удалось найти чистое направление для {type}");
            return;
        }

        Vector2Int specialPos = GetNextPosition(anchorRoom.gridPosition, chosenDir);

        // Создаём комнату
        GameObject specialObj = new GameObject(objName);
        Room specialRoom = specialObj.AddComponent<Room>();
        specialRoom.gridPosition = specialPos;
        specialRoom.roomType = type;
        rooms.Add(specialRoom);

        // Связываем
        anchorRoom.availableExits |= chosenDir;
        specialRoom.availableExits |= GetOppositeDirection(chosenDir);
        anchorRoom.AddConnection(specialRoom, chosenDir);
        specialRoom.AddConnection(anchorRoom, GetOppositeDirection(chosenDir));

        Debug.Log($"Создан {type} на позиции {specialPos} от комнаты {anchorRoom.gridPosition} (тип {anchorRoom.roomType})");
    }



    // Вспомогательный метод: проверяет, какие направления вокруг свободны
    private List<Direction> GetFreeDirections(Vector2Int pos)
    {
        List<Direction> free = new List<Direction>();

        Vector2Int leftPos = pos + Vector2Int.left;
        if (GetRoomAtPosition(leftPos) == null)
            free.Add(Direction.Left);

        Vector2Int rightPos = pos + Vector2Int.right;
        if (GetRoomAtPosition(rightPos) == null)
            free.Add(Direction.Right);

        Vector2Int upPos = pos + Vector2Int.up;
        if (GetRoomAtPosition(upPos) == null)
            free.Add(Direction.Up);

        Vector2Int downPos = pos + Vector2Int.down;
        if (GetRoomAtPosition(downPos) == null)
            free.Add(Direction.Down);

        return free;
    }

    // Вспомогательный метод: вычисляет новые координаты по направлению
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

    // Вспомогательный метод: возвращает противоположное направление
    private Direction GetOppositeDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Left: return Direction.Right;
            case Direction.Right: return Direction.Left;
            case Direction.Up: return Direction.Down;
            case Direction.Down: return Direction.Up;
            default: return Direction.None;
        }
    }

    // Возвращает соседнюю комнату в заданном направлении
    public Room GetNeighborInDirection(Room currentRoom, Direction direction)
    {
        Debug.Log($"GetNeighbor: комната {currentRoom.gridPosition}, ищем соседа {direction}");
        Vector2Int targetPos = currentRoom.gridPosition;

        switch (direction)
        {
            case Direction.Left: targetPos += Vector2Int.left; break;
            case Direction.Right: targetPos += Vector2Int.right; break;
            case Direction.Up: targetPos += Vector2Int.up; break;
            case Direction.Down: targetPos += Vector2Int.down; break;
            default: return null;
        }
        Debug.Log($"GetNeighbor: из {currentRoom.gridPosition} направление {direction} -> ищем {targetPos}");
        Room neighbor = rooms.Find(r => r.gridPosition == targetPos);
        Debug.Log($"Найден сосед: {(neighbor != null ? neighbor.gridPosition.ToString() : "null")}");
        return neighbor;
    }

    public Room GetRoomAtPosition(Vector2Int pos)
    {
        return rooms.Find(r => r.gridPosition == pos);
    }


    public List<Room> GetAllRooms()
    {
        return rooms;
    }
}