using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Prefs", menuName = "Prefs")]
public class Prefs : ScriptableObject
{
    [Header("Audio")] 
    public float masterValue;
    public float masterVolume;
    public float musicValue;
    public float musicVolume;
    public float sfxValue;
    public float sfxVolume;

    [Header("Graphics")] 
    public int resolutionIndex;
    public int resolutionW;
    public int resolutionH;
    public bool fullscreen;
    public FullScreenMode fullScreenMode;
}
