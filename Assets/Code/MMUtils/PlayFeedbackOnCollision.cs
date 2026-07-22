using MoreMountains.Feedbacks;
using UnityEngine;

public class PlayFeedbackOnCollision : MonoBehaviour
{
    [SerializeField] MMF_Player _player;
    
    void OnCollisionEnter2D(Collision2D other)
    {
        _player?.PlayFeedbacks();
    }
}
