using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public void OnStart() => SceneManager.LoadScene("Level");

    public void OnExit() => Application.Quit();

    public void OnMenu() => SceneManager.LoadScene("Root");
}
