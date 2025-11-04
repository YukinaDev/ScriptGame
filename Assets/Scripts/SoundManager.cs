using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
    [Header("Settings")]
    public bool showDebugLogs = true;
    public bool showDebugGizmos = true;
    
    private List<EnemyAI> registeredEnemies = new List<EnemyAI>();
    private List<SoundEvent> activeSounds = new List<SoundEvent>();
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    public void RegisterEnemy(EnemyAI enemy)
    {
        if (!registeredEnemies.Contains(enemy))
        {
            registeredEnemies.Add(enemy);
            
            if (showDebugLogs)
                Debug.Log($"[SoundManager] Registered enemy: {enemy.gameObject.name}");
        }
    }
    
    public void UnregisterEnemy(EnemyAI enemy)
    {
        registeredEnemies.Remove(enemy);
        
        if (showDebugLogs)
            Debug.Log($"[SoundManager] Unregistered enemy: {enemy.gameObject.name}");
    }
    
    // Được gọi từ FirstPersonController hoặc bất kỳ object nào phát ra sound
    public void EmitSound(Vector3 position, float intensity, string soundType = "Footstep")
    {
        // Notify tất cả enemies
        foreach (EnemyAI enemy in registeredEnemies)
        {
            if (enemy != null)
            {
                enemy.OnSoundHeard(position, intensity);
            }
        }
        
        // Add to active sounds for visualization
        activeSounds.Add(new SoundEvent(position, intensity, Time.time));
        
        if (showDebugLogs)
            Debug.Log($"[SoundManager] Sound emitted at {position}, intensity: {intensity}, type: {soundType}");
    }
    
    void Update()
    {
        // Clean up old sound events (for visualization)
        activeSounds.RemoveAll(s => Time.time - s.timestamp > 0.5f);
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Draw active sounds
        foreach (SoundEvent soundEvent in activeSounds)
        {
            float age = Time.time - soundEvent.timestamp;
            float alpha = 1f - (age / 0.5f);
            
            Gizmos.color = new Color(1f, 0.5f, 0f, alpha);
            Gizmos.DrawWireSphere(soundEvent.position, soundEvent.intensity);
        }
    }
    
    private class SoundEvent
    {
        public Vector3 position;
        public float intensity;
        public float timestamp;
        
        public SoundEvent(Vector3 pos, float inten, float time)
        {
            position = pos;
            intensity = inten;
            timestamp = time;
        }
    }
}
