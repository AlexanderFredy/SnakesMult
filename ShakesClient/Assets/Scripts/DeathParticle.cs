using UnityEngine;

public class DeathParticle : MonoBehaviour
{
    [SerializeField] private GameObject _deathParticle;
    private Material _material;

    public void ShowDestroy()
    {
        var destroyEffect = Instantiate(_deathParticle, transform.position, transform.rotation);
        destroyEffect.GetComponent<ParticleSystemRenderer>().material = _material;
        Destroy(destroyEffect,0.5f);
    }

    public void SetDestroyParticleSystemMaterial(Material material)
    {
        _material = material;
    }
}
