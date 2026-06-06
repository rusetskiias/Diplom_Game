using UnityEngine;

public class Pickup : MonoBehaviour
{
    public ItemData itemData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                ApplyEffect(playerStats);
            }
            // Сообщаем комнате, что предмет подобрали
            Room room = GetComponentInParent<Room>();
            if (room != null)
            {
                room.itemTaken = true;
            }
            Destroy(gameObject);
        }
    }
    private void ApplyEffect(PlayerStats playerStats)
    {
        switch (itemData.itemType)
        {
            case ItemType.DamageUp:
                playerStats.ModifyDamage(itemData.value);
                break;
            case ItemType.FireRateUp:
                playerStats.ModifyFireRate(-itemData.value); // уменьшаем задержку
                break;
            case ItemType.SpeedUp:
                playerStats.ModifySpeed(itemData.value);
                break;
            case ItemType.HealthUp:
                playerStats.ModifyMaxHealth(itemData.value);
                break;
            case ItemType.HealthRestore:
                // Если value < 1, считаем как процент от максимального здоровья
                if (itemData.value < 1f)
                {
                    float percent = itemData.value;
                    float healAmount = playerStats.maxHealth * percent;
                    playerStats.Heal(healAmount);
                }
                else
                {
                    playerStats.Heal(itemData.value);
                }
                break;
        }
    }
}