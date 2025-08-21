using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Transform canvas;

    GameObject lobbyObj = null;
    GameObject shopObj = null;
    GameObject roomObj = null;
    GameObject loadingCircleObj = null;

    List<GameObject> shopItemsObjList = new List<GameObject>();

    static Queue<Action> mainThreadActions = new Queue<Action>();

    void Awake()
    {
        Debug.Log("GameManager init");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
        EasterEggInit();
    }

    async void Init()
    {
        lobbyObj = null;
        shopObj = null;
        roomObj = null;

        if (canvas == null)
            canvas = GameObject.Find("Canvas").transform;

        GameObject prefab = Resources.Load<GameObject>("prefabs/GameLobby");
        lobbyObj = Instantiate(prefab, canvas);

        lobbyObj.transform.Find("MakeRoomBtn").GetComponent<Button>().onClick.AddListener(OnClickMakeRoom);
        lobbyObj.transform.Find("RoomListBtn").GetComponent<Button>().onClick.AddListener(OnClickRoomList);
        lobbyObj.transform.Find("ShopBtn").GetComponent<Button>().onClick.AddListener(OnClickEnterShop);
        lobbyObj.transform.Find("InvenBtn").GetComponent<Button>().onClick.AddListener(OnClickEnterInventory);
        lobbyObj.transform.Find("Wallet/LinkWalletBtn").GetComponent<Button>().onClick.AddListener(OnClickLinkWalletPage);
        lobbyObj.transform.Find("Wallet/UpdateWalletBtn").GetComponent<Button>().onClick.AddListener(OnClickUpdateWallet);
        lobbyObj.transform.Find("LogOutBtn").GetComponent<Button>().onClick.AddListener(OnClickLogOut);

        UpdateWallet(true);

        await NetworkManager.Instance.ConnectSocket(OnRoomUpdate);

    }

    // Update is called once per frame
    void Update()
    {
        EasterEgg();
        CheckMainThreadActions();
    }

    void OnClickLogOut()
    {
        LoadScene(CommonDefine.LOGIN_SCENE);
    }

    void LoadScene(string nextSceneName)
    {
        GameDataManager.Instance.nextScene = nextSceneName;
        SceneManager.LoadScene(CommonDefine.LOADING_SCENE);
    }

    void OnClickUpdateWallet()
    {
        NetworkManager.Instance.SendServerGet(CommonDefine.GET_MY_WALLET_URL, null, UpdateWallet);
    }

    void UpdateWallet(bool result)
    {
        if (!result)
        {
            Debug.Log("내 지갑 로드 실패");
        }

        if(GameDataManager.Instance.walletBalance < 0)
        {
            lobbyObj.transform.Find("Wallet/balance").GetComponent<TMP_Text>().text = "지갑 연동 안됨.";
        }
        else
        {
            lobbyObj.transform.Find("Wallet/balance").GetComponent<TMP_Text>().text = "잔액 : " + GameDataManager.Instance.walletBalance.ToString("F2");
        }

    }

    void OnClickLinkWalletPage()
    {
        GameObject prefab = Resources.Load<GameObject>("prefabs/LinkWallet");
        GameObject obj = Instantiate(prefab, canvas);

        obj.transform.Find("CloseBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(obj));
        obj.transform.Find("LinkBtn").GetComponent<Button>().onClick.AddListener(() => OnClickLinkWallet(obj));
        obj.transform.Find("LinkBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(obj));
    }

    void OnClickLinkWallet(GameObject obj)
    {
        string privateKey = obj.transform.Find("PrivateKey").GetComponent<TMP_InputField>().text;

        LinkWalletPostData data = new LinkWalletPostData
        {
            privateKey = privateKey,
        };

        NetworkManager.Instance.SendServerPost(CommonDefine.LINK_WALLET_URL, data, CallbackLinkWallet);
    }


    void CallbackLinkWallet(bool result)
    {
        if (result)
        {
            CreateMsgBoxOneBtn("지갑 연동 성공", OnClickUpdateWallet);
        }
        else
        {
            CreateMsgBoxOneBtn("지갑 연동 실패");
        }
    }

    void OnClickRoomList()
    {
        NetworkManager.Instance.SendServerGet(CommonDefine.ROOM_LIST_URL, null, CallbackRoomList);
    }

    void CallbackRoomList(bool result)
    {
        GameObject prefab = Resources.Load<GameObject>("prefabs/RoomList");
        GameObject obj = Instantiate(prefab, canvas);

        obj.transform.Find("closeBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(obj));
        obj.transform.Find("closeBtn").GetComponent<Button>().onClick.AddListener(() => { GameDataManager.Instance.roomList = null; });

        Sprite[] spriteFrontAll = Resources.LoadAll<Sprite>("images/pokemon-front");
        GameObject itemPrefab = Resources.Load<GameObject>("prefabs/RoomListItem");
        Transform content = obj.transform.Find("ScrollView/Viewport/Content");

        for (int i = 0; i < GameDataManager.Instance.roomList.Length; i++)
        {
            var room = GameDataManager.Instance.roomList[i];

            GameObject itemObj = Instantiate(itemPrefab, content);

            itemObj.transform.Find("Icon/IconImage").GetComponent<Image>().sprite = spriteFrontAll[room.members[0].pokemonId - 1];

            for (int k = 0; k < room.members.Count; ++k)
            {
                var member = room.members[k];
                if (room.leaderId == member.userSeq)
                {
                    itemObj.transform.Find("Title").GetComponent<TMP_Text>().text = member.userId + "의 방";
                }
            }

            itemObj.transform.Find("Level").GetComponent<TMP_Text>().text = "Level " + room.bossPokemonId.ToString();

            itemObj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => SelectPokemon_JoinRoom(room.roomId, obj));
        }

    }

    void SelectPokemon_JoinRoom(string roomId, GameObject roomListObj)
    {
        GameObject prefab = Resources.Load<GameObject>("prefabs/Inventory");
        GameObject obj = Instantiate(prefab, canvas);

        obj.transform.Find("closeBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(obj));

        obj.transform.Find("Title").GetComponent<TMP_Text>().text = "포켓몬 선택";

        Sprite[] spriteFrontAll = Resources.LoadAll<Sprite>("images/pokemon-front");
        GameObject itemPrefab = Resources.Load<GameObject>("prefabs/InventoryItem");
        Transform content = obj.transform.Find("ScrollView/Viewport/Content");

        for (int i = 0; i < GameDataManager.Instance.myPokemonList.Length; i++)
        {
            var pokemon = GameDataManager.Instance.myPokemonList[i];

            GameObject itemObj = Instantiate(itemPrefab, content);

            itemObj.transform.Find("Icon/IconImage").GetComponent<Image>().sprite = spriteFrontAll[pokemon.poketmonId - 1];

            itemObj.transform.Find("Title").GetComponent<TMP_Text>().text = pokemon.name;
            itemObj.transform.Find("Context").GetComponent<TMP_Text>().text = "hp : " + pokemon.hp.ToString();

            itemObj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => JoinRoom(roomId, pokemon.poketmonId));
            itemObj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => DestroyObject(obj));
            itemObj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => DestroyObject(roomListObj));
        }

    }


    void JoinRoom(string roomId, int pokemonId)
    {
        // todo 포켓몬 구입후 데이터 갱신
        Debug.Log("JoinRoom : " + roomId);
        NetworkManager.Instance.JoinRoom(OnRoomUpdate, roomId, pokemonId);
    }

    void OnClickEnterShop()
    {
        if(GameDataManager.Instance.pokemonShopList == null)
        {
            NetworkManager.Instance.SendServerGet(CommonDefine.SHOP_LIST_URL, null, CallbackShopList);
        }
        else
        {
            CreateShop();
        }

    }

    void CallbackShopList(bool result)
    {
        if (result)
        {
            CreateShop();
        }
        else
        {
            CreateMsgBoxOneBtn("상점 로드 실패");
        }
    }

    void CreateShop()
    {
        if(shopObj == null)
        {
            GameObject prefab = Resources.Load<GameObject>("prefabs/Shop");
            shopObj = Instantiate(prefab, canvas);
        }

        shopObj.transform.Find("closeBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(shopObj));

        Sprite[] spriteFrontAll = Resources.LoadAll<Sprite>("images/pokemon-front");
        GameObject itemPrefab = Resources.Load<GameObject>("prefabs/ShopItem");
        Transform content = shopObj.transform.Find("ScrollView/Viewport/Content");

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        shopItemsObjList.Clear();

        for (int i = 0; i < GameDataManager.Instance.pokemonShopList.Length; i++)
        {
            var shopItem = GameDataManager.Instance.pokemonShopList[i];

            bool isHave = false;
            if(GameDataManager.Instance.myPokemonIds != null && GameDataManager.Instance.myPokemonIds.Contains(shopItem.pokemon.id))
            {
                isHave = true;
            }

            GameObject itemObj = Instantiate(itemPrefab, content);

            itemObj.transform.Find("Icon/IconImage").GetComponent<Image>().sprite = spriteFrontAll[shopItem.pokemon.id - 1];

            itemObj.transform.Find("Title").GetComponent<TMP_Text>().text = shopItem.pokemon.name;
            itemObj.transform.Find("Context").GetComponent<TMP_Text>().text = "hp : " + shopItem.pokemon.hp.ToString() + " / 가격 : " + shopItem.price.ToString();

            if (isHave)
            {
                itemObj.transform.Find("Button/buyText").GetComponent<TMP_Text>().text = "보유";
            }
            else
            {
                itemObj.transform.Find("Button/buyText").GetComponent<TMP_Text>().text = "구매";
                itemObj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => PurchasePokemon(shopItem.shop_id));
            }

            shopItemsObjList.Add(itemObj);
        }

    }

    void PurchasePokemon(int idx)
    {
        Debug.Log("PurchasePokemon : " + idx);
        PurchasePostData data = new PurchasePostData
        {
            itemId = idx,
        };

        NetworkManager.Instance.SendServerPost(CommonDefine.SHOP_PURCHASE_URL, data, CallbackPurchasePokemon);
    }

    void CallbackPurchasePokemon(bool result)
    {
        if (result)
        {
            NetworkManager.Instance.SendServerGet(CommonDefine.GET_MY_POKEMON_URL, null, CallbackMyPokemonAfterPurchasePokemon);
        }
        else
        {
            CreateMsgBoxOneBtn("상점 구매 실패");
        }
    }

    void CallbackMyPokemonAfterPurchasePokemon(bool result)
    {
        if (result)
        {
            CreateMsgBoxOneBtn("구매 완료");
            UpdateshopItems();
        }
        else
        {
            CreateMsgBoxOneBtn("상점 구매후 포켓몬 로드 실패");
        }
    }

    void UpdateshopItems()
    {
        for (int i = 0; i < GameDataManager.Instance.pokemonShopList.Length; i++)
        {
            var shopItem = GameDataManager.Instance.pokemonShopList[i];

            bool isHave = false;
            if(GameDataManager.Instance.myPokemonIds != null && GameDataManager.Instance.myPokemonIds.Contains(shopItem.pokemon.id))
            {
                isHave = true;
            }

            GameObject itemObj = shopItemsObjList[i];
            itemObj.transform.Find("Button").GetComponent<Button>().onClick.RemoveAllListeners();

            if (isHave)
            {
                itemObj.transform.Find("Button/buyText").GetComponent<TMP_Text>().text = "보유";
            }
            else
            {
                itemObj.transform.Find("Button/buyText").GetComponent<TMP_Text>().text = "구매";
                itemObj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => PurchasePokemon(shopItem.shop_id));
            }
        }
    }

    void CreateLoadingCircle()
    {
        GameObject prefab = Resources.Load<GameObject>("prefabs/LoadingCircle");
        loadingCircleObj = Instantiate(prefab, canvas);
    }

    void DestroyLoadingCircle()
    {
        DestroyObject(loadingCircleObj);
    }

    void OnClickMakeRoom()
    {
        GameObject prefab = Resources.Load<GameObject>("prefabs/MakeRoom");
        GameObject obj = Instantiate(prefab, canvas);

        obj.transform.Find("Title/detail").GetComponent<TMP_Text>().text = GameDataManager.Instance.loginData.id +  "의 방";

        var dropdown = obj.transform.Find("Level/Dropdown").GetComponent<TMP_Dropdown>();
        dropdown.ClearOptions();
        List<string> list = new List<string>();
        for(int i = 0; i < 20; ++i)
        {
            list.Add("level " + (i + 1));
        }
        dropdown.AddOptions(list);
        
        obj.transform.Find("CancelBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(obj));
        obj.transform.Find("Select/SelectBtn").GetComponent<Button>().onClick.AddListener(() => SelectPokemonMakeRoom(obj));

        obj.transform.Find("Select/Context").GetComponent<TMP_Text>().text = "포켓몬을\n선택해주세요.";

    }

    void SelectPokemonMakeRoom(GameObject makeRoomObj)
    {
        GameObject prefab = Resources.Load<GameObject>("prefabs/Inventory");
        GameObject obj = Instantiate(prefab, canvas);

        obj.transform.Find("closeBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(obj));

        obj.transform.Find("Title").GetComponent<TMP_Text>().text = "포켓몬 선택";

        Sprite[] spriteFrontAll = Resources.LoadAll<Sprite>("images/pokemon-front");
        GameObject itemPrefab = Resources.Load<GameObject>("prefabs/InventoryItem");
        Transform content = obj.transform.Find("ScrollView/Viewport/Content");

        for (int i = 0; i < GameDataManager.Instance.myPokemonList.Length; i++)
        {
            var pokemon = GameDataManager.Instance.myPokemonList[i];

            GameObject itemObj = Instantiate(itemPrefab, content);

            itemObj.transform.Find("Icon/IconImage").GetComponent<Image>().sprite = spriteFrontAll[pokemon.poketmonId - 1];

            itemObj.transform.Find("Title").GetComponent<TMP_Text>().text = pokemon.name;
            itemObj.transform.Find("Context").GetComponent<TMP_Text>().text = "hp : " + pokemon.hp.ToString();

            itemObj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => UsePokemon_MakeRoom(pokemon, makeRoomObj));
            itemObj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => DestroyObject(obj));
        }

    }

    void UsePokemon_MakeRoom(MyPokemon pokemon, GameObject makeRoomObj)
    {
        // todo 내 포켓몬 설정후 데이터 갱신
        makeRoomObj.transform.Find("Select/Icon/IconImage").GetComponent<Image>().sprite = Resources.LoadAll<Sprite>("images/pokemon-front")[pokemon.poketmonId - 1];
        makeRoomObj.transform.Find("Select/Context").GetComponent<TMP_Text>().text = pokemon.name + "\nhp : " + pokemon.hp.ToString();

        makeRoomObj.transform.Find("MakeBtn").GetComponent<Button>().onClick.RemoveAllListeners();
        makeRoomObj.transform.Find("MakeBtn").GetComponent<Button>().onClick.AddListener(() => MakeRoom(makeRoomObj, pokemon.poketmonId));
        makeRoomObj.transform.Find("MakeBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(makeRoomObj));
    }

    void MakeRoom(GameObject obj, int pokemonId)
    {
        var dropdown = obj.transform.Find("Level/Dropdown").GetComponent<TMP_Dropdown>();
        string dropdownText = dropdown.options[dropdown.value].text;
        string level = Regex.Replace(dropdownText, "[^0-9]", "");
        Debug.Log("level : " + level);

        NetworkManager.Instance.CreateRoom(OnRoomUpdate, int.Parse(level), pokemonId);
    }

    void OnRoomUpdate(SocketIOResponse response)
    {
        try
        {
            string json = response.GetValue().ToString();
            GameDataManager.Instance.myRoomInfo = JsonUtility.FromJson<Room>(json);
            Debug.Log($"RoomUpdate: {json}");

            SocketHandleResponse(GameDataManager.Instance.myRoomInfo.eventType);
        }
        catch (Exception ex)
        {
            Debug.LogError($"RoomUpdate error: {ex.Message}");
        }
    }

    void SocketHandleResponse(string eventType)
    {
        switch (eventType)
        {
            case CommonDefine.SOCKET_CREATE_ROOM:
            case CommonDefine.SOCKET_JOIN_ROOM:
                {
                    mainThreadActions.Enqueue(EnterRoom);
                }
                break;
            case CommonDefine.SOCKET_LEAVE_ROOM:
                {
                    mainThreadActions.Enqueue(LeaveRoom);
                }
                break;

        }
    }

    void CheckMainThreadActions()
    {
        while (mainThreadActions.Count > 0)
        {
            mainThreadActions.Dequeue()?.Invoke();
        }
    }


    void EnterRoom()
    {
        Sprite[] spriteFrontAll = Resources.LoadAll<Sprite>("images/pokemon-front");

        if (roomObj == null)
        {
            GameObject prefab = Resources.Load<GameObject>("prefabs/Room");
            roomObj = Instantiate(prefab, canvas);

            roomObj.transform.Find("Boss/Icon/IconImage").GetComponent<Image>().sprite = spriteFrontAll[GameDataManager.Instance.myRoomInfo.bossPokemonId - 1];
            roomObj.transform.Find("Boss/Level").GetComponent<TMP_Text>().text = "Level " + GameDataManager.Instance.myRoomInfo.bossPokemonId.ToString();

            roomObj.transform.Find("closeBtn").GetComponent<Button>().onClick.AddListener(() => NetworkManager.Instance.LeaveRoom(OnRoomUpdate, GameDataManager.Instance.myRoomInfo.roomId));
            roomObj.transform.Find("closeBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(roomObj));

            if (GameDataManager.Instance.loginData.seq == GameDataManager.Instance.myRoomInfo.leaderId)
            {
                roomObj.transform.Find("startBtn").gameObject.SetActive(true);
            }
            else
            {
                roomObj.transform.Find("startBtn").gameObject.SetActive(false);
            }
        }

        for (int i = 1; i <= 4; ++i)
        {
            roomObj.transform.Find("User/" + i.ToString()).gameObject.SetActive(false);
        }

        for (int i = 0; i < GameDataManager.Instance.myRoomInfo.members.Count; ++i)
        {
            string idx = (i + 1).ToString();
            var member = GameDataManager.Instance.myRoomInfo.members[i];

            if(GameDataManager.Instance.myRoomInfo.leaderId == member.userSeq)
            {
                roomObj.transform.Find("Title").GetComponent<TMP_Text>().text = member.userId + "의 방";
            }

            roomObj.transform.Find("User/" + idx).gameObject.SetActive(true);
            roomObj.transform.Find("User/" + idx + "/Name").GetComponent<TMP_Text>().text = member.userId;

            roomObj.transform.Find("User/" + idx + "/Icon/IconImage").GetComponent<Image>().sprite = spriteFrontAll[member.pokemonId - 1];
        }
    }

    void LeaveRoom()
    {
        int mySeq = GameDataManager.Instance.loginData.seq;
        int leaderSeq = GameDataManager.Instance.myRoomInfo.leaderId;
        if (mySeq == leaderSeq)
        {
            bool leaveMe = true;
            for (int i = 0; i < GameDataManager.Instance.myRoomInfo.members.Count; ++i)
            {
                int userSeq = GameDataManager.Instance.myRoomInfo.members[i].userSeq;
                if (mySeq == userSeq)
                {
                    leaveMe = false;
                    break;
                }
            }

            if (leaveMe == false)
            {
                EnterRoom();
            }
        }
        else
        {
            bool leaveMe = true;
            bool leaveLeader = true;
            for (int i = 0; i < GameDataManager.Instance.myRoomInfo.members.Count; ++i)
            {
                int userSeq = GameDataManager.Instance.myRoomInfo.members[i].userSeq;
                if (mySeq == userSeq)
                {
                    leaveMe = false;
                }

                if (leaderSeq == userSeq)
                {
                    leaveLeader = false;
                }
            }

            if (leaveMe == false)
            {
                if (leaveLeader)
                {
                    NetworkManager.Instance.LeaveRoom(OnRoomUpdate, GameDataManager.Instance.myRoomInfo.roomId);
                    DestroyRoomObject();
                    CreateMsgBoxOneBtn("방장이 방을 나갔습니다.");
                }
                else
                {
                    EnterRoom();
                }
            }
        }
    }

    void DestroyRoomObject()
    {
        DestroyObject(roomObj);
    }

    void OnClickEnterInventory()
    {
        // todo GameDataManager의 내 포켓몬 데이터 확인후 없으면 서버에서 포켓몬 데이터 받아오기
        if (GameDataManager.Instance.myPokemonList == null)
        {
            NetworkManager.Instance.SendServerGet(CommonDefine.GET_MY_POKEMON_URL, null, CallbackMyPokemon);
        }
        else
        {
            CreateInventory();
        }
    }


    void CallbackMyPokemon(bool result)
    {
        if (result)
        {
            CreateInventory();

        }
        else
        {
            CreateMsgBoxOneBtn("내 포켓몬 로드 실패");
        }
    }

    void CreateInventory()
    {
        GameObject prefab = Resources.Load<GameObject>("prefabs/Inventory");
        GameObject obj = Instantiate(prefab, canvas);

        obj.transform.Find("closeBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(obj));

        obj.transform.Find("Title").GetComponent<TMP_Text>().text = "인벤토리";

        Sprite[] spriteFrontAll = Resources.LoadAll<Sprite>("images/pokemon-front");
        GameObject itemPrefab = Resources.Load<GameObject>("prefabs/InventoryItem");
        Transform content = obj.transform.Find("ScrollView/Viewport/Content");

        for (int i = 0; i < GameDataManager.Instance.myPokemonList.Length; i++)
        {
            var pokemon = GameDataManager.Instance.myPokemonList[i];

            GameObject itemObj = Instantiate(itemPrefab, content);

            itemObj.transform.Find("Icon/IconImage").GetComponent<Image>().sprite = spriteFrontAll[pokemon.poketmonId - 1];

            itemObj.transform.Find("Title").GetComponent<TMP_Text>().text = pokemon.name;
            itemObj.transform.Find("Context").GetComponent<TMP_Text>().text = "hp : " + pokemon.hp.ToString();

            itemObj.transform.Find("Button").gameObject.SetActive(false);
        }

    }

    void DestroyObject(GameObject obj)
    {
        Destroy(obj);
    }

    void CreateMsgBoxOneBtn(string desc, Action checkResult = null)
    {
        GameObject msgBoxPrefabOneBtn = Resources.Load<GameObject>("prefabs/MessageBox_1Button");
        GameObject obj = Instantiate(msgBoxPrefabOneBtn, canvas);

        obj.transform.Find("desc").GetComponent<TMP_Text>().text = desc;
        obj.transform.Find("CheckBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(obj));

        if (checkResult != null)
        {
            obj.transform.Find("CheckBtn").GetComponent<Button>().onClick.AddListener(() => checkResult());
        }
    }




    #region EasterEgg

    int upArrowCount = 0;
    void EasterEggInit()
    {
        upArrowCount = 0;

        lobbyObj.transform.Find("GrantBtn").gameObject.SetActive(false);
        lobbyObj.transform.Find("DeductBtn").gameObject.SetActive(false);

        lobbyObj.transform.Find("GrantBtn").GetComponent<Button>().onClick.AddListener(OnClickGrant);
        lobbyObj.transform.Find("DeductBtn").GetComponent<Button>().onClick.AddListener(OnClickDeduct);
    }

    void OnClickDeduct()
    {
        CreateLoadingCircle();

        WalletGetSetPostData data = new WalletGetSetPostData
        {
            amount = "1000",
        };

        NetworkManager.Instance.SendServerPost(CommonDefine.BLOCKCHAIN_DEDUCT_URL, data, CallbackDeduct);
    }

    void CallbackDeduct(bool result)
    {
        DestroyLoadingCircle();

        if (result)
        {
            CreateMsgBoxOneBtn("CallbackDeduct 성공", OnClickUpdateWallet);
        }
        else
        {
            CreateMsgBoxOneBtn("CallbackDeduct 실패");
        }
    }

    void OnClickGrant()
    {
        CreateLoadingCircle();

        WalletGetSetPostData data = new WalletGetSetPostData
        {
            amount = "1000",
        };

        NetworkManager.Instance.SendServerPost(CommonDefine.BLOCKCHAIN_GRANT_URL, data, CallbackGrant);
    }

    void CallbackGrant(bool result)
    {
        DestroyLoadingCircle();

        if (result)
        {
            CreateMsgBoxOneBtn("CallbackGrant 성공", OnClickUpdateWallet);
        }
        else
        {
            CreateMsgBoxOneBtn("CallbackGrant 실패");
        }
    }

    void EasterEgg()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            upArrowCount++;
            if(upArrowCount >= 3)
            {
                lobbyObj.transform.Find("GrantBtn").gameObject.SetActive(true);
                lobbyObj.transform.Find("DeductBtn").gameObject.SetActive(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            upArrowCount = 0;

            lobbyObj.transform.Find("GrantBtn").gameObject.SetActive(false);
            lobbyObj.transform.Find("DeductBtn").gameObject.SetActive(false);
        }
      
    }

    #endregion

    async void OnDestroy()
    {
        await NetworkManager.Instance.DisconnectSocket();
    }
}
