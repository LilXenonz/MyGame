using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager instance;

    public TextMeshProUGUI objectiveText;
    public AudioSource voiceSource;
    public float textDisplayTime = 5f;

    public List<Objective> objectives = new List<Objective>();

    private int currentObjectiveIndex = 0;
    private bool objectiveActive = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (objectiveText) objectiveText.gameObject.SetActive(false);
        StartObjective(0);
    }

    public void StartObjective(int index)
    {
        if (index < 0 || index >= objectives.Count) return;

        currentObjectiveIndex = index;
        objectiveActive = true;

        Objective obj = objectives[currentObjectiveIndex];

        foreach (GameObject go in obj.activateObjects)
        {
            if (go) go.SetActive(true);
        }

        foreach (GameObject go in obj.deactivateObjects)
        {
            if (go) go.SetActive(false);
        }

        if (objectiveText)
        {
            objectiveText.text = obj.text;
            objectiveText.gameObject.SetActive(true);
            StartCoroutine(HideTextAfterDelay());
        }

        if (voiceSource && obj.voiceClip)
        {
            voiceSource.clip = obj.voiceClip;
            voiceSource.Play();
        }
    }

    public void CompleteCurrentObjective()
    {
        if (!objectiveActive) return;

        objectiveActive = false;

        Objective obj = objectives[currentObjectiveIndex];
        obj.interactEvent.Invoke();

        if (obj.nextObjectiveIndex == -1)
        {
            if (objectiveText) objectiveText.gameObject.SetActive(false);
            Debug.Log("All objectives complete!");
        }
        else
        {
            StartObjective(obj.nextObjectiveIndex);
        }
    }

    IEnumerator HideTextAfterDelay()
    {
        yield return new WaitForSeconds(textDisplayTime);
        if (objectiveText) objectiveText.gameObject.SetActive(false);
    }

    public void ShowCurrentObjective()
    {
        if (!objectiveActive) return;
        if (objectiveText)
        {
            objectiveText.gameObject.SetActive(true);
            StartCoroutine(HideTextAfterDelay());
        }
    }
}

[System.Serializable]
public class Objective
{
    [TextArea(2, 4)]
    public string text;
    public AudioClip voiceClip;

    public List<GameObject> activateObjects = new List<GameObject>();
    public List<GameObject> deactivateObjects = new List<GameObject>();

    public UnityEvent interactEvent;

    public int nextObjectiveIndex = -1;
}