using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public static SceneSwitcher Instance { get; private set; }

    [Tooltip("Nombres de las escenas a ciclar, en orden. Deben estar agregadas en Build Settings.")]
    public string[] sceneNames;

    private int currentIndex = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        string activeScene = SceneManager.GetActiveScene().name;
        int foundIndex = System.Array.IndexOf(sceneNames, activeScene);
        if (foundIndex >= 0)
            currentIndex = foundIndex;
    }

    void Update()
    {
        if (sceneNames == null || sceneNames.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            NextScene();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PreviousScene();
        }
    }

    public void NextScene()
    {
        currentIndex = (currentIndex + 1) % sceneNames.Length;
        LoadCurrentScene();
    }

    public void PreviousScene()
    {
        currentIndex = (currentIndex - 1 + sceneNames.Length) % sceneNames.Length;
        LoadCurrentScene();
    }

    private void LoadCurrentScene()
    {
        SceneManager.LoadScene(sceneNames[currentIndex]);
    }
}