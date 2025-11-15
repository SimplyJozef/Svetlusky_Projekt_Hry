using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 5f;
    public GameObject interactionPromptGameObject; 
    
    private bool isAimingAtBoat = false;
    private Camera playerCamera;
    private QuestManager questManager;

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

        bool hitBoatThisFrame = false;

        if (Physics.Raycast(ray, out hitInfo, interactionDistance))
        {
            if (hitInfo.collider.CompareTag("InteractableBoat"))
            {
                hitBoatThisFrame = true;
            }
        }

        if (hitBoatThisFrame && !isAimingAtBoat)
        {
            interactionPromptGameObject.SetActive(true);

            isAimingAtBoat = true;
        }
        else if (!hitBoatThisFrame && isAimingAtBoat)
        {
            interactionPromptGameObject.SetActive(false);
            isAimingAtBoat = false;
        }

        if (isAimingAtBoat && Input.GetKeyDown(KeyCode.E))
        {
            questManager.InteractWithBoat();
        }
    }
}