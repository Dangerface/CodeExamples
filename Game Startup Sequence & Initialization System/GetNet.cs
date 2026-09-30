using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

[DefaultExecutionOrder(-11)] 
public class GetNet : MonoBehaviour
{
    public bool isConnected = false;
    public Button retry;
    public TextMeshProUGUI m_Text;
    void Start()
    {
        retry.gameObject.SetActive(false);
        Run();
        
    }
    void Run()
    {
        StartCoroutine(CheckInternetConnection(isConnected =>
    {
        if (isConnected)
        {
            retry.gameObject.SetActive(false);
        }
        else
            {retry.gameObject.SetActive(true); m_Text.text = "No internet access";}
    }));

    }

    IEnumerator CheckInternetConnection(Action<bool> action)
    {
        UnityWebRequest request = new UnityWebRequest("https://google.com");
        yield return request.SendWebRequest();
        if (request.error != null) {
            Debug.Log ("Internet off");
            action (false);
             isConnected = false;
        } else{
            Debug.Log ("Internet ON");
            action (true);
             isConnected = true;
        }
    }

    public void CheckAgain()
    {
        Run();
        
    }
}