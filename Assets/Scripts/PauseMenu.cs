using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    #region Variables
    [SerializeField] public GameObject pauseMenuUI;
    public bool isPaused;

    [SerializeField] public GameObject mainMenuUI;
    [SerializeField] public GameObject settingsMenuUI;
    #endregion

    #region Functions
    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        isPaused = false;
    }

    public void Settings()
    {
        mainMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
