using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "War/ServerConfig")]
public class ServerConfig : ScriptableObject 
{
    [Header("Latency (ms)")] public int minDelayMs = 200, maxDelayMs = 600;
    [Header("Instability")][Range(0, 1)] public float networkErrorChance = 0.05f;
    [Range(0, 1)] public float timeoutChance = 0.05f;
    [Header("Determinism")] public int shuffleSeed = 0;
}
