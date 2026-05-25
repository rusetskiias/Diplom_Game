using UnityEngine;

public class RoomVisualizer : MonoBehaviour
{
    [System.Serializable]
    public struct DoorSetup
    {
        public Direction direction;
        public GameObject doorObject; // Сам объект двери (например, Door_Left)
    }

    [Header("Список дверей на сцене")]
    public DoorSetup[] doors;

    // Метод включает дверь, если есть проход, и выключает, если там тупик
    public void ApplyLayout(Room logicalRoom)
    {
        foreach (var setup in doors)
        {
            if (setup.doorObject == null) continue;

            // Проверяем по графу, есть ли сосед в этом направлении
            bool hasConnection = logicalRoom.HasConnection(setup.direction);

            // Включаем или выключаем объект двери
            setup.doorObject.SetActive(hasConnection);

            if (hasConnection)
            {
                Debug.Log($"[RoomVisualizer] Дверь {setup.direction} ВКЛЮЧЕНА (проход открыт).");
            }
            else
            {
                Debug.Log($"[RoomVisualizer] Дверь {setup.direction} ВЫКЛЮЧЕНА (там теперь просто стена).");
            }
        }
    }
}
