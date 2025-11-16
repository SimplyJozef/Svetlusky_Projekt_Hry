using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 5f;
    public GameObject interactionPromptGameObject;
    public TextMeshProUGUI interactionPromptText; // Nová premenná pre text

    private bool isAimingAtInteractable = false;
    private Camera playerCamera;
    private QuestManager questManager;
    private GameObject currentInteractableObject = null; // Zapamätáme si, s čím interagujeme

    void Start()
    {
        playerCamera = GetComponent<Camera>();
        questManager = FindObjectOfType<QuestManager>();
        interactionPromptGameObject.SetActive(false);
    }

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
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