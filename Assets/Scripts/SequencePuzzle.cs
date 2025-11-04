using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

// Puzzle cần tương tác theo thứ tự (ví dụ: 4 lever theo thứ tự 3-1-4-2)
public class SequencePuzzle : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public string puzzleName = "Sequence Puzzle";
    public List<GameObject> sequenceObjects; // Thứ tự đúng
    public bool resetOnWrong = true;
    
    [Header("Messages")]
    public string wrongSequenceMessage = "Wrong! Try again...";
    public string correctSequenceMessage = "Correct sequence!";
    public string progressMessage = "Step {0}/{1} completed";
    
    [Header("Events")]
    public UnityEvent OnSequenceComplete;
    public UnityEvent OnWrongStep;
    
    [Header("Audio")]
    public AudioClip correctStepSound;
    public AudioClip wrongStepSound;
    public AudioClip completionSound;
    private AudioSource audioSource;
    
    private int currentStep = 0;
    private bool isCompleted = false;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Register all objects in sequence
        foreach (GameObject obj in sequenceObjects)
        {
            if (obj != null)
            {
                SequencePuzzleButton button = obj.GetComponent<SequencePuzzleButton>();
                if (button == null)
                {
                    button = obj.AddComponent<SequencePuzzleButton>();
                }
                button.puzzleManager = this;
            }
        }
    }
    
    public void OnObjectInteracted(GameObject obj)
    {
        if (isCompleted) return;
        
        // Check xem object có đúng thứ tự không
        if (currentStep < sequenceObjects.Count && obj == sequenceObjects[currentStep])
        {
            // Đúng!
            currentStep++;
            
            // Play sound
            if (correctStepSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(correctStepSound);
            }
            
            // Show progress
            MessageDisplay messageDisplay = FindObjectOfType<MessageDisplay>();
            if (messageDisplay != null && currentStep < sequenceObjects.Count)
            {
                messageDisplay.ShowMessage(string.Format(progressMessage, currentStep, sequenceObjects.Count));
            }
            
            Debug.Log($"[SequencePuzzle] Correct step {currentStep}/{sequenceObjects.Count}");
            
            // Check xem đã hoàn thành chưa
            if (currentStep >= sequenceObjects.Count)
            {
                CompleteSequence();
            }
        }
        else
        {
            // Sai!
            if (wrongStepSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(wrongStepSound);
            }
            
            MessageDisplay messageDisplay = FindObjectOfType<MessageDisplay>();
            if (messageDisplay != null)
            {
                messageDisplay.ShowMessage(wrongSequenceMessage);
            }
            
            OnWrongStep?.Invoke();
            
            Debug.Log($"[SequencePuzzle] Wrong step! Expected {sequenceObjects[currentStep].name}, got {obj.name}");
            
            if (resetOnWrong)
            {
                currentStep = 0;
                Debug.Log("[SequencePuzzle] Sequence reset!");
            }
        }
    }
    
    void CompleteSequence()
    {
        isCompleted = true;
        
        // Play sound
        if (completionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completionSound);
        }
        
        // Show message
        MessageDisplay messageDisplay = FindObjectOfType<MessageDisplay>();
        if (messageDisplay != null)
        {
            messageDisplay.ShowMessage(correctSequenceMessage);
        }
        
        // Trigger event
        OnSequenceComplete?.Invoke();
        
        Debug.Log($"[SequencePuzzle] {puzzleName} completed!");
    }
    
    public void ResetSequence()
    {
        currentStep = 0;
        isCompleted = false;
        Debug.Log("[SequencePuzzle] Manually reset");
    }
}

// Component cho mỗi object trong sequence
public class SequencePuzzleButton : MonoBehaviour, IInteractable
{
    [HideInInspector]
    public SequencePuzzle puzzleManager;
    
    public string interactPrompt = "Press E to activate";
    public AudioClip clickSound;
    
    public string GetInteractPrompt()
    {
        return interactPrompt;
    }
    
    public void Interact(GameObject player)
    {
        if (puzzleManager != null)
        {
            puzzleManager.OnObjectInteracted(gameObject);
        }
        
        // Play click sound
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null && clickSound != null)
        {
            audio.PlayOneShot(clickSound);
        }
    }
}
