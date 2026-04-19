using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Firebase.Storage;
using System.IO;
using UnityEngine.Networking;
using System.Collections;
public class FirebaseAuthManager : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private bool firebaseReady = false;
    private bool isProcessing = false;

    [Header("── PANELS ──")]
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject homepagePanel;

    [Header("── LOGIN PANEL ──")]
    public TMP_InputField login_Email;
    public TMP_InputField login_Password;
    public TMP_Text login_Status;
    public Button login_ConfirmButton;

    [Header("── REGISTER PANEL ──")]
    public TMP_InputField reg_Username;
    public TMP_InputField reg_Email;
    public TMP_InputField reg_Phone;
    public TMP_InputField reg_Password;
    public TMP_InputField reg_ConfirmPassword;
    public TMP_Text reg_Status;
    public Button reg_ConfirmButton;

    [Header("── HOMEPAGE ──")]
    public TMP_Text home_UsernameText;   // ← drag your username TMP text here
    public Image home_ProfilePicture; // ← drag your profile Image here

    [Header("── SHARED ──")]
    public GameObject loadingPanel;




    [Header("── PROFILE INFO PANEL ──")]
    public TMP_Text profile_Username;   // drag "My Name" text (Profile Info section)
    public TMP_Text profile_Email;      // drag "username@email.com" text
    public TMP_Text profile_Phone;      // drag "+990231644" text
    public TMP_Text home_TopUsername;   // drag "My Name" text (top card section)

    // ── START ──────────────────────────────────────────────────────
    void Start()
    {
        ShowPanel("login");
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
                    db = FirebaseFirestore.DefaultInstance; // ← Firestore init
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

        if (string.IsNullOrEmpty(email)) { SetLoginStatus("Please enter your email."); return; }
        if (!IsValidEmail(email)) { SetLoginStatus("Invalid email address."); return; }
        if (string.IsNullOrEmpty(password)) { SetLoginStatus("Please enter your password."); return; }
        if (password.Length < 6) { SetLoginStatus("Password min 6 characters."); return; }

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

        if (string.IsNullOrEmpty(username)) { SetRegisterStatus("Please enter a username."); return; }
        if (username.Length < 3) { SetRegisterStatus("Username min 3 characters."); return; }
        if (!IsValidUsername(username)) { SetRegisterStatus("Username: letters, numbers, _ only."); return; }
        if (string.IsNullOrEmpty(email)) { SetRegisterStatus("Please enter your email."); return; }
        if (!IsValidEmail(email)) { SetRegisterStatus("Invalid email address."); return; }
        if (string.IsNullOrEmpty(phone)) { SetRegisterStatus("Please enter phone number."); return; }
        if (!IsValidPhone(phone)) { SetRegisterStatus("Invalid phone number."); return; }
        if (string.IsNullOrEmpty(password)) { SetRegisterStatus("Please enter a password."); return; }
        if (password.Length < 6) { SetRegisterStatus("Password min 6 characters."); return; }
        if (string.IsNullOrEmpty(confirmPassword)) { SetRegisterStatus("Please confirm your password."); return; }
        if (password != confirmPassword) { SetRegisterStatus("Passwords do not match."); return; }

        // ✅ Check username uniqueness FIRST, then register
        CheckUsernameAndRegister(username, email, password, phone);
    }

    // ── CHECK USERNAME UNIQUE THEN REGISTER ────────────────────────
    void CheckUsernameAndRegister(string username, string email, string password, string phone)
    {
        isProcessing = true;
        SetRegisterConfirmInteractable(false);
        ShowLoading(true);
        SetRegisterStatus("Checking username...");

        // Query Firestore: does this username already exist?
        db.Collection("usernames").Document(username.ToLower()).GetSnapshotAsync()
            .ContinueWith(task =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    if (task.IsFaulted)
                    {
                        isProcessing = false;
                        ShowLoading(false);
                        SetRegisterConfirmInteractable(true);
                        SetRegisterStatus("Error checking username. Try again.");
                        return;
                    }

                    // ❌ Username already taken
                    if (task.Result.Exists)
                    {
                        isProcessing = false;
                        ShowLoading(false);
                        SetRegisterConfirmInteractable(true);
                        SetRegisterStatus("❌ Username already taken. Choose another.");
                        return;
                    }

                    // ✅ Username is free — proceed to register
                    Register(email, password, username, phone);
                });
            });
    }

    // ── REGISTER ───────────────────────────────────────────────────
    void Register(string email, string password, string username, string phone)
    {
        SetRegisterStatus("Creating account...");

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (task.IsCanceled)
                {
                    isProcessing = false;
                    ShowLoading(false);
                    SetRegisterConfirmInteractable(true);
                    SetRegisterStatus("Registration cancelled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    isProcessing = false;
                    ShowLoading(false);
                    SetRegisterConfirmInteractable(true);
                    SetRegisterStatus(GetFirebaseError(task.Exception));
                    return;
                }

                FirebaseUser user = task.Result.User;

                // ── Save DisplayName ───────────────────────────────
                UserProfile profile = new UserProfile { DisplayName = username };
                user.UpdateUserProfileAsync(profile);

                // ── Save to Firestore ──────────────────────────────
                // 1. Reserve the username (for uniqueness check)
                db.Collection("usernames").Document(username.ToLower()).SetAsync(
                    new Dictionary<string, object> { { "uid", user.UserId } }
                );

                // 2. Save full user data
                db.Collection("users").Document(user.UserId).SetAsync(
                    new Dictionary<string, object>
                    {
                        { "username",  username        },
                        { "email",     email           },
                        { "phone",     phone           },
                        { "createdAt", FieldValue.ServerTimestamp }
                    }
                ).ContinueWith(dbTask =>
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        isProcessing = false;
                        ShowLoading(false);
                        SetRegisterConfirmInteractable(true);

                        if (dbTask.IsFaulted)
                        {
                            SetRegisterStatus("Account created but data save failed.");
                            Debug.LogError("Firestore save failed: " + dbTask.Exception);
                            return;
                        }

                        Debug.Log("✅ User registered and saved: " + username);
                        ClearRegisterFields();
                        ShowPanel("login");
                        SetLoginStatus("✅ Registered! Please login.");
                    });
                });
            });
        });
    }

    // ── LOGIN ──────────────────────────────────────────────────────
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

                if (task.IsCanceled) { SetLoginStatus("Login cancelled."); return; }

                if (task.IsFaulted)
                {
                    SetLoginStatus(GetFirebaseError(task.Exception));
                    return;
                }

                FirebaseUser user = task.Result.User;
                Debug.Log("✅ Logged in: " + user.Email);

                // ── Fetch username from Firestore ──────────────────
                LoadUserDataAndShowHomepage(user);
            });
        });
    }

    // ── LOAD USER DATA → HOMEPAGE ──────────────────────────────────
    void LoadUserDataAndShowHomepage(FirebaseUser user)
    {
        SetLoginStatus("Loading profile...");
        ShowLoading(true);

        db.Collection("users").Document(user.UserId).GetSnapshotAsync()
            .ContinueWith(task =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    ShowLoading(false);

                    // ── Always read from Firebase Auth first ───────────
                    // DisplayName = username (saved during register)
                    // Email       = always stored in Firebase Auth
                    string displayUsername = user.DisplayName;  // from Firebase Auth
                    string displayEmail = user.Email;        // from Firebase Auth
                    string displayPhone = "";

                    // ── Then enrich with Firestore data ────────────────
                    if (!task.IsFaulted && task.Result.Exists)
                    {
                        // Override only if Firestore has the value
                        if (task.Result.TryGetValue("username", out string fsUsername)
                            && !string.IsNullOrEmpty(fsUsername))
                            displayUsername = fsUsername;

                        if (task.Result.TryGetValue("phone", out string fsPhone))
                            displayPhone = fsPhone;

                        // Email always comes from Firebase Auth — more reliable
                        // displayEmail = user.Email is already set above
                    }

                    // ── If DisplayName somehow empty, use Firestore ────
                    if (string.IsNullOrEmpty(displayUsername))
                    {
                        if (!task.IsFaulted && task.Result.Exists)
                            task.Result.TryGetValue("username", out displayUsername);
                    }

                    // ── Update all UI ──────────────────────────────────
                    if (home_UsernameText != null)
                        home_UsernameText.text = displayUsername ?? "";

                    if (home_TopUsername != null)
                        home_TopUsername.text = displayUsername ?? "";

                    if (profile_Username != null)
                        profile_Username.text = displayUsername ?? "";

                    if (profile_Email != null)
                        profile_Email.text = displayEmail ?? "";

                    if (profile_Phone != null)
                        profile_Phone.text = displayPhone ?? "";

                    ClearLoginFields();
                    ShowPanel("homepage");
                });
            });
    }

    // ── LOGOUT ─────────────────────────────────────────────────────
    public void Logout()
    {
        auth?.SignOut();
        ClearLoginFields();
        ClearRegisterFields();

        // ── Clear homepage ─────────────────────────────────────────────
        if (home_UsernameText != null) home_UsernameText.text = "";
        if (home_TopUsername != null) home_TopUsername.text = "";

        // ── Clear profile info ─────────────────────────────────────────
        if (profile_Username != null) profile_Username.text = "";
        if (profile_Email != null) profile_Email.text = "";
        if (profile_Phone != null) profile_Phone.text = "";

        SetLoginStatus("You have been logged out.");
        ShowPanel("login");
    }
    // ── SWITCH PANELS ──────────────────────────────────────────────
    public void OnGoToRegister()
    {
        if (isProcessing) { isProcessing = false; ShowLoading(false); }
        ClearLoginFields();
        SetLoginStatus("");
        ShowPanel("register");
    }

    public void OnGoToLogin()
    {
        if (isProcessing) { isProcessing = false; ShowLoading(false); }
        ClearRegisterFields();
        SetRegisterStatus("");
        ShowPanel("login");
    }

    // ── PANEL SWITCHER ─────────────────────────────────────────────
    void ShowPanel(string panel)
    {
        if (loginPanel) loginPanel.SetActive(panel == "login");
        if (registerPanel) registerPanel.SetActive(panel == "register");
        if (homepagePanel) homepagePanel.SetActive(panel == "homepage");
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

    // ── STATUS ─────────────────────────────────────────────────────
    void SetLoginStatus(string msg) { if (login_Status != null) login_Status.text = msg; }
    void SetRegisterStatus(string msg) { if (reg_Status != null) reg_Status.text = msg; }

    // ── BUTTONS ────────────────────────────────────────────────────
    void SetLoginConfirmInteractable(bool v) { if (login_ConfirmButton) login_ConfirmButton.interactable = v; }
    void SetRegisterConfirmInteractable(bool v) { if (reg_ConfirmButton) reg_ConfirmButton.interactable = v; }

    // ── LOADING ────────────────────────────────────────────────────
    void ShowLoading(bool show) { if (loadingPanel) loadingPanel.SetActive(show); }

    // ── VALIDATION ─────────────────────────────────────────────────
    bool IsValidEmail(string email) =>
        Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", RegexOptions.IgnoreCase);

    bool IsValidPhone(string phone) =>
        Regex.IsMatch(phone, @"^\+?[0-9]{7,15}$");

    bool IsValidUsername(string username) =>
        Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"); // letters, numbers, underscore only

    // ── FIREBASE ERROR ─────────────────────────────────────────────
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