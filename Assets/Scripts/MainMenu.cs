using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
   [SerializeField]
   private TMP_InputField _nameInputField;
   public void PlayGame()
   {
      PlayerPrefs.SetString("UserName", _nameInputField.text);
      SceneManager.LoadScene("Scenes/Svetlusky_scena_dark");
   }

   public void QuitGame()
   {
      Application.Quit();
   }
}
