using UnityEngine;
using System.Collections.Generic;

public class ButtonSequencePuzzle : PuzzleBase
{
    [Header("Button Sequence Settings")]
    public int[] correctSequence = new int[] { 1, 3, 2, 4 };
    private List<int> currentSequence = new List<int>();
    
    [Header("Buttons")]
    public GameObject[] buttons;

    protected override void Start()
    {
        base.Start();
        
        if (isSolved)
        {
            DisableButtons();
        }
    }

    public void PressButton(int buttonIndex)
    {
        if (isSolved) return;

        currentSequence.Add(buttonIndex);
        Debug.Log($"Button {buttonIndex} pressed. Sequence: {string.Join(", ", currentSequence)}");

        if (currentSequence.Count == correctSequence.Length)
        {
            if (CheckSequence())
            {
                SolvePuzzle();
            }
            else
            {
                Debug.Log("Wrong sequence! Resetting...");
                currentSequence.Clear();
            }
        }
    }

    bool CheckSequence()
    {
        if (currentSequence.Count != correctSequence.Length)
            return false;

        for (int i = 0; i < correctSequence.Length; i++)
        {
            if (currentSequence[i] != correctSequence[i])
                return false;
        }

        return true;
    }

    protected override void OnPuzzleSolved()
    {
        Debug.Log("Button sequence puzzle solved!");
        DisableButtons();
    }

    void DisableButtons()
    {
        if (buttons == null) return;

        foreach (GameObject button in buttons)
        {
            if (button != null)
            {
                button.SetActive(false);
            }
        }
    }
}
