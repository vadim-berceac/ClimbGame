using UnityEngine;
using Zenject;

public class VehicleInteraction : MonoBehaviour
{
    [SerializeField] private Transform model;
    private VehicleCore _vehicleCore;
    private bool _attached;
    private CharacterCore _characterCore;

    [Inject]
    private void Construct(CoreController vehicleCore)
    {
        _vehicleCore = (VehicleCore) vehicleCore;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        other.TryGetComponent(out _characterCore);
        Debug.Log($"TriggerEnter: _characterCore = {_characterCore}"); // проверь что нашёл
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out _characterCore))
        {
            _characterCore = null;
        }
    }
    
    private void Update()
    {
        if (_characterCore)
        {
            Debug.Log($"attached: {_attached}, InteractPressed: {_characterCore.InputHandler.InteractPressed}");
        }

        if (_characterCore && !_attached && _characterCore.InputHandler.InteractPressed)
        {
            Attach();
            return;
        }

        if (_characterCore && _attached && _characterCore.InputHandler.InteractPressed)
        {
            Detach();
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
