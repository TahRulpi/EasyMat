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

    [Header("── PANELS ──")]
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject homepagePanel;

    [Header("── LOGIN PANEL FIELDS ──")]
    public TMP_InputField login_Email;
    public TMP_InputField login_Password;
    public TMP_Text login_Status;
    public Button login_ConfirmButton;
    public Button login_SwitchButton;     // "Register" link

    [Header("── REGISTER PANEL FIELDS ──")]
    public TMP_InputField reg_Username;
    public TMP_InputField reg_Email;
    public TMP_InputField reg_Phone;
    public TMP_InputField reg_Password;
    public TMP_InputField reg_ConfirmPassword;
    public TMP_Text reg_Status;
    public Button reg_ConfirmButton;
    public Button reg_SwitchButton;       // "Login" link

    [Header("── SHARED ──")]
    public GameObject loadingPanel;

    // ── START ──────────────────────────────────────────────────────
    void Start()
    {
        Debug.Log("=== FirebaseAuthManager START | Instance: "
                  + this.GetInstanceID()
                  + " | GameObject: " + this.gameObject.name);

        // Show login panel only
        ShowPanel("login");

        // Disable confirm buttons until Firebase is ready
        SetLoginConfirmInteractable(false);
        SetRegisterConfirmInteractable(false);

        SetLoginStatus("Initializing...");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;
                    firebaseReady = true;
                    Debug.Log("✅ Firebase Ready!");

                    SetLoginConfirmInteractable(true);
                    SetRegisterConfirmInteractable(true);
                    SetLoginStatus("Please login or register.");
                }
                else
                {
                    SetLoginStatus("Firebase failed to load.");
                    Debug.LogError("Firebase FAILED: " + task.Result);
                }
            });
        });
    }

    // ── LOGIN CONFIRM ──────────────────────────────────────────────
    public void OnLoginConfirmButton()
    {
        if (isProcessing) { SetLoginStatus("Please wait..."); return; }
        if (!firebaseReady) { SetLoginStatus("Not ready, wait..."); return; }

        string email = login_Email != null ? login_Email.text.Trim() : "";
        string password = login_Password != null ? login_Password.text : "";

        if (string.IsNullOrEmpty(email))
        {
            SetLoginStatus("Please enter your email.");
            return;
        }
        if (!IsValidEmail(email))
        {
            SetLoginStatus("Please enter a valid email.");
            return;
        }
        if (string.IsNullOrEmpty(password))
        {
            SetLoginStatus("Please enter your password.");
            return;
        }
        if (password.Length < 6)
        {
            SetLoginStatus("Password must be at least 6 characters.");
            return;
        }

        Login(email, password);
    }

    // ── REGISTER CONFIRM ───────────────────────────────────────────
    public void OnRegisterConfirmButton()
    {
        if (isProcessing) { SetRegisterStatus("Please wait..."); return; }
        if (!firebaseReady) { SetRegisterStatus("Not ready, wait..."); return; }

        string username = reg_Username != null ? reg_Username.text.Trim() : "";
        string email = reg_Email != null ? reg_Email.text.Trim() : "";
        string phone = reg_Phone != null ? reg_Phone.text.Trim() : "";
        string password = reg_Password != null ? reg_Password.text : "";
        string confirmPassword = reg_ConfirmPassword != null ? reg_ConfirmPassword.text : "";

        if (string.IsNullOrEmpty(username))
        {
            SetRegisterStatus("Please enter a username.");
            return;
        }
        if (string.IsNullOrEmpty(email))
        {
            SetRegisterStatus("Please enter your email.");
            return;
        }
        if (!IsValidEmail(email))
        {
            SetRegisterStatus("Please enter a valid email.");
            return;
        }
        if (string.IsNullOrEmpty(phone))
        {
            SetRegisterStatus("Please enter your phone number.");
            return;
        }
        if (!IsValidPhone(phone))
        {
            SetRegisterStatus("Please enter a valid phone number.");
            return;
        }
        if (string.IsNullOrEmpty(password))
        {
            SetRegisterStatus("Please enter a password.");
            return;
        }
        if (password.Length < 6)
        {
            SetRegisterStatus("Password must be at least 6 characters.");
            return;
        }
        if (string.IsNullOrEmpty(confirmPassword))
        {
            SetRegisterStatus("Please confirm your password.");
            return;
        }
        if (password != confirmPassword)
        {
            SetRegisterStatus("Passwords do not match.");
            return;
        }

        Register(email, password, username, phone);
    }

    // ── SWITCH TO REGISTER ─────────────────────────────────────────
    public void OnGoToRegister()
    {
        if (isProcessing) { isProcessing = false; ShowLoading(false); }
        ClearLoginFields();
        SetLoginStatus("");
        ShowPanel("register");
        Debug.Log("Switched to REGISTER panel");
    }

    // ── SWITCH TO LOGIN ────────────────────────────────────────────
    public void OnGoToLogin()
    {
        if (isProcessing) { isProcessing = false; ShowLoading(false); }
        ClearRegisterFields();
        SetRegisterStatus("");
        ShowPanel("login");
        Debug.Log("Switched to LOGIN panel");
    }

    // ── LOGIN LOGIC ────────────────────────────────────────────────
    void Login(string email, string password)
    {
        isProcessing = true;
        SetLoginConfirmInteractable(false);
        ShowLoading(true);
        SetLoginStatus("Logging in...");

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                isProcessing = false;
                ShowLoading(false);
                SetLoginConfirmInteractable(true);

                if (task.IsCanceled)
                {
                    SetLoginStatus("Login cancelled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    string error = GetFirebaseError(task.Exception);
                    SetLoginStatus(error);
                    // ✅ User can now freely click "Register" to switch panel
                    return;
                }

                Debug.Log("✅ Logged in: " + task.Result.User.Email);
                SetLoginStatus("Login successful!");
                ClearLoginFields();
                ShowPanel("homepage");
            });
        });
    }

    // ── REGISTER LOGIC ─────────────────────────────────────────────
    void Register(string email, string password, string username, string phone)
    {
        isProcessing = true;
        SetRegisterConfirmInteractable(false);
        ShowLoading(true);
        SetRegisterStatus("Creating account...");

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                isProcessing = false;
                ShowLoading(false);
                SetRegisterConfirmInteractable(true);

                if (task.IsCanceled)
                {
                    SetRegisterStatus("Registration cancelled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    string error = GetFirebaseError(task.Exception);
                    SetRegisterStatus(error);
                    return;
                }

                // ✅ Save display name
                FirebaseUser user = task.Result.User;
                UserProfile profile = new UserProfile { DisplayName = username };
                user.UpdateUserProfileAsync(profile).ContinueWith(profileTask =>
                {
                    Debug.Log(profileTask.IsCompleted
                        ? "✅ Username saved: " + username
                        : "⚠️ Failed to save username.");
                });

                ClearRegisterFields();
                SetRegisterStatus("");
                ShowPanel("login");
                SetLoginStatus("✅ Registered! Please login.");
            });
        });
    }

    // ── LOGOUT ─────────────────────────────────────────────────────
    public void Logout()
    {
        auth?.SignOut();
        ClearLoginFields();
        ClearRegisterFields();
        SetLoginStatus("You have been logged out.");
        ShowPanel("login");
        Debug.Log("User logged out.");
    }

    // ── PANEL SWITCHER ─────────────────────────────────────────────
    void ShowPanel(string panel)
    {
        if (loginPanel) loginPanel.SetActive(panel == "login");
        if (registerPanel) registerPanel.SetActive(panel == "register");
        if (homepagePanel) homepagePanel.SetActive(panel == "homepage");
        Debug.Log("Active panel: " + panel);
    }

    // ── CLEAR FIELDS ───────────────────────────────────────────────
    void ClearLoginFields()
    {
        if (login_Email) login_Email.text = "";
        if (login_Password) login_Password.text = "";
    }

    void ClearRegisterFields()
    {
        if (reg_Username) reg_Username.text = "";
        if (reg_Email) reg_Email.text = "";
        if (reg_Phone) reg_Phone.text = "";
        if (reg_Password) reg_Password.text = "";
        if (reg_ConfirmPassword) reg_ConfirmPassword.text = "";
    }

    // ── STATUS TEXT ────────────────────────────────────────────────
    void SetLoginStatus(string msg)
    {
        if (login_Status != null) login_Status.text = msg;
    }

    void SetRegisterStatus(string msg)
    {
        if (reg_Status != null) reg_Status.text = msg;
    }

    // ── BUTTON INTERACTABLE ────────────────────────────────────────
    void SetLoginConfirmInteractable(bool v)
    {
        if (login_ConfirmButton != null) login_ConfirmButton.interactable = v;
    }

    void SetRegisterConfirmInteractable(bool v)
    {
        if (reg_ConfirmButton != null) reg_ConfirmButton.interactable = v;
    }

    // ── LOADING ────────────────────────────────────────────────────
    void ShowLoading(bool show)
    {
        if (loadingPanel != null) loadingPanel.SetActive(show);
    }

    // ── VALIDATION ─────────────────────────────────────────────────
    bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$",
            RegexOptions.IgnoreCase);
    }

    bool IsValidPhone(string phone)
    {
        return Regex.IsMatch(phone, @"^\+?[0-9]{7,15}$");
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
                AuthError.EmailAlreadyInUse => "Email already in use. Try logging in.",
                AuthError.InvalidEmail => "Invalid email address.",
                AuthError.WeakPassword => "Password is too weak.",
                AuthError.WrongPassword => "Wrong password. Try again.",
                AuthError.UserNotFound => "No account found. Please register.",
                AuthError.NetworkRequestFailed => "Network error. Check connection.",
                AuthError.TooManyRequests => "Too many attempts. Try later.",
                _ => firebaseEx.Message
            };
        }

        return exception.Message;
    }
}