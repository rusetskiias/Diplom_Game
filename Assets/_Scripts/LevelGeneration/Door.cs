using UnityEngine;

public class Door : MonoBehaviour
{
    public Direction direction;      // Направление этой двери (например, Right)
    public Room targetRoom;          // Заполняется вашим генератором при создании графа

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (targetRoom != null)
            {
                // Передаем комнату и направление, ИЗ КОТОРОГО пришел игрок
                RoomLoader.Instance.TransitionToRoom(targetRoom, direction);
            }
            else
            {
                Debug.LogWarning("У этой двери не назначена целевая комната!");
            }
        }
    }
}