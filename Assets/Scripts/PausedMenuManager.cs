using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PausedMenuManager : MonoBehaviour
{
    public Transform head;
    public float spawnDistance = 2;
    public GameObject menu;
    public GameObject SettingsUI;
    public InputActionProperty showButton;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        bool isSettingsMenuActive = SettingsUI.activeSelf;
        bool isMenuActive = menu.activeSelf;

        if (showButton.action.WasPressedThisFrame() && isSettingsMenuActive == false)
        {
            menu.SetActive(!menu.activeSelf);

            menu.transform.position = head.position + new Vector3(head.forward.x, head.forward.y, head.forward.z).normalized * spawnDistance;
        }

        if (isMenuActive == true)
        {
            menu.transform.LookAt(new Vector3(head.position.x, menu.transform.position.y, head.position.z));
            menu.transform.forward *= -1;
        }
        else if(isSettingsMenuActive == true)
        {
            SettingsUI.transform.LookAt(new Vector3(head.position.x, menu.transform.position.y, head.position.z));
            SettingsUI.transform.forward *= -1;
        }
        

    }

    public void Switch_To_Settings()
    {
        menu.SetActive(!menu.activeSelf);

        SettingsUI.SetActive(!SettingsUI.activeSelf);
        SettingsUI.transform.position = head.position + new Vector3(head.forward.x, head.forward.y, head.forward.z).normalized * (2 * spawnDistance);
        SettingsUI.transform.LookAt(new Vector3(head.position.x, menu.transform.position.y, head.position.z));
        SettingsUI.transform.forward *= -1;
    }

    public void Switch_To_Pause()
    {
        SettingsUI.SetActive(!SettingsUI.activeSelf);

        menu.SetActive(!menu.activeSelf);
        menu.transform.position = head.position + new Vector3(head.forward.x, head.forward.y, head.forward.z).normalized * spawnDistance;
        menu.transform.LookAt(new Vector3(head.position.x, menu.transform.position.y, head.position.z));
        menu.transform.forward *= -1;
    }

    public void QuitBtn()
    {
        SceneManager.LoadScene("Main Menu");
    }
}