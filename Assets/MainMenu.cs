using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnPlayClick()
    {
        SceneManager.LoadScene("TestScene");
    }

    public void OnQuitClick()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
