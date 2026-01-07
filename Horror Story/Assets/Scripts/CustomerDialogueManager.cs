using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerDialogueManager : MonoBehaviour
{
    public static CustomerDialogueManager instance;

    public GameObject talkPanel;
    public GameObject choicePack;
    public TextMeshProUGUI subText;
    public Button choice1Button;
    public Button choice2Button;
    public Button choice3Button;
    public AudioSource talkSource;
    public float typeSpeed = 0.03f;

    public List<ConversationData> conversations = new List<ConversationData>();

    ConversationData currentConversation;
    LookAtFunc currentLookAt;
    System.Action onComplete;
    int currentNodeIndex;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        talkPanel.SetActive(false);
        choicePack.SetActive(false);
        choice1Button.onClick.AddListener(() => OnChoice(0));
        choice2Button.onClick.AddListener(() => OnChoice(1));
        choice3Button.onClick.AddListener(() => OnChoice(2));
    }

    public void StartDialogue(int conversationIndex, LookAtFunc lookAt, System.Action onFinish)
    {
        if (conversationIndex < 0 || conversationIndex >= conversations.Count) return;

        currentConversation = conversations[conversationIndex];
        currentLookAt = lookAt;
        onComplete = onFinish;
        currentNodeIndex = 0;

        currentLookAt.IKActive = true;

        StartCoroutine(ShowNode(currentNodeIndex));
    }

    IEnumerator ShowNode(int nodeIndex)
    {
        if (nodeIndex < 0 || nodeIndex >= currentConversation.nodes.Count)
        {
            StartCoroutine(EndDialogue());
            yield break;
        }

        DialogueNode node = currentConversation.nodes[nodeIndex];

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        talkPanel.SetActive(true);

        foreach (string line in node.lines)
        {
            if (talkSource) talkSource.Play();
            subText.text = node.speaker + ": ";
            foreach (char c in line)
            {
                subText.text += c;
                yield return new WaitForSeconds(typeSpeed);
            }
            if (talkSource) talkSource.Stop();
            yield return WaitForClick();
        }

        if (node.choices.Count > 0)
        {
            for (int i = 0; i < 3; i++)
            {
                if (i < node.choices.Count)
                {
                    Button btn = i == 0 ? choice1Button : i == 1 ? choice2Button : choice3Button;
                    btn.gameObject.SetActive(true);
                    btn.GetComponentInChildren<TextMeshProUGUI>().text = node.choices[i].text;
                }
                else
                {
                    Button btn = i == 0 ? choice1Button : i == 1 ? choice2Button : choice3Button;
                    btn.gameObject.SetActive(false);
                }
            }
            choicePack.SetActive(true);
        }
        else
        {
            StartCoroutine(EndDialogue());
        }
    }

    void OnChoice(int choiceIndex)
    {
        DialogueNode node = currentConversation.nodes[currentNodeIndex];
        if (choiceIndex >= node.choices.Count) return;
        StartCoroutine(ProcessChoice(choiceIndex));
    }

    IEnumerator ProcessChoice(int choiceIndex)
    {
        DialogueNode node = currentConversation.nodes[currentNodeIndex];
        choicePack.SetActive(false);

        if (talkSource) talkSource.Play();
        subText.text = "Me: ";

        foreach (char c in node.choices[choiceIndex].text)
        {
            subText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        if (talkSource) talkSource.Stop();

        yield return new WaitForSeconds(0.5f);

        int nextNode = node.choices[choiceIndex].nextNode;
        if (nextNode == -1)
        {
            StartCoroutine(EndDialogue());
        }
        else
        {
            currentNodeIndex = nextNode;
            yield return ShowNode(currentNodeIndex);
        }
    }

    IEnumerator EndDialogue()
    {
        talkPanel.SetActive(false);
        choicePack.SetActive(false);
        subText.text = "";

        if (currentLookAt != null)
        {
            currentLookAt.IKActive = false;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        onComplete?.Invoke();
        yield return null;
    }

    IEnumerator WaitForClick()
    {
        while (!Input.GetMouseButtonDown(0)) yield return null;
    }
}

[System.Serializable]
public class ConversationData
{
    public List<DialogueNode> nodes = new List<DialogueNode>();
}

[System.Serializable]
public class DialogueNode
{
    public string speaker = "Customer";
    [TextArea(2, 4)]
    public List<string> lines = new List<string>();
    public List<ChoiceData> choices = new List<ChoiceData>();
}

[System.Serializable]
public class ChoiceData
{
    [TextArea(2, 4)]
    public string text;
    public int nextNode = -1;
}