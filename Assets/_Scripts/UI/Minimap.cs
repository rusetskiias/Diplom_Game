using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Minimap : MonoBehaviour
{
    [Header("Настройки")]
    public bool showFullMapFromStart = false; // если true — вся карта сразу, если false — открывается постепенно
    public GameObject roomIconPrefab;
    public RectTransform mapContainer;
    public float cellSize = 40f;

    public GameObject currentRoomMarkerPrefab;
    private GameObject currentMarker;

    private Dictionary<Room, RectTransform> roomIcons = new Dictionary<Room, RectTransform>();
    private Dictionary<Room, List<GameObject>> roomLines = new Dictionary<Room, List<GameObject>>();
    private HashSet<Room> revealedRooms = new HashSet<Room>();
    private List<Room> lastRooms;
    private Room lastCurrentRoom;
    private Vector2Int minBounds, maxBounds;
    private List<Room> currentRooms;

    public void BuildMap(List<Room> rooms, Room currentRoom)
    {
        currentRooms = rooms;
        lastRooms = rooms;
        lastCurrentRoom = currentRoom;
        ClearMap();

        if (rooms == null || rooms.Count == 0) return;
        if (mapContainer == null) return;

        // Если режим постепенного открытия — добавляем стартовую комнату в открытые
        if (!showFullMapFromStart && revealedRooms.Count == 0)
        {
            Room startRoom = rooms.Find(r => r.roomType == RoomType.Start);
            if (startRoom != null)
            {
                revealedRooms.Add(startRoom);
            }
        }

        // Если полная карта — открываем все комнаты
        if (showFullMapFromStart)
        {
            revealedRooms.Clear();
            foreach (Room room in rooms)
            {
                revealedRooms.Add(room);
            }
        }

        // Находим границы
        minBounds = new Vector2Int(int.MaxValue, int.MaxValue);
        maxBounds = new Vector2Int(int.MinValue, int.MinValue);
        // ... остальное как было

        foreach (Room room in rooms)
        {
            minBounds.x = Mathf.Min(minBounds.x, room.gridPosition.x);
            maxBounds.x = Mathf.Max(maxBounds.x, room.gridPosition.x);
            minBounds.y = Mathf.Min(minBounds.y, room.gridPosition.y);
            maxBounds.y = Mathf.Max(maxBounds.y, room.gridPosition.y);
        }

        // 1. СНАЧАЛА создаем все иконки комнат и регистрируем их в словаре
        foreach (Room room in rooms)
        {
            CreateRoomIcon(room, room == currentRoom);
        }

        // 2. ТЕПЕРЬ создаем маркер игрока (когда словарь гарантированно заполнен)
        if (currentRoom != null && roomIcons.ContainsKey(currentRoom))
        {
            currentMarker = Instantiate(currentRoomMarkerPrefab, roomIcons[currentRoom]);

            RectTransform markerRect = currentMarker.GetComponent<RectTransform>();
            if (markerRect != null)
            {
                markerRect.anchoredPosition = Vector2.zero;
                // Сбрасываем локальный масштаб в 1, чтобы маркер не сжался в ноль
                markerRect.localScale = Vector3.one;
                // Принудительно выталкиваем точку вперед по оси Z, чтобы она была перед иконкой
                markerRect.localPosition = new Vector3(0, 0, -1f);
            }
        }
    }

    private void ClearMap()
    {
        // Удаляем точку игрока при очистке карты
        if (currentMarker != null) Destroy(currentMarker);

        foreach (var kvp in roomIcons)
        {
            if (kvp.Value != null) Destroy(kvp.Value.gameObject);
        }
        roomIcons.Clear();

        foreach (var kvp in roomLines)
        {
            foreach (GameObject line in kvp.Value)
            {
                if (line != null) Destroy(line);
            }
        }
        roomLines.Clear();
    }

    private void CreateRoomIcon(Room room, bool isCurrent)
    {
        if (roomIconPrefab == null) return;

        // Если не полная карта и комната не открыта — не создаём иконку
        if (!showFullMapFromStart && !revealedRooms.Contains(room))
            return;

        Vector2 canvasPos = GridToContainerPosition(room.gridPosition);

        GameObject iconObj = Instantiate(roomIconPrefab, mapContainer);
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchoredPosition = canvasPos;

        roomIcons[room] = iconRect;

        Image iconImage = iconObj.GetComponent<Image>();
        if (iconImage != null)
        {
            if (isCurrent)
            {
                iconImage.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                if (room.roomType == RoomType.Start || room.roomType == RoomType.Combat)
                    iconImage.color = new Color(0.45f, 0.45f, 0.45f, 1f); // сбалансированный серый

                else if (room.roomType == RoomType.Shop)
                    iconImage.color = new Color(1f, 0.8f, 0f, 1f); // Золотой 

                else if (room.roomType == RoomType.Gold)
                    iconImage.color = new Color(0.4f, 1f, 0.4f, 1f); // Светло-зеленый / Лаймовый

                else if (room.roomType == RoomType.Boss)
                    iconImage.color = new Color(1f, 0.2f, 0.2f, 1f); // ярко-красный
                else
                    iconImage.color = new Color(0.45f, 0.45f, 0.45f, 1f);
            }


            if (room.isCleared && !isCurrent)
            {
                Color c = iconImage.color;
                c.a = 0.5f;
                iconImage.color = c;
            }
        }
    }

    public void RevealRoom(Room room)
    {
        if (room == null) return;
        if (revealedRooms.Contains(room)) return;

        revealedRooms.Add(room);

        // Если карта уже была построена — перестраиваем её заново
        if (lastRooms != null)
        {
            BuildMap(lastRooms, lastCurrentRoom);
        }
    }

    private Vector2 GridToContainerPosition(Vector2Int gridPos)
    {
        float offsetX = (gridPos.x - (minBounds.x + maxBounds.x) / 2f) * cellSize;
        float offsetY = (gridPos.y - (minBounds.y + maxBounds.y) / 2f) * cellSize;

        return new Vector2(offsetX, offsetY);
    }
}
