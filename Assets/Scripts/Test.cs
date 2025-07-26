using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Transform canvas;

    void OnClickTestBtn1()
    {
        Debug.Log("OnClickTestBtn1");

        Debug.Log(GameDataManager.Instance.testNum);

    }

    IEnumerator Test2(int num)
    {
        Debug.Log(num++);
        yield return new WaitForSeconds(1);
        Debug.Log(num);

    }

    void Init()
    {
        GameObject prefab = Resources.Load<GameObject>("Test");
        GameObject obj = Instantiate(prefab, canvas);

        obj.transform.Find("TestBtn1").GetComponent<Button>().onClick.AddListener(OnClickTestBtn1);

    }

    void Awake()
    {
        Debug.Log("Test Awake");
    }

    void OnEnable()
    {
        Debug.Log("Test OnEnable");
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Test Start");
        Debug.LogWarning("Test Start");
        Debug.LogError("Test Start");

        Init();
        StartCoroutine(Test2(1));
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Test Update");
    }

    void LateUpdate()
    {
        Debug.Log("Test LateUpdate");
    }

    void OnApplicationQuit()
    {
        Debug.Log("Test OnApplicationQuit");
    }

    void OnDisable()
    {
        Debug.Log("Test OnDisable");
    }

    void OnDestroy()
    {
        Debug.Log("Test OnDestroy");
    }

}
