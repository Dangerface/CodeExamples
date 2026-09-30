using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[DefaultExecutionOrder(-7)] 
public class LoadSceneAsync : MonoBehaviour
{
    public TextMeshProUGUI m_Text;
    private SetBaseTime setbasetimescript;
    private GetNet getnetscript;
    private bool sceneLoaded = false;

    void Start()
    {
        setbasetimescript = GetComponent<SetBaseTime>();
        getnetscript = GetComponent<GetNet>();
    }

    void Update()
    {
        // Only run this logic if we haven't started loading yet
        if (!sceneLoaded)
        {
            if (!getnetscript.isConnected)
            {
                m_Text.text = "Waiting for network connection...";
            }
            else if (!setbasetimescript.lockBaseTime)
            {
                m_Text.text = "Waiting for base time sync...";
            }
            else
            {
                // All conditions met, start loading
                StartCoroutine(LoadScene());
                sceneLoaded = true;
            }
        }
    }

    IEnumerator LoadScene()
    {
        yield return null;
        
        // Begin to load the Scene you specify
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Main");
        
        // Don't let the Scene activate until you allow it to
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            // Output current progress while under 90%
            if (asyncOperation.progress < 0.9f)
            {
                m_Text.text = "Loading progress: " + Mathf.Round(asyncOperation.progress * 100) + "%";
            }
            else
            {
                // Reached 90%+, check if we are ready to activate
                if (setbasetimescript.lockBaseTime)
                {
                    m_Text.text = "Loading progress: 100%";
                    // Activate the Scene
                    asyncOperation.allowSceneActivation = true;
                }
                else
                {
                    // Fallback if lockBaseTime becomes false at the last second
                    m_Text.text = "Ready, waiting for base time lock...";
                }
            }

            yield return null;
        }
    }
}