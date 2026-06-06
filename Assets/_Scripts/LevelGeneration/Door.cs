using UnityEngine;

public class Door : MonoBehaviour
{
    public Direction direction;      // Направление этой двери (например, Right)
    public Room targetRoom;          // Заполняется генератором при создании графа

    private bool isLocked = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (isLocked)
            {
                return;
            }

            if (targetRoom != null)
            {
                RoomLoader.Instance.TransitionToRoom(targetRoom, direction);
            }
        }
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        // Визуально можно сменить спрайт или цвет
    }
}