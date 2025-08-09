using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : Singleton<NetworkManager>
{
    protected override void Awake()
    {
        base.Awake();  // 싱글톤 초기화

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
            Debug.Log("응답: " + request.downloadHandler.text);

            HandleResponse(api, request.downloadHandler.text);
           
            onResult?.Invoke(true);

        }
        else
        {
            Debug.LogError("POST 실패: " + request.error);
            onResult?.Invoke(false);
        }

    }



    public void SendServerPost(string api, object packet, Action<bool> onResult)
    {
        Debug.Log(api);
        StartCoroutine(ServerCallPost(api, packet, onResult));
    }

    IEnumerator ServerCallPost(string api, object packet, Action<bool> onResult)
    {
        string json = JsonUtility.ToJson(packet);

        UnityWebRequest request = new UnityWebRequest(CommonDefine.WEB_BASE_URL + api, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("authorization", GameDataManager.Instance.loginData.sessionId);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("응답: " + request.downloadHandler.text);

            HandleResponse(api, request.downloadHandler.text);
            //GameDataManager.Instance.token = res.token;
            onResult?.Invoke(true);
        }
        else
        {
            Debug.LogError("POST 실패: " + request.error);
            onResult?.Invoke(false);
        }

    }

    #endregion


    #region WEB_GET

    public void SendServerGet(string api, List<ServerPacket> packetList, Action<bool> onResult)
    {
        Debug.Log(api);
        StartCoroutine(ServerCallGet(api, packetList, onResult));
    }

    IEnumerator ServerCallGet(string api, List<ServerPacket> packetList, Action<bool> onResult)
    {
        string packetStr = "";
        if (packetList != null)
        {
            for (int i = 0; i < packetList.Count; ++i)
            {
                if (packetStr.Length > 0)
                    packetStr += "&";

                ServerPacket packet = packetList[i];
                packetStr += packet.packetType + "=" + packet.packetValue;
            }
        }

        string url = CommonDefine.WEB_BASE_URL + api;
        if (packetStr.Length > 0)
        {
            url += "?" + packetStr;
        }

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("authorization", GameDataManager.Instance.loginData.sessionId);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("응답: " + request.downloadHandler.text);

            HandleResponse(api, request.downloadHandler.text);
            //GameDataManager.Instance.token = res.token;
            onResult?.Invoke(true);
        }
        else
        {
            Debug.LogError("GET 실패: " + request.error);
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
            case CommonDefine.GET_MY_POKEMON_URL:
                {
                    GameDataManager.Instance.myPokemonList = JsonHelper.FromJson<MyPokemon>(data);
                    GameDataManager.Instance.myPokemonIds = new HashSet<int>(GameDataManager.Instance.myPokemonList.Select(p => p.poketmonId));
                }
                break;
            case CommonDefine.GET_MY_WALLET_URL:
                {
                    WalletData wallet = JsonUtility.FromJson<WalletData>(data);
                    GameDataManager.Instance.walletBalance = double.Parse(wallet.balance);
                }
                break;
            case CommonDefine.SHOP_LIST_URL:
                {
                    GameDataManager.Instance.pokemonShopList = JsonHelper.FromJson<PokemonShop>(data);
                }
                break;
           

        }
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{ \"array\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }

    void OnApplicationQuit()
    {
       
    }
}
