using System.Collections;
using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    [Header("UI Prvky")]
    public TextMeshProUGUI questTextUI;

    [Header("Modely Lode")]
    public GameObject boatModel_Overturned; // lod_0 (prevrátená)
    public GameObject boatModel_NoPaddles;  // lod_1 (normálna, bez pádiel)
    public GameObject boatModel_WithPaddles; // lod_2 (normálna, s pádlami)

    private enum QuestState { FindBoat, FindPaddles, EscapeReady }
    private QuestState currentState = QuestState.FindBoat;
    private int paddlesCollected = 0;

    void Start()
    {    
        
        // Na začiatku je viditeľná len prevrátená loď
        boatModel_Overturned.SetActive(true);
        boatModel_NoPaddles.SetActive(false);
        boatModel_WithPaddles.SetActive(false);
        
        StartCoroutine(DisplayMessage("Hľadaj prevrátenú loď pri rieke...", 5f));
        
    }

    // Centrálna funkcia, ktorú volá PlayerInteraction
    public void HandleInteraction(GameObject interactedObject)
    {
        if (interactedObject.CompareTag("InteractableBoat"))
        {
            HandleBoatInteraction();
        }
        else if (interactedObject.CompareTag("InteractablePaddle"))
        {
            HandlePaddleInteraction(interactedObject);
        }
    }
    
    // Funkcia na získanie správneho textu pre loď
    public string GetBoatInteractionText()
    {
        switch (currentState)
        {
            case QuestState.FindBoat:
                return "Obráť loď (Stlač E)";
            case QuestState.FindPaddles:
                if (paddlesCollected < 2)
                    return "Potrebuješ nájsť pádla...";
                else
                    return "Umiestni pádla (Stlač E)";
            case QuestState.EscapeReady:
                return "Utiecť (Stlač E)";
            default:
                return "";
        }
    }

    private void HandleBoatInteraction()
    {
        switch (currentState)
        {
            case QuestState.FindBoat:
                TurnBoatOver();
                break;
            case QuestState.FindPaddles:
                // Umiestnime pádla len ak ich máme dosť
                if (paddlesCollected >= 2)
                {
                    PlacePaddles();
                }
                else
                {
                    StartCoroutine(DisplayMessage($"Chýba ti ešte {2 - paddlesCollected} pádlo/pádla.", 3f));
                }
                break;
            case QuestState.EscapeReady:
                Escape();
                break;
        }
    }

    private void HandlePaddleInteraction(GameObject paddleObject)
    {
        paddlesCollected++;
        Destroy(paddleObject);
        StartCoroutine(DisplayMessage($"Našiel si pádlo! ({paddlesCollected}/2)", 3f));

        if (paddlesCollected >= 2)
        {
            StartCoroutine(DisplayMessage("Máš obe pádla! Vráť sa k lodi.", 4f));
        }
    }

    private void TurnBoatOver()
    {
        boatModel_Overturned.SetActive(false);
        boatModel_NoPaddles.SetActive(true);
        currentState = QuestState.FindPaddles;
        StartCoroutine(DisplayMessage("Nájdi dve pádla, aby si mohol utiecť.", 5f));
    }

    private void PlacePaddles()
    {
        // Jednoducho vymeníme model lode bez pádiel za model s pádlami
        boatModel_NoPaddles.SetActive(false);
        boatModel_WithPaddles.SetActive(true);
        
        StartCoroutine(DisplayMessage("Pádla sú na mieste!", 3f));
        currentState = QuestState.EscapeReady;
    }

    private void Escape()
    {
        Debug.Log("HRA SKONČILA! Hráč utiekol.");
        // Tu príde tvoja logika konca hry
        // napr. SceneManager.LoadScene("MainMenu");
        // Application.Quit();
    }

    private IEnumerator DisplayMessage(string message, float duration)
    {
        questTextUI.enabled = true;
        questTextUI.text = message;
        yield return new WaitForSeconds(duration);
        questTextUI.enabled = false;
    }
}