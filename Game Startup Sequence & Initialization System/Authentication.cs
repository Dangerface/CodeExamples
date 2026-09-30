using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using UnityEngine.UI;

[DefaultExecutionOrder(-10)]
public class Authentication : MonoBehaviour
{
  public bool signedIn=false;
  public bool signInProgress=false;
  private GetNet getnetscript;
  public Button retry;
  void Awake()
  {
      getnetscript = GetComponent<GetNet>();
  }
  
    void Update()
    {
      if(!signInProgress && getnetscript.isConnected )
      {
      ForRealStartLogIn();
      
      }
    }

async void ForRealStartLogIn()
    {
      signInProgress=true;
          await UnityServices.InitializeAsync();
               
          SetupEvents();

          await SignInAnonymouslyAsync();
    }
void SetupEvents() 
{
  AuthenticationService.Instance.SignedIn += () => 
	{
		Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
		// Shows how to get an access token
		//Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

	};

  AuthenticationService.Instance.SignInFailed += (err) => 
	{
		retry.gameObject.SetActive(true);
    signInProgress=false;
	};

  AuthenticationService.Instance.SignedOut += () => 
	{
		Debug.Log("Player signed out.");
    retry.gameObject.SetActive(true);
    signedIn = false;
    signInProgress=false;
	};
 
  AuthenticationService.Instance.Expired += () =>
    {
        Debug.Log("Player session could not be refreshed and expired.");
        retry.gameObject.SetActive(true);
        signedIn = false;
        signInProgress=false;
    };
}

async Task SignInAnonymouslyAsync()
{
    try
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        //Debug.Log("Sign in anonymously succeeded!");
        signedIn = true;

    }
    catch (AuthenticationException ex)
    {
        
        Debug.LogException(ex);
    }
    catch (RequestFailedException ex)
    {
        
        Debug.LogException(ex);
     }
     
}
}
