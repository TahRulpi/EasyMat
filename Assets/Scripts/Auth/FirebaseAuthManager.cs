using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine.UI;

public class FirebaseAuthManager : MonoBehaviour
{
    private FirebaseAuth auth;
    private bool firebaseReady = false;
    private bool isLoginMode = true;
    private bool isProcessing = false;

    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;
    public TMP_Text titleText;
    public TMP_Text switchText;
    public TMP_Text switchLinkText;
    public Button confirmButton;
    public GameObject loadingPanel;

    void Start()
    {

        Debug.Log("=== SCRIPT IS RUNNING ===");
        // ✅ BLOCK confirm button until Firebase is ready
        SetConfirmButtonInteractable(false);
        SetStatus("Initializing Firebase...");
        Debug.Log("=== Firebase Init Starting ===");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;
                    firebaseReady = true;
                    Debug.Log("✅ Firebase Ready!");

                    // ✅ CRITICAL: Only go to game if ALREADY logged in from before
                    // If no user, stay on login screen — DO NOT go to game
                    if (auth.CurrentUser != null)
                    {
                        Debug.Log("Already logged in: " + auth.CurrentUser.Email);
                        LoadGameScene();
                    }
                    else
                    {
                        // Stay on login screen, enable confirm button now
                        SetConfirmButtonInteractable(true);
                        SetLoginMode();
                        SetStatus("Please login or register.");
                    }
                }
                else
                {
                    SetStatus("❌ Firebase failed to load.");
                    Debug.LogError("Firebase FAILED: " + task.Result);
                }
            });
        });
    }

    // ── CONFIRM BUTTON ─────────────────────────────────────────────
    public void OnConfirmButton()
    {
        // ✅ BLOCK if already processing
        if (isProcessing)
        {
            SetStatus("Please wait...");
            return;
        }

        // ✅ BLOCK if Firebase not ready
        if (!firebaseReady)
        {
            SetStatus("Firebase not ready. Please wait.");
            return;
        }

        // ✅ BLOCK if email is empty
        if (string.IsNullOrEmpty(emailInput.text.Trim()))
        {
            SetStatus("❌ Please enter your email.");
            return;
        }

        // ✅ BLOCK if password is empty
        if (string.IsNullOrEmpty(passwordInput.text))
        {
            SetStatus("❌ Please enter your password.");
            return;
        }

        // ✅ BLOCK if password too short
        if (passwordInput.text.Length < 6)
        {
            SetStatus("❌ Password must be at least 6 characters.");
            return;
        }

        // ✅ Now decide login or register
        if (isLoginMode)
            Login();
        else
            Register();
    }

    // ── REGISTER ───────────────────────────────────────────────────
    void Register()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        isProcessing = true;
        SetConfirmButtonInteractable(false);
        ShowLoading(true);
        SetStatus("Creating account...");
        Debug.Log("Attempting Register: " + email);

        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWith(task =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    isProcessing = false;
                    ShowLoading(false);
                    SetConfirmButtonInteractable(true);

                    if (task.IsCanceled)
                    {
                        SetStatus("❌ Registration cancelled.");
                        Debug.LogWarning("Register cancelled.");
                        return;
                    }

                    if (task.IsFaulted)
                    {
                        string error = GetFirebaseError(task.Exception);
                        SetStatus("❌ " + error);
                        Debug.LogError("Register failed: " + error);
                        // ✅ STAY ON SCREEN — do NOT load game
                        return;
                    }

                    // ✅ ONLY load game after confirmed success
                    Debug.Log("✅ Account created: " + task.Result.User.Email);
                    SetStatus("✅ Account created! Loading game...");
                    LoadGameScene(); // ← ONLY called here on success
                });
            });
    }

    // ── LOGIN ──────────────────────────────────────────────────────
    void Login()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        isProcessing = true;
        SetConfirmButtonInteractable(false);
        ShowLoading(true);
        SetStatus("Logging in...");
        Debug.Log("Attempting Login: " + email);

        auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWith(task =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    isProcessing = false;
                    ShowLoading(false);
                    SetConfirmButtonInteractable(true);

                    if (task.IsCanceled)
                    {
                        SetStatus("❌ Login cancelled.");
                        Debug.LogWarning("Login cancelled.");
                        return;
                    }

                    if (task.IsFaulted)
                    {
                        string error = GetFirebaseError(task.Exception);
                        SetStatus("❌ " + error);
                        Debug.LogError("Login failed: " + error);
                        // ✅ STAY ON SCREEN — do NOT load game
                        return;
                    }

                    // ✅ ONLY load game after confirmed success
                    Debug.Log("✅ Logged in: " + task.Result.User.Email);
                    SetStatus("✅ Welcome! Loading game...");
                    LoadGameScene(); // ← ONLY called here on success
                });
            });
    }

    // ── SWITCH LOGIN / REGISTER MODE ───────────────────────────────
    public void OnSwitchModeButton()
    {
        if (isLoginMode)
            SetRegisterMode();
        else
            SetLoginMode();

        // Clear fields when switching
        emailInput.text = "";
        passwordInput.text = "";
        SetStatus("");
    }

    void SetLoginMode()
    {
        isLoginMode = true;
        if (titleText) titleText.text = "Log in to your account";
        if (switchText) switchText.text = "Don't have any account?";
        if (switchLinkText) switchLinkText.text = "Register";
        Debug.Log("Mode: LOGIN");
    }

    void SetRegisterMode()
    {
        isLoginMode = false;
        if (titleText) titleText.text = "Create your account";
        if (switchText) switchText.text = "Already have an account?";
        if (switchLinkText) switchLinkText.text = "Login";
        Debug.Log("Mode: REGISTER");
    }

    // ── LOGOUT ─────────────────────────────────────────────────────
    public void Logout()
    {
        auth?.SignOut();
        Debug.Log("User signed out.");
        SceneManager.LoadScene("LoginScene");
    }

    // ── LOAD GAME ──────────────────────────────────────────────────
    void LoadGameScene()
    {
        // ✅ FINAL SAFETY CHECK before loading
        if (auth.CurrentUser == null)
        {
            SetStatus("❌ Not authenticated. Please login.");
            Debug.LogError("Tried to load game without auth!");
            return;
        }

        Debug.Log("✅ Loading GameScene for: " + auth.CurrentUser.Email);
        SceneManager.LoadScene("GameScene");
    }

    // ── UI HELPERS ─────────────────────────────────────────────────
    void SetConfirmButtonInteractable(bool interactable)
    {
        if (confirmButton != null)
            confirmButton.interactable = interactable;
    }

    void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    void ShowLoading(bool show)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(show);
    }

    // ── FIREBASE ERROR HANDLER ─────────────────────────────────────
    string GetFirebaseError(System.AggregateException exception)
    {
        Firebase.FirebaseException firebaseEx = null;
        foreach (var e in exception.Flatten().InnerExceptions)
        {
            firebaseEx = e as Firebase.FirebaseException;
            if (firebaseEx != null) break;
        }

        if (firebaseEx != null)
        {
            var errorCode = (AuthError)firebaseEx.ErrorCode;
            return errorCode switch
            {
                AuthError.EmailAlreadyInUse => "Email already in use.",
                AuthError.InvalidEmail => "Invalid email address.",
                AuthError.WeakPassword => "Password is too weak.",
                AuthError.WrongPassword => "Incorrect password.",
                AuthError.UserNotFound => "No account with this email.",
                AuthError.NetworkRequestFailed => "Network error. Check connection.",
                AuthError.TooManyRequests => "Too many attempts. Try later.",
                _ => firebaseEx.Message
            };
        }
        return exception.Message;
    }
}