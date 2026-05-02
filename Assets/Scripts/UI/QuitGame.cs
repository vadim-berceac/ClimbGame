using UnityEngine;
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{
    [field: SerializeField] public Button QuitButton { get; set; }

    private void Start()
    {
        QuitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnDisable()
    {
        QuitButton.onClick.RemoveAllListeners();
    }

    private void OnQuitClicked()
    {
        Application.Quit();
    }
}
