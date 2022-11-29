using UnityEngine; //Connect to UnityEngine
using UnityEngine.SceneManagement; //Use scene management to change scenes

public class MenuHandler : MonoBehaviour
{
    #region Manager Functions
    public void LimitFrames(int optionID)
    {
        //Set frame limit to value corresponding with optionID
        if (optionID == 0) Application.targetFrameRate = 300; //0 is unlimited
        else if (optionID == 1) Application.targetFrameRate = 60; //1 is 60fps
        else if (optionID == 2) Application.targetFrameRate = 30; //2 is 30fps
        else Application.targetFrameRate = 20; //3 is 20fps
    }
    public void ChangeScene(int sceneIndex)
    {
        //Load the scene that corresponds to the value passed from menu
        SceneManager.LoadScene(sceneIndex);
    }
    public void QuitGame()
    {
        //If unity editor, stop play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        //Quit the game
        Application.Quit();
    }
    #endregion
}
