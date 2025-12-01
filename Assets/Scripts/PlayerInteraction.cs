using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 5f;
    public GameObject interactionPromptGameObject;
    public TextMeshProUGUI interactionPromptText; // Nová premenná pre text
    
    [SerializeField] private Camera _playerCamera;
    
    private bool isAimingAtInteractable = false;
    private QuestManager questManager;
    private GameObject currentInteractableObject = null; // Zapamätáme si, s čím interagujeme

    void Start()
    {
        questManager = FindObjectOfType<QuestManager>();
        interactionPromptGameObject.SetActive(false);
    }

    void Update()
    {
        Debug.Log("Tick");
        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
        RaycastHit hitInfo;

        bool hitInteractableThisFrame = false;

        if (Physics.Raycast(ray, out hitInfo, interactionDistance))
        {
            // Kontrolujeme, či sme trafili loď ALEBO pádlo
            if (hitInfo.collider.CompareTag("InteractableBoat") || hitInfo.collider.CompareTag("InteractablePaddle"))
            {
                hitInteractableThisFrame = true;
                currentInteractableObject = hitInfo.collider.gameObject; // Uložíme si objekt
                UpdateInteractionPrompt(currentInteractableObject); // Aktualizujeme text
            }
        }

        if (hitInteractableThisFrame && !isAimingAtInteractable)
        {
            interactionPromptGameObject.SetActive(true);
            isAimingAtInteractable = true;
        }
        else if (!hitInteractableThisFrame && isAimingAtInteractable)
        {
            interactionPromptGameObject.SetActive(false);
            isAimingAtInteractable = false;
            currentInteractableObject = null; // Zabudneme na objekt
        }

        if (isAimingAtInteractable && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Interact");
            var interactableName = currentInteractableObject is null ? "UNKNOWN" : currentInteractableObject.name;
            LogManager.Instance.SendLog($"[Interact]{interactableName}");
            // Informujeme QuestManager, s ktorým objektom sme interagovali
            questManager.HandleInteraction(currentInteractableObject);
        }
    }

    // Nová funkcia na zmenu textu podľa toho, na čo sa pozeráme
    void UpdateInteractionPrompt(GameObject target)
    {
        if (target.CompareTag("InteractableBoat"))
        {
            interactionPromptText.text = questManager.GetBoatInteractionText();
        }
        else if (target.CompareTag("InteractablePaddle"))
        {
            interactionPromptText.text = "Zobrať pádlo (Stlač E)";
        }
    }
}