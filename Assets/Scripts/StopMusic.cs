using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopMusic : MonoBehaviour
{
    void Start()
    {
        GameObject.Find("Music")?.SetActive(false);
    }
}
