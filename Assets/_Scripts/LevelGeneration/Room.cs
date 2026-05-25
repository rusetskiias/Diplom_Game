using UnityEngine;
using System.Collections.Generic;

// Типы комнат
public enum RoomType
{
    Start,      // Стартовая комната (вход)
    Combat,     // Обычная боевая комната
    Shop,       // Магазин
    Gold,        //Комната с особым предметом
    Boss        // Комната босса (выход)
}

public enum Direction
{
    None = 0,
    Left = 1,
    Right = 2,
    Up = 4,
    Down = 8
}

public class Room : MonoBehaviour
{
    [Header("Основные параметры")]
    public RoomType roomType;
    public Vector2Int gridPosition;     // Позиция в сетке (X, Y)

    [Header("Соединения")]
    public List<Room> connectedRooms;   // Соседние комнаты
    public Direction availableExits;    // Какие выходы есть у комнаты (битовая маска)

    [Header("Состояние")]
    public bool isVisited;              // Посещал ли игрок эту комнату

    // Для комнаты босса
    public int entranceCount = 0;       // Количество входов (не больше 1)

    private void Awake()
    {
        connectedRooms = new List<Room>();
        isVisited = false;
        availableExits = Direction.None;
        entranceCount = 0;
    }

    // Добавить соединение с другой комнатой
    public void AddConnection(Room room, Direction direction)
    {
        if (!connectedRooms.Contains(room))
        {
            connectedRooms.Add(room);
            availableExits |= direction;
        }
    }

    // Убрать соединение с комнатой
    public void RemoveConnection(Room room, Direction direction)
    {
        if (connectedRooms.Contains(room))
        {
            connectedRooms.Remove(room);
            availableExits &= ~direction;
        }
    }

    // Проверить, есть ли выход в определённом направлении
    public bool HasExit(Direction direction)
    {
        return (availableExits & direction) != 0;
    }

    // Получить список непосещённых соседних комнат
    public List<Room> GetUnvisitedNeighbors()
    {
        List<Room> unvisited = new List<Room>();
        foreach (Room room in connectedRooms)
        {
            if (!room.isVisited)
            {
                unvisited.Add(room);
            }
        }
        return unvisited;
    }

    // Проверка: можно ли добавить вход в комнату босса
    public bool CanAddEntrance()
    {
        if (roomType != RoomType.Boss) return true;
        return entranceCount < 1;  // У босса не больше 1 входа
    }

    // Добавить вход (для босса)
    public void AddEntrance()
    {
        if (roomType == RoomType.Boss)
        {
            entranceCount++;
        }
    }
    // Метод проверяет, открыто ли указанное направление в битовой маске выходов комнаты
    public bool HasConnection(Direction direction)
    {
        // availableExits — это ваша переменная маски из генератора (проверьте её имя в файле)
        return (availableExits & direction) == direction;
    }

}