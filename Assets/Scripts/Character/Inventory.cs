using System.Collections.Generic;
using System.Linq;

public class Inventory
{
    private readonly List<ItemInstance> _instances = new();
    private readonly EquipmentManager _equipmentManager;
    private readonly CharacterSlots _characterSlots;

    private ItemInstance _primaryWeapon;

    public Inventory(EquipmentManager equipmentManager, CharacterSlots characterSlots)
    {
        _equipmentManager = equipmentManager;
        _characterSlots = characterSlots;
    }

    public void AddInstance(SimpleItem data)
    {
        var instance = new ItemInstance(data);
        _instances.Add(instance);
    }

    public void SetPrimaryWeapon(ItemInstance itemInstance)
    {
        if (itemInstance == null || !(itemInstance.GetData() is EquippedItem))
        {
            return;
        }

        if (!_instances.Contains(itemInstance))
        {
            return;
        }

        _primaryWeapon = itemInstance;
        _equipmentManager.Equip(_primaryWeapon, _characterSlots);
    }

    public void DrawWeapon()
    {
        if (_primaryWeapon == null)
            return;

        _equipmentManager.SetItemActive(_primaryWeapon, _characterSlots);
    }

    public void UnDrawWeapon()
    {
        if (_primaryWeapon == null)
            return;

        _equipmentManager.SetItemEquipped(_primaryWeapon, _characterSlots);
    }

    public ItemInstance GetPrimaryWeapon() => _primaryWeapon;

    public ItemInstance GetItemInstance(SimpleItem data)
    {
        return _instances.FirstOrDefault(i => i.GetData() == data);
    }
}