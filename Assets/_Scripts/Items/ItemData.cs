using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    public float value;      // величина эффекта
}

public enum ItemType
{
    DamageUp,
    FireRateUp,
    SpeedUp,
    HealthUp,
    HealthRestore
}