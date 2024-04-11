using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Playernameforoffice : MonoBehaviour

{
    private GameObject UserMan;
    private string user;
    public TextMeshProUGUI TextMesh;

    // Start is called before the first frame update
    void Start()
    {
        UserMan = GameObject.Find("UserManager");
        UserManager Manager = UserMan.GetComponent<UserManager>();
        user = Manager.currentUser;
        TextMesh.text = user;
    }
}
