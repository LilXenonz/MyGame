using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SleepSystem : MonoBehaviour
{
    [Header("UI & Animator")]
    [SerializeField] private Animator anim;
    [SerializeField] private TextMeshProUGUI subText;
    [Header("Typing Settings")]
    [SerializeField] private float writeSpeed = 0.05f;
    [Header("Game Start Settings")]
    public bool fadeScene = false;
    public string Holder = "";
    public int sceneIndex;

    private static SleepSystem instance;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);

        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }

        if (subText == null)
        {
            subText = GetComponentInChildren<TextMeshProUGUI>();
        }

    }

    private void Start()
    {
        if (fadeScene)
        {
            FadeToScene(sceneIndex);
        }
    }

    public void FadeToScene(int targetSceneIndex)
    {        
        StartCoroutine(FadeSceneRoutine(targetSceneIndex));
    }

    private IEnumerator FadeIn()
    {
        if (anim == null || subText == null)
        {
            Debug.LogError("Animator or TextMeshProUGUI not assigned!");
            yield break;
        }

        anim.SetInteger("FadeInt", 1);

        subText.text = "";

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < Holder.Length; i++)
        {
            subText.text += Holder[i];
            yield return new WaitForSeconds(writeSpeed);
        }
    }

    private IEnumerator FadeOut()
    {
        if (anim == null || subText == null)
        {
            Debug.LogError("Animator or TextMeshProUGUI not assigned!");
            yield break;
        }

        for (int i = Holder.Length; i >= 0; i--)
        {
            subText.text = Holder.Substring(0, i);
            yield return new WaitForSeconds(writeSpeed);
        }

        yield return new WaitForSeconds(1f);

        subText.text = "";

        anim.SetInteger("FadeInt", 2);
    }

    private IEnumerator FadeSceneRoutine(int targetSceneIndex)
    {
        yield return StartCoroutine(FadeIn());
        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene(targetSceneIndex);

        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => SceneManager.GetActiveScene().buildIndex == targetSceneIndex);

        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeOut());

        yield return new WaitForSeconds(1f);

        anim.SetInteger("FadeInt", 0);
    }
}