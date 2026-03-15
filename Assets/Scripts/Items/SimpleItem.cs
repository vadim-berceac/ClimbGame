using UnityEngine;

[CreateAssetMenu(fileName = "SimpleItem", menuName = "Scriptable Objects/SimpleItem")]
public class SimpleItem : ScriptableObject
{
    [field: SerializeField] public string ItemName { get; private set; }
    [field: SerializeField] public int Price { get; private set; }
    [field: SerializeField] public Sprite Icon { get; set; }
    [field: SerializeField] public GameObject GroundPrefab { get; set; }
}
