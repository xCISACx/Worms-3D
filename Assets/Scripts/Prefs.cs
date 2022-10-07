using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Prefs", menuName = "Prefs")]
public class Prefs : ScriptableObject
{
    [Header("Audio")] 
    public float MasterValue;
    public float MasterVolume;
    public float MusicValue;
    public float MusicVolume;
    public float SfxValue;
    public float SfxVolume;

    [Header("Graphics")] 
    public int ResolutionIndex;
    public int ResolutionW;
    public int ResolutionH;
    public bool Fullscreen;
    public FullScreenMode FullScreenMode;
}
