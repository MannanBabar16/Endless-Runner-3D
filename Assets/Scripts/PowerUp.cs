using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { Invisibility, Magnet }
    public PowerUpType type;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            switch (type)
            {
                case PowerUpType.Invisibility:
                    player.ActivateInvisibility();
                    break;
                case PowerUpType.Magnet:
                    player.ActivateMagnet();
                    break;
            }

            gameObject.SetActive(false); // Return to pool
        }
    }
}