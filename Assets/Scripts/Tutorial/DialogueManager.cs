using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueBubble;
    public TextMeshProUGUI dialogueText;
    public string[] dialogueLines;       // lines tab
    private int currentLine = 0;
    private bool playerInRange = false;
    private enum TutorialAction { None, Jump, Attack, Run }
    private TutorialAction waitingForAction = TutorialAction.None;

    void Start()
    {
        dialogueBubble.SetActive(false);
        dialogueText.text = "";
    }

    void Update()
    {
        if (playerInRange)
        {

            if (waitingForAction == TutorialAction.None && Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {

                ShowNextLine();
            }

            if (waitingForAction == TutorialAction.Jump && Input.GetKeyDown(KeyCode.Space))
            {
                waitingForAction = TutorialAction.None;
                ShowNextLine();
            }
            if (waitingForAction == TutorialAction.Attack && Input.GetKeyDown(KeyCode.E))
            {
                waitingForAction = TutorialAction.None;
                ShowNextLine();
            }
            if (waitingForAction == TutorialAction.Run && Input.GetKey(KeyCode.LeftShift))
            {
                waitingForAction = TutorialAction.None;
                ShowNextLine();
            }
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            dialogueBubble.SetActive(true);
            playerInRange = true;
            ShowCurrentLine();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            dialogueText.text = "";
            dialogueBubble.SetActive(false);
        }
    }
    void ShowCurrentLine()
    {
        if (currentLine < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLine];

            // Associer une action à la ligne
            if (dialogueLines[currentLine] == "Try to jump by pressing space !")
                waitingForAction = TutorialAction.Jump;

            if (dialogueLines[currentLine] == "Try to attack by pressing E !")
                waitingForAction = TutorialAction.Attack;

            if (dialogueLines[currentLine] == "Try to run by pressing shift and walk !")
                waitingForAction = TutorialAction.Run;
        }
    }
        void ShowNextLine()
        {
            currentLine++;
        if (currentLine == dialogueLines.Length)
        {
            currentLine = 0;
        }
        ShowCurrentLine();
        }
    }

