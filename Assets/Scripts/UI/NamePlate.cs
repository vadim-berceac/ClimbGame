using TMPro;
using UnityEngine;

public class Nameplate : MonoBehaviour
{
    [SerializeField] private TextMeshPro nameText;
    [SerializeField] private bool showHpBar;
    [SerializeField] private Transform healthBarRoot;        
    [SerializeField] private Transform healthFill;
    [SerializeField] private float maxHealthBarWidth = 2f;   
    [SerializeField] private Camera targetCamera;

    private Transform _myTransform;
    private Transform _mainCamera;
    private Vector3 _offset = Vector3.zero;

    private void Awake()
    {
        _myTransform = transform;
        _mainCamera = Camera.main.transform;

        if (!showHpBar)
        {
            healthBarRoot.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        _myTransform.position = transform.parent.position + _offset;

        if (_mainCamera == null)
        {
           return;
        }
        
        var lookDir = _mainCamera.position - _myTransform.position;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
            _myTransform.rotation = Quaternion.LookRotation(-lookDir);
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if(!showHpBar) return;
        
        var healthPercent = Mathf.Clamp01(currentHealth / maxHealth);
        
        if (healthFill == null)
        {
           return;
        }
        
        var scale = healthFill.localScale;
        scale.x = healthPercent * maxHealthBarWidth; 
        healthFill.localScale = scale;
            
        if (healthFill.TryGetComponent<SpriteRenderer>(out var sr))
        {
            sr.color = Color.Lerp(Color.red, Color.green, healthPercent);
        }
    }

    public void SetOffset(Vector3 offset)
    {
        _offset = offset;
    }

    public void SetName(string newName)
    {
        if (nameText != null)
            nameText.text = newName;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}