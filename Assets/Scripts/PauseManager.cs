using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    // Premenná, do ktorej si v editore pretiahneme náš panel s menu
    [SerializeField] private GameObject pauseMenuPanel; 
    
    // Premenná, ktorá sleduje, či je hra pauznutá
    private bool isPaused = false;

    void Start()
    {
        // Na začiatku hry sa uistíme, že menu je vypnuté
        pauseMenuPanel.SetActive(false);
    }

    void Update()
    {
        // Každý frame kontrolujeme, či hráč stlačil klávesu Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                // Ak je hra pauznutá, pokračujeme
                Resume();
            }
            else
            {
                // Ak hra beží, pauzneme ju
                Pause();
            }
        }
    }

    // Funkcia na pokračovanie v hre
    public void Resume()
    {
        pauseMenuPanel.SetActive(false); // Vypneme panel s menu
        Time.timeScale = 1f;             // Obnovíme plynutie času v hre
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked; // Zamkneme kurzor (dôležité pre FPS hry)
        Cursor.visible = false;                   // Skryjeme kurzor
    }

    // Funkcia na pauznutie hry
    void Pause()
    {
        pauseMenuPanel.SetActive(true);  // Zapneme panel s menu
        Time.timeScale = 0f;             // Zastavíme čas v hre
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;   // Odomkneme kurzor, aby sa dalo klikať na tlačidlá
        Cursor.visible = true;                    // Zobrazíme kurzor
    }

    // Funkcia na návrat do hlavného menu
    public void LoadMainMenu()
    {
        // DÔLEŽITÉ: Pred načítaním novej scény musíme obnoviť čas!
        Time.timeScale = 1f; 
        SceneManager.LoadScene("Main_Menu"); // Nahraď "Main_Menu" presným názvom tvojej scény s menu
    }
}