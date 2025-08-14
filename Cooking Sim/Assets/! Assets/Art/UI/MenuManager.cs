using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject recipeMenuPanel;
    public GameObject controllerPanel;

    [Header("Start Cooking Button")]
    public GameObject startCookingButton;

    [Header("Recipe Buttons")]
    public Button jollofButton;
    public Button fufuButton;

    [Header("TMP Text References")]
    public TextMeshProUGUI jollofText;
    public TextMeshProUGUI fufuText;
    public TextMeshProUGUI startCookingText;

    private GameObject previousMenu;
    private string selectedRecipe = "";

    // ------------------------------
    // UI Navigation
    // ------------------------------

    public void ShowMainMenu()
    {
        recipeMenuPanel.SetActive(false);
        controllerPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        previousMenu = mainMenuPanel;
    }

    public void ShowRecipeMenu()
    {
        mainMenuPanel.SetActive(false);
        controllerPanel.SetActive(false);
        recipeMenuPanel.SetActive(true);
        previousMenu = recipeMenuPanel;
    }

    public void ShowControllers()
    {
        if (mainMenuPanel.activeSelf)
        {
            previousMenu = mainMenuPanel;
            mainMenuPanel.SetActive(false);
        }
        else if (recipeMenuPanel.activeSelf)
        {
            previousMenu = recipeMenuPanel;
            recipeMenuPanel.SetActive(false);
        }

        controllerPanel.SetActive(true);
    }

    public void BackToPreviousMenu()
    {
        controllerPanel.SetActive(false);

        if (previousMenu != null)
        {
            previousMenu.SetActive(true);
        }
        else
        {
            mainMenuPanel.SetActive(true);  // Fallback
        }
    }

    // ------------------------------
    // Select Recipe
    // ------------------------------

    public void SelectRecipe(string recipe)
    {
        selectedRecipe = recipe;
        Debug.Log("Selected recipe: " + recipe);

        // Reset all recipe buttons and text colors
        jollofButton.GetComponent<Image>().color = Color.white;
        fufuButton.GetComponent<Image>().color = Color.white;
        jollofText.color = Color.white;
        fufuText.color = Color.white;

        // Highlight selected recipe
        if (recipe == "Jollof")
        {
            jollofButton.GetComponent<Image>().color = Color.yellow;
            jollofText.color = Color.yellow;
        }
        else if (recipe == "Fufu")
        {
            fufuButton.GetComponent<Image>().color = Color.yellow;
            fufuText.color = Color.yellow;
        }

        // Highlight Start Cooking button and text
        Image btnImage = startCookingButton.GetComponent<Image>();
        if (btnImage != null)
        {
            btnImage.color = Color.yellow;
        }

        if (startCookingText != null)
        {
            startCookingText.color = Color.yellow;
        }
    }

    // ------------------------------
    // Start Cooking (Redirect to Loading Scene)
    // ------------------------------

    public void StartCooking()
    {
        if (string.IsNullOrEmpty(selectedRecipe))
        {
            Debug.LogWarning("Please select a recipe first!");
            return;
        }

        string targetScene = "";

        if (selectedRecipe == "Jollof")
            targetScene = "MechanicsJollofFinal";
        else if (selectedRecipe == "Fufu")
            targetScene = "MechanicsFufuFinal";

        if (!string.IsNullOrEmpty(targetScene))
        {
            PlayerPrefs.SetString("SceneToLoad", targetScene);
            SceneManager.LoadScene("LoadingScene");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}