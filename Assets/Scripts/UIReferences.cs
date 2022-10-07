using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIReferences : MonoBehaviour
{
    public TMP_Text CurrentPlayerText;
    public TMP_Text CurrentUnitText;
    public TMP_Text TimerText;
    public GlobalHpBehaviour GlobalHpBarParent;
    public GameObject WinCanvas;
    public TMP_Text WinCanvasText;
    public Image ChargeBar;
    public GameObject DamagePopUpPrefab;
    public GameObject Reticle;
    public TMP_Text EndTurnTimerText;
}
