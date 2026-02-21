using UnityEngine;

public interface ICoreController
{
    public AdvancedCharacterController  Controller                  { get; set; }
    public PlayablesAnimatorController  PlayablesAnimatorController { get; set; }
    public InputHandler                 InputHandler { get; set; }
    
    public void UpdateLocomotion(bool isInitialization = false){}
}

public abstract class CoreController : MonoBehaviour, ICoreController
{
    public AdvancedCharacterController  Controller                  { get; set; }
    public PlayablesAnimatorController  PlayablesAnimatorController { get; set; }
    public InputHandler                 InputHandler { get; set; }

    public abstract void UpdateLocomotion(bool isInitialization = false);
}
