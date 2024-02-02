using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    public string currentUser;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
