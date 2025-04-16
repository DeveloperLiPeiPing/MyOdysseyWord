using UnityEngine;
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{
    [SerializeField] private Button QuitGameButton;
    
    void Start()
    {
        QuitGameButton.onClick.AddListener(CloseGame);
    }
    
    
    private void CloseGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

}