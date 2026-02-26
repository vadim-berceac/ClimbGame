using UnityEngine;

public interface IInputSource
{
    public Vector2 OnMove { get; set; }
    public Vector2 OnLook { get; set; }
    public Vector3 Rotation { get; set; }
    public bool OnJump { get; set; }
    public bool OnRun { get; set; }
    public bool OnCrouch { get; set; }
    public bool OnInteract { get; set; }
}

public abstract class MonoInputSource : MonoBehaviour, IInputSource
{
    public Vector2 OnMove { get; set; }
    public Vector2 OnLook { get; set; }
    public Vector3 Rotation { get; set; }
    public bool OnJump { get; set; }
    public bool OnRun { get; set; }
    public bool OnCrouch { get; set; }
    
    public bool OnInteract { get; set; }
}
