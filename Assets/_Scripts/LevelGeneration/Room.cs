using UnityEngine;
using System.Collections.Generic;

// Типы комнат
public enum RoomType
{
    None = 0,
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
    public bool isCleared = false; // Очищена ли комната от врагов
    [Header("Основные параметры")]
    public RoomType roomType;
    public Vector2Int gridPosition;     // Позиция в сетке (X, Y)

    [Header("Соединения")]
    public List<Room> connectedRooms;   // Соседние комнаты
    public Direction availableExits;    // Какие выходы есть у комнаты (битовая маска)

    [Header("Состояние")]
    public bool isVisited;              // Посещал ли игрок эту комнату

    public bool isDoorToNextLevelActive = false;

    public bool itemTaken = false;      // был ли предмет уже подобран в этой комнате
    public int goldItemIndex = -1;      // индекс выбранного предмета в Gold комнате (-1 означает не выбран)

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
    // Метод проверяет, открыто ли указанное направление в битовой маске выходов комнаты
    public bool HasConnection(Direction direction)
    {
        // availableExits — это ваша переменная маски из генератора (проверьте её имя в файле)
        return (availableExits & direction) == direction;
    }

}