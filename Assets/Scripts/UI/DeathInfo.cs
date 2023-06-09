using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum DeathCause { UNKNOWN, GUN, CHAINSAW, TRAP }

public class DeathInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI killerName;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI victimName;

    [SerializeField] private Sprite[] causeOfDeathImages;

    // Start is called before the first frame update
    void Start()
    {

    }

    /// <summary>
    /// Updates the death information on the board.
    /// </summary>
    /// <param name="killer"></param>
    /// <param name="victim"></param>
    /// <param name="causeOfDeath"></param>
    public void UpdateDeathInformation(string killer, string victim, DeathCause causeOfDeath = DeathCause.UNKNOWN)
    {
        killerName.text = killer;
        victimName.text = victim;

        Sprite causeImage = GetCauseOfDeathImage(causeOfDeath);

        if (causeImage != null)
            icon.sprite = causeImage;
        else
            icon.color = new Color(0, 0, 0, 0);
    }

    private Sprite GetCauseOfDeathImage(DeathCause causeOfDeath) => causeOfDeath == DeathCause.UNKNOWN ? null : causeOfDeathImages[(int)causeOfDeath - 1];
}
