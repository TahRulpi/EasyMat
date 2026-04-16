using UnityEngine;
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
   // private bool hasRegistered = false;

    [Header("Panels")]
    public GameObject loginPanel;       // drag your LoginPanel here
    public GameObject homepagePanel;    // drag your Homepage panel here

    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;
    public TMP_Text titleText;
    public TMP_Text switchText;
    public TMP_Text switchLinkText;
    public Button confirmButton;
    public GameObject loadingPanel;


    public TMP_InputField usernameInput;
    public TMP_InputField confirmPasswordInput;


    // ── UNITY START ────────────────────────────────────────────────
    void Start()
    {
        Debug.Log("=== SCRIPT IS RUNNING ===");

        // Make sure homepage is hidden at start
        ShowHomepage(false);

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
                    Debug.Log("Firebase Ready!");

                    // If already logged in from a previous session -> go straight to homepage
                   /* if (auth.CurrentUser != null)
                    {
                        Debug.Log("Already logged in: " + auth.CurrentUser.Email);
                        ShowHomepage(true);
                    }*/
                   // else
                    
                        SetConfirmButtonInteractable(true);
                        SetLoginMode();
                        SetStatus("Please login or register.");
                    
                }
                else
                {
                    SetStatus("Firebase failed to load.");
                    Debug.LogError("Firebase FAILED: " + task.Result);
                }
            });
        });
    }

    // ── CONFIRM BUTTON ─────────────────────────────────────────────
    public void OnConfirmButton()
    {
        if (isProcessing)
        {
            SetStatus("Please wait...");
            return;
        }

        if (!firebaseReady)
        {
            SetStatus("Firebase not ready. Please wait.");
            return;
        }

        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string username = usernameInput != null ? usernameInput.text.Trim() : "";
        string confirmPassword = confirmPasswordInput != null ? confirmPasswordInput.text : "";

        // ── EMAIL VALIDATION ─────────────────────
        if (string.IsNullOrEmpty(email))
        {
            SetStatus("Please enter your email.");
            return;
        }

        if (!IsValidEmail(email))
        {
            SetStatus("Please enter a valid email address.");
            return;
        }

        // ── PASSWORD VALIDATION ──────────────────
        if (string.IsNullOrEmpty(password))
        {
            SetStatus("Please enter your password.");
            return;
        }

        if (password.Length < 6)
        {
            SetStatus("Password must be at least 6 characters.");
            return;
        }

        // ── REGISTER MODE VALIDATION ─────────────
        if (!isLoginMode)
        {
            if (string.IsNullOrEmpty(username))
            {
                SetStatus("Please enter a username.");
                return;
            }

            if (string.IsNullOrEmpty(confirmPassword))
            {
                SetStatus("Please confirm your password.");
                return;
            }

            if (password != confirmPassword)
            {
                SetStatus("Passwords do not match.");
                return;
            }

            // ✅ REGISTER
            Register(email, password);
        }
        else
        {
            // ✅ LOGIN
            Login(email, password);
        }
    }

    // ── EMAIL VALIDATION ───────────────────────────────────────────
    bool IsValidEmail(string email)
    {
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

        string username = usernameInput != null ? usernameInput.text.Trim() : "";

        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWith(task =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    isProcessing = false;
                    ShowLoading(false);
                    SetConfirmButtonInteractable(true);

                    // ❌ Cancelled
                    if (task.IsCanceled)
                    {
                        SetStatus("Registration cancelled.");
                        return;
                    }

                    // ❌ Error
                    if (task.IsFaulted)
                    {
                        string error = GetFirebaseError(task.Exception);
                        SetStatus(error);
                        Debug.LogError("Register failed: " + error);
                        return;
                    }

                    // ✅ SUCCESS
                    FirebaseUser user = task.Result.User;
                    Debug.Log("Account created: " + user.Email);

                    // 🔥 SAVE USERNAME (IMPORTANT FIX)
                    if (!string.IsNullOrEmpty(username))
                    {
                        UserProfile profile = new UserProfile
                        {
                            DisplayName = username
                        };

                        user.UpdateUserProfileAsync(profile).ContinueWith(profileTask =>
                        {
                            if (profileTask.IsCompleted)
                            {
                                Debug.Log("Username saved: " + username);
                            }
                            else
                            {
                                Debug.LogWarning("Failed to save username.");
                            }
                        });
                    }

                    // ✅ Clear inputs
                    emailInput.text = "";
                    passwordInput.text = "";
                    confirmPasswordInput.text = "";
                    if (usernameInput != null)
                        usernameInput.text = "";

                    // ✅ Switch to login panel
                    SetLoginMode();

                    // ✅ Final message
                    SetStatus("Registration complete. Please login.");
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

                    // ❌ Cancelled
                    if (task.IsCanceled)
                    {
                        SetStatus("Login cancelled.");
                        return;
                    }

                    // ❌ Error handling (FIXED PROPERLY)
                    if (task.IsFaulted)
                    {
                        string error = GetFirebaseError(task.Exception);

                        switch (error)
                        {
                            case "No account found. Please register first.":
                                SetStatus("Account not found. Please register first.");
                                break;

                            case "Incorrect password.":
                                SetStatus("Wrong password. Try again.");
                                break;

                            case "Invalid email address.":
                                SetStatus("Invalid email format.");
                                break;

                            case "Network error. Check your connection.":
                                SetStatus("Network issue. Try again.");
                                break;

                            default:
                                SetStatus(error);
                                break;
                        }

                        Debug.LogError("Login failed: " + error);
                        return;
                    }

                    // ✅ SUCCESS
                    Debug.Log("Logged in: " + task.Result.User.Email);

                    SetStatus("Login successful!");
                    ShowHomepage(true);

                    // Clean inputs (important)
                    emailInput.text = "";
                    passwordInput.text = "";
                    if (confirmPasswordInput != null)
                        confirmPasswordInput.text = "";
                    if (usernameInput != null)
                        usernameInput.text = "";
                });
            });
    }

    // ── SWITCH LOGIN / REGISTER MODE ───────────────────────────────
    public void OnSwitchModeButton()
    {
        if (isProcessing)
        {
            SetStatus("Please wait before switching.");
            return;
        }

        if (isLoginMode)
            SetRegisterMode();
        else
            SetLoginMode();

        // Clear ALL fields properly
        emailInput.text = "";
        passwordInput.text = "";
        confirmPasswordInput.text = "";

        if (usernameInput != null)
            usernameInput.text = "";

        SetStatus("");
    }

    /*void SetLoginMode()
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
    }*/


    void SetLoginMode()
    {
        isLoginMode = true;

        if (titleText) titleText.text = "Log in to your account";
        if (switchText) switchText.text = "Don't have an account?";
        if (switchLinkText) switchLinkText.text = "Register";

        // Hide register fields
        if (usernameInput) usernameInput.gameObject.SetActive(false);
        if (confirmPasswordInput) confirmPasswordInput.gameObject.SetActive(false);

        Debug.Log("Mode: LOGIN");
    }

    void SetRegisterMode()
    {
        isLoginMode = false;

        if (titleText) titleText.text = "Create your account";
        if (switchText) switchText.text = "Already have an account?";
        if (switchLinkText) switchLinkText.text = "Login";

        // Show register fields
        if (usernameInput) usernameInput.gameObject.SetActive(true);
        if (confirmPasswordInput) confirmPasswordInput.gameObject.SetActive(true);

        Debug.Log("Mode: REGISTER");
    }

    // ── LOGOUT ─────────────────────────────────────────────────────
    public void Logout()
    {
        auth?.SignOut();
        Debug.Log("User signed out.");
        emailInput.text = "";
        passwordInput.text = "";
        SetLoginMode();
        SetStatus("You have been logged out.");
        ShowHomepage(false);
    }

    // ── PANEL SWITCHER ─────────────────────────────────────────────
    void ShowHomepage(bool show)
    {
        if (loginPanel != null) loginPanel.SetActive(!show);
        if (homepagePanel != null) homepagePanel.SetActive(show);
        Debug.Log(show ? "Homepage shown" : "LoginPanel shown");
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