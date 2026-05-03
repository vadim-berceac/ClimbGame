using UnityEngine;

public class ConnectionInfo : MonoBehaviour
{
    [field: SerializeField] public TMPro.TMP_Text Text { get; private set; }

    private void Awake()
    {
        ConnectionService.ConnectionChanged += UpdateText;
        UpdateText(ConnectionService.CurrentConnection);
    }

    private void UpdateText(ConnectionData data)
    {
        Text.text = data.ToString();
    }

    private void OnDestroy()
    {
        ConnectionService.ConnectionChanged -= UpdateText;
    }
}
