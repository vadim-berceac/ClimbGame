using System;
using UnityEngine;

/// <summary>
/// Только для инициализации слотов
/// </summary>
[Serializable]
public struct ItemSlotData
{
    [field: SerializeField] public ItemSlotType SlotType { get; set; }
    [field: SerializeField] public TransformData TransformData { get; set; }
    [field: SerializeField] public HumanBodyBones ParentBone { get; set; }
}

/// <summary>
/// Для использования инициализированных слотов
/// </summary>
[Serializable]
public struct SlotSettings
{
    [field: SerializeField] public ItemSlotType SlotType { get; set; }
    [field: SerializeField] public TransformData TransformData { get; set; }
}

[Serializable]
public struct TransformData
{
    [field: SerializeField] public Vector3 Position { get; private set; }
    [field: SerializeField] public Quaternion Rotation { get; private set; }
    [field: SerializeField] public float Scale { get; private set; }
    [field: SerializeField] public bool Active { get; private set; }
}

public enum ItemSlotType
{
    RightHandSlot = 0,
    LeftHandSlot = 1,
    UpperHeadSlot = 2,
    LowerHeadSlot = 3,
    TorsoSlot = 4,
    SpineSlot = 5,
    HipsSlot = 6,
}
