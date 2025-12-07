using System.Collections;
using System.Collections.Generic; // Potrebné pre List
using UnityEngine;
using UnityEngine.UI; // Potrebné pre Image
using TMPro;

public class QuestManager : MonoBehaviour
{
    [Header("UI Prvky")]
    public TextMeshProUGUI questTextUI;

    [Header("Kompas Nastavenia")]
    public Transform playerTransform;      // Sem pretiahni hráča (FirstPersonController alebo kameru)
    public GameObject arrowPrefab;         // Sem pretiahni Prefab šípky, čo si vytvoril
    public RectTransform arrowContainer;   // Sem pretiahni ten prázdny Panel na vrchu obrazovky

    [Header("Modely Lode")]
    public GameObject boatModel_Overturned; 
    public GameObject boatModel_NoPaddles;  
    public GameObject boatModel_WithPaddles; 

    private enum QuestState { FindBoat, FindPaddles, EscapeReady }
    private QuestState currentState = QuestState.FindBoat;
    private int paddlesCollected = 0;

    // Zoznam na sledovanie aktívnych šípok a pádiel
    private List<PaddleTracker> activeTrackers = new List<PaddleTracker>();

    // Pomocná trieda na spárovanie pádla a jeho šípky
    private class PaddleTracker
    {
        public Transform paddleTransform;
        public RectTransform arrowRect;
    }

    void Start()
    {    
        boatModel_Overturned.SetActive(true);
        boatModel_NoPaddles.SetActive(false);
        boatModel_WithPaddles.SetActive(false);
        
        StartCoroutine(DisplayMessage("Hľadaj prevrátenú loď pri rieke...", 5f));
    }

    // Update používame na otáčanie šípok
    void Update()
    {
        if (currentState == QuestState.FindPaddles)
        {
            UpdateCompass();
        }
    }

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
    
    public string GetBoatInteractionText()
    {
        switch (currentState)
        {
            case QuestState.FindBoat: return "Obráť loď (Stlač E)";
            case QuestState.FindPaddles:
                return (paddlesCollected < 2) ? "Potrebuješ nájsť pádla..." : "Umiestni pádla (Stlač E)";
            case QuestState.EscapeReady: return "Utiecť (Stlač E)";
            default: return "";
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
                if (paddlesCollected >= 2) PlacePaddles();
                else StartCoroutine(DisplayMessage($"Chýba ti ešte {2 - paddlesCollected} pádlo/pádla.", 3f));
                break;
            case QuestState.EscapeReady:
                Escape();
                break;
        }
    }

    private void HandlePaddleInteraction(GameObject paddleObject)
    {
        paddlesCollected++;
        
        // Nájdeme tracker pre toto konkrétne pádlo a zničíme šípku
        PaddleTracker trackerToRemove = activeTrackers.Find(x => x.paddleTransform == paddleObject.transform);
        if (trackerToRemove != null)
        {
            if(trackerToRemove.arrowRect != null) Destroy(trackerToRemove.arrowRect.gameObject);
            activeTrackers.Remove(trackerToRemove);
        }

        Destroy(paddleObject); // Zničí pádlo v svete
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
        StartCoroutine(DisplayMessage("Nájdi dve pádla, sleduj kompas!", 5f));

        // --- SPUSTENIE KOMPASU ---
        InitializeCompass();
    }

    private void InitializeCompass()
    {
        // Nájde všetky pádla v scéne
        GameObject[] paddles = GameObject.FindGameObjectsWithTag("InteractablePaddle");

        foreach (GameObject paddle in paddles)
        {
            // Vytvorí UI šípku
            GameObject newArrow = Instantiate(arrowPrefab, arrowContainer);
            
            // Pridá do zoznamu na sledovanie
            PaddleTracker newTracker = new PaddleTracker();
            newTracker.paddleTransform = paddle.transform;
            newTracker.arrowRect = newArrow.GetComponent<RectTransform>();
            
            activeTrackers.Add(newTracker);
        }
    }

    private void UpdateCompass()
    {
        // Krok 1: Nájdeme, ktoré pádlo je najbližšie
        PaddleTracker closestTracker = null;
        float minDistance = float.MaxValue;

        // Prejdeme zoznam a nájdeme min vzdialenosť
        foreach (var tracker in activeTrackers)
        {
            if (tracker.paddleTransform != null)
            {
                float dist = Vector3.Distance(playerTransform.position, tracker.paddleTransform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestTracker = tracker;
                }
            }
        }

        // Krok 2: Aktualizujeme šípky
        for (int i = activeTrackers.Count - 1; i >= 0; i--)
        {
            PaddleTracker tracker = activeTrackers[i];

            // Safety check (ak bolo pádlo zničené/zobrané)
            if (tracker.paddleTransform == null || tracker.arrowRect == null) 
            {
                if(tracker.arrowRect != null) Destroy(tracker.arrowRect.gameObject);
                activeTrackers.RemoveAt(i);
                continue;
            }

            // Ak je toto tracker pre najbližšie pádlo -> ZAPNI HO a OTOČ
            if (tracker == closestTracker)
            {
                tracker.arrowRect.gameObject.SetActive(true); // Zviditeľniť

                // Výpočet uhla (rovnako ako predtým)
                Vector3 directionToPaddle = tracker.paddleTransform.position - playerTransform.position;
                directionToPaddle.y = 0;
                float angle = Vector3.SignedAngle(playerTransform.forward, directionToPaddle, Vector3.up);
                tracker.arrowRect.localEulerAngles = new Vector3(0, 0, -angle);
            }
            else
            {
                // Ak nie je najbližšie -> SKRY HO
                tracker.arrowRect.gameObject.SetActive(false);
            }
        }
    }

    private void PlacePaddles()
    {
        boatModel_NoPaddles.SetActive(false);
        boatModel_WithPaddles.SetActive(true);
        StartCoroutine(DisplayMessage("Pádla sú na mieste!", 3f));
        currentState = QuestState.EscapeReady;
    }

    private void Escape()
    {
        Debug.Log("HRA SKONČILA! Hráč utiekol.");
    }

    private IEnumerator DisplayMessage(string message, float duration)
    {
        questTextUI.enabled = true;
        questTextUI.text = message;
        yield return new WaitForSeconds(duration);
        questTextUI.enabled = false;
    }
}