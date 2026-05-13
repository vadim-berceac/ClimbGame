using Unity.Netcode;
using UnityEngine;

public interface ICoreController
{
    public AdvancedCharacterController  Controller                  { get; set; }
    public PlayablesAnimatorController  PlayablesAnimatorController { get; set; }
    public InputHandler                 InputHandler { get; set; }
    
    public void SwitchLocomotion(LocomotionConfigs newLocomotionConfig, bool setBusy = false){}
}

public abstract class CoreController : NetworkBehaviour , ICoreController
{
    public AdvancedCharacterController  Controller                  { get; set; }
    public PlayablesAnimatorController  PlayablesAnimatorController { get; set; }
    public InputHandler                 InputHandler { get; set; }

    public abstract void SwitchLocomotion(LocomotionConfigs newLocomotionConfig, bool setBusy = false);
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public virtual void RequestOwnershipServerRpc(ulong requestingClientId, InputSourceMode mode){}
}
