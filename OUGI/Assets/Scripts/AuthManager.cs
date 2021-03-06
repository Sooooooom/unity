﻿using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
    // value

    // fb가 준비되어있는지 
    public bool IsFirebaseReady { get; private set; }
    public bool IsSignInOnProgress { get; private set; }

    public InputField emailField;
    public InputField passwordField;
    public Button signButton;

    // 아래 변수들을 전부 static으로 선언한 이유는 외부에서 app, auth, user를 즉시 접근가능하도록 하기 위함
    // 원래는 singleton으로 만드는게 맞지만 간단히 구현한 상태

    // fb app을 관리
    public static FirebaseApp firebaseApp;
    // app 중에서도 auth를 집중적으로 관리
    public static FirebaseAuth firebaseAuth;

    public static FirebaseUser User;

    private void Awake()
    {
        // 이제 fb를 사용가능하니 fb app 할당
        firebaseApp = FirebaseApp.DefaultInstance;
        firebaseAuth = FirebaseAuth.DefaultInstance;
    }

    private void Start()
    {
    }

    public void SignUp() // sign up 회원가입
    {
        if (!IsFirebaseReady)
        {
            return;
        }

        signButton.interactable = false;

        firebaseAuth.CreateUserWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWithOnMainThread(task =>
        {
            signButton.interactable = true;

            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
            }
            else
            {
                signButton.interactable = true;
                // Firebase user has been created.
                FirebaseUser newUser = task.Result;
                Debug.LogFormat("Firebase user created successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
            }
        });
    }

    public void LogIn()
    {
        // fb 비활성화 / 이미 로그인시도가 진행적인 경우 / 로그인을 성공해서 이미 유저가 할당된 경우
        if (!IsFirebaseReady || IsSignInOnProgress || User != null)
        {
            Debug.Log("return");
            return;
        }

        IsSignInOnProgress = true;
        signButton.interactable = false;

        firebaseAuth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWithOnMainThread(task =>
        {
            // 로그로 상태를 알린다
            Debug.Log($"Sign in status : {task.Status}");

            IsSignInOnProgress = true;
            signButton.interactable = true;

            if (task.IsFaulted) // task를 실패했을 경우
            {
                Debug.LogError(task.Exception); // 어떤 예외가 발생했는지
            }
            else if (task.IsCanceled) // 실패한 건 아닌데 cancel된 경우
            {
                Debug.LogError("Signin Canceled");
            }
            else // 처리가 정상적으로 된 경우
            {
                // email과 password에 대응되는 유저정보가 fb로부터 넘어온다 
                User = task.Result;
                Debug.Log(User.Email); // 해당유저의 email을 로그로 찍어준다
                UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
            }
        }
        );
    }
}
