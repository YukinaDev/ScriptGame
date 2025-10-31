using UnityEngine;

public class CodeLockPuzzle : PuzzleBase, IInteractable
{
    [Header("Code Lock Settings")]
    public string correctCode = "1234";
    private string enteredCode = "";

    [Header("UI (Optional)")]
    public string prompt = "Press [E] to enter code";
    public string solvedPrompt = "Lock already opened";

    public string GetInteractPrompt()
    {
        if (isSolved)
            return solvedPrompt;
        
        return prompt;
    }

    public void Interact(GameObject player)
    {
        if (isSolved)
        {
            Debug.Log("This lock is already open!");
            return;
        }

        OpenCodeInputUI();
    }

    void OpenCodeInputUI()
    {
        Debug.Log("Opening code input UI... (implement your UI here)");
    }

    public void SubmitCode(string code)
    {
        if (code == correctCode)
        {
            SolvePuzzle();
        }
        else
        {
            Debug.Log("Wrong code!");
        }
    }

    protected override void OnPuzzleSolved()
    {
        Debug.Log("Code lock opened!");
    }

    protected override void OnPuzzleAlreadySolved()
    {
        Debug.Log("Lock was already opened previously");
    }
}
