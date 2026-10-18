using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


    [SerializeField] private GameObject[] Levels;
    private GameObject currentLevel;
    public GameObject StartMenu,LevelMenu,inGameMenu,PauseMenu,LevelCompletePan,ResetPan;
    private int CurrentLevelIndex = 0;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public void LoadLevel(int levelIndex)
    {
       currentLevel=Instantiate(Levels[levelIndex]);
        HideMenu();
        CurrentLevelIndex = levelIndex;
    }

    public void UnloadCurrentLevel()
    {
        if (currentLevel != null)
        {
            Destroy(currentLevel);
        }
    }

    public void ShowLevelMenu()
    {
        LevelMenu.SetActive(true);
        StartMenu.SetActive(false);
        inGameMenu.SetActive(false);
        PauseMenu.SetActive(false);
        LevelCompletePan.SetActive(false);
    }

    public void HideMenu()
    {
        LevelMenu.SetActive(false);
        StartMenu.SetActive(false);
        inGameMenu.SetActive(true);
        PauseMenu.SetActive(false);
        LevelCompletePan.SetActive(false);
    }

    public void LoadNextLevel()
    {
        UnloadCurrentLevel();
        CurrentLevelIndex++;

        LoadLevel(CurrentLevelIndex);
        LevelCompletePan.SetActive(false);
    }

    public void ShowComplete()
    {
        if (CurrentLevelIndex >= Levels.Length-1)
        {
            ResetPan.SetActive(true);
            return;
        }
       
        LevelCompletePan.SetActive(true);
    }

    public void Reset()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);

        Destroy(gameObject);
    }

}
