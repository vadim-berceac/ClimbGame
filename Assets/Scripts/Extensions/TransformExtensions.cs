using UnityEngine;

public static class TransformExtensions
{
    public static Transform FindChildRecursive(this Transform transform, string name)
    {
        foreach (Transform child in transform)
        {
            if (child.name.Contains(name))
            {
                return child;
            }

            var result = FindChildRecursive(child, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
    
    private static void AttachSource(this Transform transform, Transform source,
        Vector3 position, Vector3 rotation, float scale, bool enabled)
    {
        source.SetParent(transform, false);
        source.SetLocalPositionAndRotation(position, Quaternion.Euler(rotation));

        var desiredLossy = Vector3.one * scale;
        var parentLossy = transform.lossyScale;

        source.localScale = new Vector3(
            desiredLossy.x / parentLossy.x,
            desiredLossy.y / parentLossy.y,
            desiredLossy.z / parentLossy.z
        );

        source.gameObject.SetActive(enabled);
    }
    
    public static void AttachSource(this Transform transform, Transform source,
        SlotSettings slotSettings)
    {
        var scale = slotSettings.TransformData.Scale;
        var position = slotSettings.TransformData.Position;
        var rotation = slotSettings.TransformData.Rotation.eulerAngles;
        var enable = slotSettings.TransformData.Active;
        
        transform.AttachSource(source, position, rotation, scale, enable);
    }
}
