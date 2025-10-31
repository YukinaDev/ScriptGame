using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    [Header("Player Stats")]
    public float currentStamina = 100f;
    public float maxStamina = 100f;
    
    [Header("Inventory")]
    public List<string> inventoryItems = new List<string>();
    public int currentSlot = 0;
    
    [Header("Progress")]
    public List<string> completedHouses = new List<string>();
    public List<string> pickedUpItems = new List<string>();
    public Dictionary<string, bool> puzzleSolved = new Dictionary<string, bool>();
    public Dictionary<string, bool> doorsUnlocked = new Dictionary<string, bool>();
    
    [Header("Settings")]
    public float mouseSensitivity = 2f;
    public float masterVolume = 1f;
    
    [Header("Scene Info")]
    public string lastScene = "HubScene";
    public Vector3 lastPosition = Vector3.zero;

    public PlayerData()
    {
        inventoryItems = new List<string>();
        completedHouses = new List<string>();
        pickedUpItems = new List<string>();
        puzzleSolved = new Dictionary<string, bool>();
        doorsUnlocked = new Dictionary<string, bool>();
    }

    public void AddItem(string itemName)
    {
        if (!inventoryItems.Contains(itemName))
        {
            inventoryItems.Add(itemName);
        }
    }

    public void RemoveItem(string itemName)
    {
        if (inventoryItems.Contains(itemName))
        {
            inventoryItems.Remove(itemName);
        }
    }

    public bool HasItem(string itemName)
    {
        return inventoryItems.Contains(itemName);
    }

    public void MarkHouseCompleted(string houseName)
    {
        if (!completedHouses.Contains(houseName))
        {
            completedHouses.Add(houseName);
        }
    }

    public bool IsHouseCompleted(string houseName)
    {
        return completedHouses.Contains(houseName);
    }

    public void SetPuzzleSolved(string puzzleID, bool solved)
    {
        puzzleSolved[puzzleID] = solved;
    }

    public bool IsPuzzleSolved(string puzzleID)
    {
        return puzzleSolved.ContainsKey(puzzleID) && puzzleSolved[puzzleID];
    }

    public void UnlockDoor(string doorID)
    {
        doorsUnlocked[doorID] = true;
    }

    public bool IsDoorUnlocked(string doorID)
    {
        return doorsUnlocked.ContainsKey(doorID) && doorsUnlocked[doorID];
    }

    public void MarkItemPickedUp(string itemID)
    {
        if (!pickedUpItems.Contains(itemID))
        {
            pickedUpItems.Add(itemID);
        }
    }

    public bool HasPickedUpItem(string itemID)
    {
        return pickedUpItems.Contains(itemID);
    }
}
