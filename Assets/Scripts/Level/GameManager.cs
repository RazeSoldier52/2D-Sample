using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void OnEnable()
    {
        WorldBoundary.OnPlayerFellOutOfBounds += ReloadScene;
    }
    public void OnDisable()
    {
        WorldBoundary.OnPlayerFellOutOfBounds-= ReloadScene;
    }
    public void ReloadScene()
    {
        Scene currentScene=SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
