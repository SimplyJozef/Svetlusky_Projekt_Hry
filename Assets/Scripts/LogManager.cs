using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LogManager : MonoBehaviour
{
    public static LogManager Instance { get; private set; }
    
    [SerializeField] private bool _bRecordLogs = true;
    
    private string _userId;

    private Coroutine _postCoroutine;

    private string _logs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        _userId = PlayerPrefs.GetString("UserName");
        SendLog("[LogBegin]");
        _postCoroutine = StartCoroutine(PeriodicLogSendCoroutine());
    }
    
    public void LogMovement(Vector3 position, Vector3 fwd)
    {
        _logs += $"|Move;{position.x};{position.y};{position.z};{fwd.x};{fwd.y};{fwd.z};{Time.timeSinceLevelLoad}";
    }
    
    private IEnumerator PeriodicLogSendCoroutine()
    {
        while (_bRecordLogs)
        {
            yield return new WaitForSeconds(10f);
            SendLog(_logs);
            _logs = "";
        }
    }
    
    public void SendLog(string data)
    {
        var apiUrl = "https://game-log-server-w2i98.ondigitalocean.app/recordGameLog";
        var log = new LogEntry
        {
            User = _userId,
            Data = data
        };
        var json = JsonUtility.ToJson(log);
        StartCoroutine(SendPostRequest(apiUrl, json));
    }
    
    private IEnumerator SendPostRequest(string apiUrl, string json)
    {
        var req = new UnityWebRequest(apiUrl, "POST");
        var bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + req.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error: " + req.error);
        }
    }

}
