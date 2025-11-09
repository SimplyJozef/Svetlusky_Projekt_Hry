using UnityEngine;

public class LampManager : MonoBehaviour
{

    [Header("Nastavení světel")]
    [SerializeField] [Range(0f, 20f)] private float intensity = 1.0f; 
    [SerializeField] [Range(0f, 50f)] private float range = 10.0f;   
    [SerializeField] private Color lightColor = Color.yellow;     

    [Header("Seznam lamp k ovládání")]
    [SerializeField] private GameObject[] allLamps;

    private void OnValidate()
    {
        ApplySettingsToAllLamps();
    }

   
    void ApplySettingsToAllLamps()
    {
        
        if (allLamps != null)
        {
          
            foreach (GameObject lamp in allLamps)
            {
                
                if (lamp != null)
                {
                   
                    Light pointLight = lamp.GetComponentInChildren<Light>();

             
                    if (pointLight != null)
                    {
                        pointLight.intensity = intensity;
                        pointLight.range = range;
                        pointLight.color = lightColor;
                    }
                }
            }
        }
    }
}