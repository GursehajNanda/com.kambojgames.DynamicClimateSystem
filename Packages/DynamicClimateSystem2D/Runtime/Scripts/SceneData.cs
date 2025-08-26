using KambojGames.Utilities2D;
using UnityEngine;
using UnityEngine.SceneManagement;


[CreateAssetMenu(fileName = "SceneData", menuName = "ScriptableObject/SceneData")]
public class SceneData : ScriptableObject
{
    private static SceneData m_instance;
    private string m_activeScene;
    public string ActiveScene => m_activeScene;

    public System.Action OnSceneLoadedAction;
    public System.Action OnSceneUnloadedAction;
    public System.Action<Scene,LoadSceneMode> OnSceneLoadedWithScene;
    public System.Action<Scene> OnSceneUnloadedActionWithScene;


    public static SceneData Instance
    {
        get
        {
            if(m_instance == null)
            {
                m_instance = Resources.Load<SceneData>("SceneData");
                if (m_instance == null)
                {
                    Debug.LogError("SceneData instance not found in Resources.");
                }
            }
            return m_instance;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }



    public void LoadScene(string sceneName)
    {
        HandleDataBeforeLoadScene();
        SceneManager.LoadScene(sceneName);
    }

    //invoke action OnSceneBeforeLoadedAction action
    //remove this later and add it to the dynacmic night and day script
    private void HandleDataBeforeLoadScene()
    {
        GameObject orbitCenter = GameObject.FindGameObjectWithTag("OrbitCenter");
        if (orbitCenter != null)
        {
            orbitCenter.transform.parent = null;
            Destroy(orbitCenter);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        m_activeScene = scene.name;
        OnSceneLoadedWithScene?.Invoke(scene,mode);
        OnSceneLoadedAction?.Invoke();
    }

    void OnSceneUnloaded(Scene scene)
    {
        OnSceneUnloadedActionWithScene?.Invoke(scene);
        OnSceneUnloadedAction?.Invoke();
    }


   
}

