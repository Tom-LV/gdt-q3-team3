using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MusicTrigger : MonoBehaviour
{
    [Tooltip("The audio track that should play when the player enters this area.")]
    public AudioClip roomTrack;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (MusicManager.Instance != null && roomTrack != null)
            {
                MusicManager.Instance.PlayMusic(roomTrack);
            }
        }
    }
}