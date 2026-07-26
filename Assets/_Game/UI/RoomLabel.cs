using _Game.Features.Dice;
using TMPro;
using UnityEngine;

public class RoomLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;
    
    private int currentLevel = -1;

    void Update()
    {
        if (W.HasResource<PlayerState>() &&
            currentLevel != W.GetResource<PlayerState>().CurrentLevel)
        {
            currentLevel = W.GetResource<PlayerState>().CurrentLevel;
            _label.text = $"Room {currentLevel}";
        }
    }
}
