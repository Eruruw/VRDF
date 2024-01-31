using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class Create_User_Check : MonoBehaviour
{
    public TMP_InputField inputField;

    // Update is called once per frame
    void Update()
    {
        // Check if there is text inside the input field
        if (inputField != null && !string.IsNullOrEmpty(inputField.text))
        {
            // Validate the input using a regular expression
            if (IsInputValid(inputField.text))
            {
                Debug.Log("Input is valid: " + inputField.text);
            }
            else
            {
                Debug.Log("Invalid input! Only alphabetical characters are allowed.");
            }
        }
        else
        {
            Debug.Log("Input field is empty");
        }
    }

    bool IsInputValid(string input)
    {
        // Use a regular expression to check if the input contains only alphabetical characters
        // Adjust the regular expression pattern based on your validation criteria
        return Regex.IsMatch(input, "^[a-zA-Z]+$");
    }
}
