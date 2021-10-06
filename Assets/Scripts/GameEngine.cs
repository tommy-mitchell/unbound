using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using CommonLibrary;

public class GameEngine : MonoBehaviour
{
    public static GameEngine e;
    
    [SerializeField]
    private Transform _player;

    public event Action Engine_onStart;
    public event Action Engine_onGameOver;

    public void OnCollapseTrigger()// => Engine_onGameOver?.Invoke();
    {
        SceneManager.LoadScene("Title");
    }

    public void OnExit()
    {
        SceneManager.LoadScene("End");
    }

    private IEnumerator Start()
    {
        e = this;

        Engine_onGameOver += () => Debug.Log("game over");
        Engine_onStart    += () => Debug.Log("started");

        Debug.Log("start method");
        yield return StartCoroutine(CommonMethods.WaitForSecondsCoroutine(2f));
        Engine_onStart?.Invoke();
    }
}
