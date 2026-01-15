using UnityEngine;

public class RigControl : SingletonMonoBehaviour<RigControl>
{
    [SerializeField] private ParticleSystem windParticle;

    public void UpdateWindParticle(Vector3 bodyVelocity)
    {
        bool toggle = Mathf.Abs(Vector3.Dot(bodyVelocity, transform.forward)) > 2;

        if (toggle)
        {
            if (!windParticle.isPlaying)
            {
                windParticle.Play();
            }
        }
        else
        {
            if (windParticle.isPlaying)
            {
                windParticle.Stop();
            }
        }
    }
}