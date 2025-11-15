using System.Collections;
using UnityEngine;
using TMPro; // Potrebné pre TextMeshPro

public class QuestManager : MonoBehaviour
{
    // === Vizuálne prvky, ktoré pretiahneme v editore ===
    [Header("UI Prvky")]
    public TextMeshProUGUI questTextUI;

    [Header("Objekty Lode")]
    public GameObject overturnedBoat; // Sem dáme lod_0
    public GameObject normalBoat;     // Sem dáme lod_1

    // === Logika úlohy ===
    private enum QuestState { HladajLod, HladajPadla }
    private QuestState currentState = QuestState.HladajLod;

    void Start()
    {
        // Na začiatku hry zobrazíme prvú úlohu
        StartCoroutine(DisplayMessage("Hľadaj prevrátenú loď pri rieke...", 5f));
    }

    // Funkcia, ktorú volá náš PlayerInteraction skript
    public void InteractWithBoat()
    {
        // Ak sme vo fáze hľadania lode
        if (currentState == QuestState.HladajLod)
        {
            TurnBoatOver();
        }
    }

    private void TurnBoatOver()
    {
        // Vymeníme modely
        overturnedBoat.SetActive(false);
        normalBoat.SetActive(true);

        // Zmeníme stav úlohy
        currentState = QuestState.HladajPadla;

        // Zobrazíme novú úlohu
        StartCoroutine(DisplayMessage("Nájdi dve pádla a vráť sa k lodi, aby si mohol utiecť.", 5f));
    }

    // Funkcia (coroutine) na zobrazenie textu na určitý čas
    private IEnumerator DisplayMessage(string message, float duration)
    {
        questTextUI.text = message;
        questTextUI.enabled = true; // Zobrazíme text

        yield return new WaitForSeconds(duration); // Počkáme určený čas

        questTextUI.enabled = false; // Skryjeme text
    }
}