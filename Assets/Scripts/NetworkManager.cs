using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : Singleton<NetworkManager>
{
    protected override void Awake()
    {
        base.Awake();  // ΩÃ±€≈Ê √ ±‚»≠

        Debug.Log("NetworkManager init");
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    #region WEB_POST
    

    public void SendLoginServer(string api, string id, string password, Action<bool> onResult)
    {
        Debug.Log(api);
        StartCoroutine(ServerLoginCall(api, id, password, onResult));
    }

    IEnumerator ServerLoginCall(string api, string id, string password, Action<bool> onResult)
    {
        LoginPostData data = new LoginPostData
        {
            id = id,
            password = password
        };
        string json = JsonUtility.ToJson(data);

        UnityWebRequest request = new UnityWebRequest(CommonDefine.WEB_BASE_URL + api, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("¿¿¥‰: " + request.downloadHandler.text);

            HandleResponse(api, request.downloadHandler.text);
           
            onResult?.Invoke(true);

        }
        else
        {
            Debug.LogError("POST Ω«∆–: " + request.error);
            onResult?.Invoke(false);
        }

    }

    #endregion



    void HandleResponse(string api, string data)
    {
        if (string.IsNullOrEmpty(api) || string.IsNullOrEmpty(data))
            return;

        switch(api)
        {
            case CommonDefine.LOGIN_URL:
                {
                    GameDataManager.Instance.loginData = JsonUtility.FromJson<LoginData>(data);
                }
                break;
            

        }
    }

    async void OnApplicationQuit()
    {
       
    }
}
