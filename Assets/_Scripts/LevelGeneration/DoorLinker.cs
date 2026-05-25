using System.Collections;
using UnityEngine;

public class DoorLinker : MonoBehaviour
{
    private Room currentRoom;
    private GraphGenerator graphGenerator;

    public void Initialize(Room sceneRoom, GraphGenerator graphGenerator, Room modelRoom)
    {
        this.currentRoom = modelRoom;
        this.graphGenerator = graphGenerator; // Сохраняем ссылку на генератор

        // Переносим данные о маске выходов из модели генератора в физический компонент на сцене
        if (sceneRoom != null && modelRoom != null)
        {
            sceneRoom.roomType = modelRoom.roomType;
            sceneRoom.availableExits = modelRoom.availableExits;
            sceneRoom.gridPosition = modelRoom.gridPosition;
        }

        // Передаем генератор внутрь метода связи дверей
        LinkDoors(graphGenerator);

        // Запускаем визуализацию через корутину, чтобы Unity успела обновить объекты
        StartCoroutine(ApplyVisualsDelayed(sceneRoom));
    }

    private IEnumerator ApplyVisualsDelayed(Room sceneRoom)
    {
        // Ждем один кадр, чтобы все компоненты на сцене гарантированно проснулись
        yield return null;

        RoomVisualizer visualizer = GetComponent<RoomVisualizer>();
        if (visualizer != null)
        {
            visualizer.ApplyLayout(sceneRoom);
        }
        else
        {
            Debug.LogWarning($"[DoorLinker] На объекте {gameObject.name} не найден компонент RoomVisualizer!");
        }
    }

    // ИСПРАВЛЕНО: метод теперь правильно принимает генератор в качестве параметра
    private void LinkDoors(GraphGenerator generator)
    {
        if (currentRoom == null)
        {
            Debug.LogError("[DoorLinker] currentRoom равен NULL в методе LinkDoors!");
            return;
        }

        if (generator == null)
        {
            Debug.LogError("[DoorLinker] Переданный GraphGenerator равен NULL в методе LinkDoors!");
            return;
        }

        Door[] doors = GetComponentsInChildren<Door>();
        Debug.Log($"[DoorLinker] Комната {currentRoom.roomType}, позиция {currentRoom.gridPosition}, найдено дверей на сцене: {doors.Length}");

        foreach (Door door in doors)
        {
            // Используем переданный генератор для поиска соседа
            Room neighbor = generator.GetNeighborInDirection(currentRoom, door.direction);
            door.targetRoom = neighbor;

            if (neighbor != null)
                Debug.Log($"  -> Дверь {door.direction} ведёт в комнату {neighbor.gridPosition} ({neighbor.roomType})");
            else
                Debug.Log($"  -> Дверь {door.direction} НЕ имеет соседа (тупик стены).");
        }
    }
}
