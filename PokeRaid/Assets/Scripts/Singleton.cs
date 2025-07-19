using UnityEngine;

/// <summary>
/// � MonoBehaviour Ŭ�������� ���� �� �ִ� ���� �̱��� ���ø�
/// ���� ������ �ڵ� �����ǰ�, ���� �ٿ������� �״�� ����.
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
                    // ���� �����ϴ��� ���� ã��
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        // ������ �ڵ����� GameObject�� ����
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
            Destroy(gameObject); // �ߺ� ����
        }
    }

}
