using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using TMPro;

public class FirebaseAuthManager : MonoBehaviour
{
    private FirebaseAuth auth;

    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;

                // Already logged in? Skip login screen
                if (auth.CurrentUser != null)
                    LoadGameScene();
            }
            else
            {
                Debug.LogError("Firebase failed: " + task.Result);
            }
        });
    }

    public void Register()
    {
        auth.CreateUserWithEmailAndPasswordAsync(
            emailInput.text, passwordInput.text
        ).ContinueWith(task => {
            if (task.IsFaulted)
            {
                statusText.text = "Error: " + task.Exception.Message;
                return;
            }
            statusText.text = "Account created!";
            LoadGameScene();
        });
    }

    public void Login()
    {
        auth.SignInWithEmailAndPasswordAsync(
            emailInput.text, passwordInput.text
        ).ContinueWith(task => {
            if (task.IsFaulted)
            {
                statusText.text = "Login failed: " + task.Exception.Message;
                return;
            }
            statusText.text = "Welcome back!";
            LoadGameScene();
        });
    }

    public void Logout()
    {
        auth.SignOut();
        SceneManager.LoadScene("LoginScene");
    }

    void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene"); // your AR/Map scene
    }
}