using UnityEngine;

/// <summary>
/// 어떤 MonoBehaviour 클래스에도 붙일 수 있는 범용 싱글톤 템플릿
/// 씬에 없으면 자동 생성되고, 씬에 붙여놓으면 그대로 사용됨.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    // 씬에 존재하는지 먼저 찾음
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        // 없으면 자동으로 GameObject를 생성
                        GameObject obj = new GameObject(typeof(T).Name);
                        _instance = obj.AddComponent<T>();
                    }

                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject); // 중복 제거
        }
    }

}
