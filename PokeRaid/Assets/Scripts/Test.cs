using UnityEngine;

public class Test : MonoBehaviour
{
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
