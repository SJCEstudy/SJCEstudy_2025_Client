using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public Transform canvas;

    GameObject loginObj = null;
    GameObject registerObj = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
    }

   
    void Init()
    {
        loginObj = null;
        registerObj = null;

        //GameDataManager.Instance.ResetData();

        if(canvas == null)
            canvas = GameObject.Find("Canvas").transform;

        GameObject prefab = Resources.Load<GameObject>("prefabs/Login");
        loginObj = Instantiate(prefab, canvas);

        loginObj.transform.Find("LoginBtn").GetComponent<Button>().onClick.AddListener(OnClickLogin);
        loginObj.transform.Find("RegisterBtn").GetComponent<Button>().onClick.AddListener(OnClickRegisterPage);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnClickLogin()
    {
        string id = loginObj.transform.Find("ID").GetComponent<TMP_InputField>().text;
        string password = loginObj.transform.Find("Password").GetComponent<TMP_InputField>().text;

        Debug.Log("id : " + id + " pwd : " + password);

        NetworkManager.Instance.SendLoginServer(CommonDefine.LOGIN_URL, id, password, CallbackLogin);
    }

    void OnClickRegisterPage()
    {
        if(registerObj == null)
        {
            GameObject prefab = Resources.Load<GameObject>("prefabs/Register");
            registerObj = Instantiate(prefab, canvas);

            registerObj.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(OnClickLoginPage);
            registerObj.transform.Find("RegisterBtn").GetComponent<Button>().onClick.AddListener(OnClickRegister);
        }
        else
        {
            registerObj.SetActive(true);
        }

        registerObj.transform.Find("ID").GetComponent<TMP_InputField>().text = "";
        registerObj.transform.Find("Password").GetComponent<TMP_InputField>().text = "";

    }

    void OnClickLoginPage()
    {
        registerObj.SetActive(false);

        loginObj.transform.Find("ID").GetComponent<TMP_InputField>().text = "";
        loginObj.transform.Find("Password").GetComponent<TMP_InputField>().text = "";

    }

    void OnClickRegister()
    {
        string id = registerObj.transform.Find("ID").GetComponent<TMP_InputField>().text;
        string password = registerObj.transform.Find("Password").GetComponent<TMP_InputField>().text;

        Debug.Log("id : " + id + " pwd : " + password);

        NetworkManager.Instance.SendLoginServer(CommonDefine.REGISTER_URL, id, password, CallbackRegister);
    }

    void CallbackRegister(bool result)
    {
        if(result)
        {
            CreateMsgBoxOneBtn("회원가입 성공", OnClickLoginPage);
        }
        else
        {
            CreateMsgBoxOneBtn("회원가입 실패");
        }
    }

    void CallbackLogin(bool result)
    {
        if (result)
        {
            CreateMsgBoxOneBtn("로그인 성공");
            
        }
        else
        {
            CreateMsgBoxOneBtn("로그인 실패");
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

}
