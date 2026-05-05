using UnityEngine;
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{
    [field: SerializeField] public Button QuitButton { get; set; }
    [field: SerializeField] public QuitMode QuitMode { get; set; }
    [field: SerializeField] public bool BreakConnection { get; set; }
    [field: SerializeField] public string MenuScene { get; set; }

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
        if (BreakConnection)
        {
            ConnectionService.Disconnect();
        }
        if (QuitMode == QuitMode.ExitGame)
        {
            Application.Quit();
            return;
        }
        ConnectionService.LoadNetworkScene(MenuScene);
    }
}

public enum QuitMode
{
    Menu = 0,
    ExitGame = 1
}
