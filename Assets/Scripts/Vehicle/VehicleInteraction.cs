using UnityEngine;
using Zenject;

public class VehicleInteraction : MonoBehaviour
{
    [SerializeField] private Transform model;
    private VehicleCore _vehicleCore;
    private bool _attached;
    private CharacterCore _characterCore;
    private float _lastActionTime = -1f;

    [Inject]
    private void Construct(CoreController vehicleCore)
    {
        _vehicleCore = (VehicleCore) vehicleCore;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CharacterCore core))
        {
            _characterCore = core;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_attached) return;
        if (other.TryGetComponent(out CharacterCore core) && core == _characterCore)
        {
            _characterCore = null;
        }
    }
    
    private void Update()
    {
        if (_characterCore && !_attached && _characterCore.InputHandler.InteractPressed && Time.time > _lastActionTime + 1f)
        {
            Attach();
            _lastActionTime = Time.time;
            return;
        }

        if (_characterCore && _attached && _characterCore.InputHandler.InteractPressed && Time.time > _lastActionTime + 1f)
        {
            Detach();
            _lastActionTime = Time.time;
        }
    }

    private void Attach()
    {
        _attached = true;
        
        _characterCore.GetComponent<CharacterController>().enabled = false;
    
        _characterCore.transform.parent = model.transform;
        _characterCore.transform.localPosition = Vector3.zero;
        _characterCore.transform.localRotation = Quaternion.identity;
        
        _vehicleCore.SetDriverInput(_characterCore.InputHandler);
    }

    private void Detach()
    {
        _characterCore.transform.parent = null;
        _characterCore.GetComponent<CharacterController>().enabled = true;
        
        _attached = false;
        _characterCore = null;
        
        _vehicleCore.ResetDriver();
    }
}