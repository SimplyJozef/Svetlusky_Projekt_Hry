using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public HealthUI healthUI;

    void Start()
    {
        currentHealth = maxHealth;
        healthUI.SetMaxHealth(maxHealth);
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        LogManager.Instance.SendLog($"[PlayerTookDamage]{dmg}");
        LogManager.Instance.SendLog($"[PlayerNewHealth]{currentHealth}");

        healthUI.SetHealth(currentHealth);

        Debug.Log("HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("PLAYER DIED");
        LogManager.Instance.SendLog("[Death]");
        SceneManager.LoadScene("Main_Menu");
    }
}
