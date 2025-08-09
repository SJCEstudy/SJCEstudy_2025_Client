using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Transform canvas;

    GameObject lobbyObj = null;
    GameObject shopObj = null;
    GameObject loadingCircleObj = null;

    List<GameObject> shopItemsObjList = new List<GameObject>();


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

    void Init()
    {
        lobbyObj = null;
        shopObj = null;

        if (canvas == null)
            canvas = GameObject.Find("Canvas").transform;

        GameObject prefab = Resources.Load<GameObject>("prefabs/GameLobby");
        lobbyObj = Instantiate(prefab, canvas);

        lobbyObj.transform.Find("ShopBtn").GetComponent<Button>().onClick.AddListener(OnClickEnterShop);
        lobbyObj.transform.Find("Wallet/LinkWalletBtn").GetComponent<Button>().onClick.AddListener(OnClickLinkWalletPage);
        lobbyObj.transform.Find("Wallet/UpdateWalletBtn").GetComponent<Button>().onClick.AddListener(OnClickUpdateWallet);
        lobbyObj.transform.Find("LogOutBtn").GetComponent<Button>().onClick.AddListener(OnClickLogOut);

        UpdateWallet(true);
    }

    // Update is called once per frame
    void Update()
    {
        EasterEgg();
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
}
