using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public Transform canvas;

    Slider loadingBar;
    TMP_Text loadingBarText;
    TMP_Text loadingText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
    }

    void Init()
    {
        canvas = GameObject.Find("Canvas").transform;

        GameObject prefab = Resources.Load<GameObject>("prefabs/Loading");
        GameObject obj = Instantiate(prefab, canvas);

        loadingBar = obj.transform.Find("LoadingBar").GetComponent<Slider>();
        loadingBarText = obj.transform.Find("LoadingBar/LoadingText").GetComponent<TMP_Text>();
        loadingText = obj.transform.Find("LoadingText").GetComponent<TMP_Text>();


        loadingBar.value = 0f;
        loadingBar.maxValue = 1.0f;
        //StartCoroutine(LoadScene());
        StartCoroutine(LoadSceneTime(3.0f));
        StartCoroutine(LoadingText(0.3f, 3));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator LoadingText(float duration, int maxDotCount)
    {
        int curDot = 0;
        while (true)
        {
            string dot = "";
            for(int i = 0; i < curDot; ++i)
            {
                dot += ".";
            }

            loadingText.text = "Loading" + dot;
            ++curDot;
            if (curDot > maxDotCount)
                curDot = 0;

            yield return new WaitForSeconds(duration);
        }

    }

    IEnumerator LoadSceneTime(float duration)
    {
        float time = 0f;
        float startValue = 0f;
        float endValue = loadingBar.maxValue;

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(GameDataManager.Instance.nextScene);
        asyncOperation.allowSceneActivation = false;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            SetLoadingBar(Mathf.Lerp(startValue, endValue, t));
            yield return null;
        }

        loadingBar.value = endValue;
        asyncOperation.allowSceneActivation = true;

    }

    IEnumerator LoadScene()
    {
        yield return null;
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(GameDataManager.Instance.nextScene);
        asyncOperation.allowSceneActivation = false;
        float timer = 0.0f;
        while (!asyncOperation.isDone)
        {
            yield return null;
            timer += Time.deltaTime;
            if (asyncOperation.progress < 0.9f)
            {
                SetLoadingBar(Mathf.Lerp(loadingBar.value, asyncOperation.progress, timer));
                if (loadingBar.value >= asyncOperation.progress)
                {
                    timer = 0f;
                }
            }
            else
            {
                SetLoadingBar(Mathf.Lerp(loadingBar.value, 1f, timer));
                if (loadingBar.value == 1.0f)
                {
                    asyncOperation.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }

    void SetLoadingBar(float cur)
    {
        loadingBar.value = cur;
        loadingBarText.text = (cur * 100).ToString("F1") + " / 100.0";
    }
}
