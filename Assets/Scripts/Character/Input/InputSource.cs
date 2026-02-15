using UnityEngine;

public interface IInputSource
{
    public Vector2 OnMove { get; set; }
    public Vector2 OnLook { get; set; }
    public bool OnJump { get; set; }
}

public abstract class InputSource : MonoBehaviour, IInputSource
{
    public Vector2 OnMove { get; set; }
    public Vector2 OnLook { get; set; }
    public bool OnJump { get; set; }
}
