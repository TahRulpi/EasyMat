using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;

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

    // ── UNITY START ────────────────────────────────────────────────
    void Start()
    {
        Debug.Log("=== SCRIPT IS RUNNING ===");

        // Block confirm button until Firebase is ready
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

                    // Only go to game if a session is already persisted
                    if (auth.CurrentUser != null)
                    {
                        Debug.Log("Already logged in: " + auth.CurrentUser.Email);
                        LoadGameScene();
                    }
                    else
                    {
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
        // Block if already processing
        if (isProcessing)
        {
            SetStatus("⏳ Please wait...");
            return;
        }

        // Block if Firebase not ready
        if (!firebaseReady)
        {
            SetStatus("⏳ Firebase not ready. Please wait.");
            return;
        }

        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        // ── CLIENT-SIDE VALIDATION (runs before ANY Firebase call) ──

        // 1. Email empty check
        if (string.IsNullOrEmpty(email))
        {
            SetStatus("❌ Please enter your email.");
            return;
        }

        // 2. Email format check (valid structure like user@domain.com)
        if (!IsValidEmail(email))
        {
            SetStatus("❌ Please enter a valid email address.");
            return;
        }

        // 3. Password empty check
        if (string.IsNullOrEmpty(password))
        {
            SetStatus("❌ Please enter your password.");
            return;
        }

        // 4. Password minimum length check
        if (password.Length < 6)
        {
            SetStatus("❌ Password must be at least 6 characters.");
            return;
        }

        // All checks passed — proceed
        if (isLoginMode)
            Login(email, password);
        else
            Register(email, password);
    }

    // ── EMAIL VALIDATION HELPER ────────────────────────────────────
    // Checks for a proper email format: something@something.something
    bool IsValidEmail(string email)
    {
        // Standard email regex — rejects "abc", "abc@", "@abc.com", "abc@abc", etc.
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$";
        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }

    // ── REGISTER ───────────────────────────────────────────────────
    void Register(string email, string password)
    {
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
                        return; // Stay on screen
                    }

                    // Success — load game
                    Debug.Log("✅ Account created: " + task.Result.User.Email);
                    SetStatus("✅ Account created! Loading game...");
                    LoadGameScene();
                });
            });
    }

    // ── LOGIN ──────────────────────────────────────────────────────
    void Login(string email, string password)
    {
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
                        return; // Stay on screen
                    }

                    // Success — load game
                    Debug.Log("✅ Logged in: " + task.Result.User.Email);
                    SetStatus("✅ Welcome! Loading game...");
                    LoadGameScene();
                });
            });
    }

    // ── SWITCH LOGIN / REGISTER MODE ───────────────────────────────
    public void OnSwitchModeButton()
    {
        // Don't allow switching while a request is in-flight
        if (isProcessing)
        {
            SetStatus("⏳ Please wait before switching modes.");
            return;
        }

        // Toggle mode
        if (isLoginMode)
            SetRegisterMode();
        else
            SetLoginMode();

        // Always clear fields and status when switching
        emailInput.text = "";
        passwordInput.text = "";
        SetStatus("");
    }

    void SetLoginMode()
    {
        isLoginMode = true;
        if (titleText) titleText.text = "Log in to your account";
        if (switchText) switchText.text = "Don't have an account?";
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
        // Final safety check before loading
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
                AuthError.EmailAlreadyInUse => "Email already in use. Try logging in instead.",
                AuthError.InvalidEmail => "Invalid email address.",
                AuthError.WeakPassword => "Password is too weak.",
                AuthError.WrongPassword => "Incorrect password.",
                AuthError.UserNotFound => "No account found. Please register first.",
                AuthError.NetworkRequestFailed => "Network error. Check your connection.",
                AuthError.TooManyRequests => "Too many attempts. Please try later.",
                _ => firebaseEx.Message
            };
        }

        return exception.Message;
    }
}