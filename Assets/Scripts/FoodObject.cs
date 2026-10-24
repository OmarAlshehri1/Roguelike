using UnityEngine;

public class FoodObject : CellObject
{
    [SerializeField, Range(1, 50)]
    private int amountGranted = 10;

    [SerializeField] private AudioClip pickupSound;

    [SerializeField, Range(0f, 1f)]
    private float pickupVolume = 0.8f;

    [SerializeField] private ParticleSystem collectEffect;

    public override void PlayerEntered()
    {
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(
                pickupSound,
                transform.position,
                pickupVolume
            );
        }

        if (collectEffect != null)
        {
            ParticleSystem effect = Instantiate(
                collectEffect,
                transform.position,
                Quaternion.identity
            );

            effect.Play();
            Destroy(effect.gameObject, 2f);
        }

        GameManager.Instance.ChangeFood(amountGranted);
        gameObject.SetActive(false);
    }
}
