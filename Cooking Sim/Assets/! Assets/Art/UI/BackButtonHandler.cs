using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButtonHandler : MonoBehaviour
{
    public GameObject currentPanel;    // e.g., ControllerPanel
    public GameObject previousPanel;   // e.g., MainMenuPanel

    public void OnBackButtonClick()
    {
        currentPanel.SetActive(false);
        previousPanel.SetActive(true);
        Debug.Log("Returned to main menu.");
    }
}