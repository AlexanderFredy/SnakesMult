using UnityEngine;

public class DeathParticle : MonoBehaviour
{
    [SerializeField] private GameObject _deathParticle;
    private Material _material;

    private void OnDestroy()
    {
        var destroyEffect = Instantiate(_deathParticle, transform.position, transform.rotation);
        destroyEffect.GetComponent<ParticleSystemRenderer>().material = _material;
    }

    public void SetDestroyParticleSystemMaterial(Material material)
    {
        _material = material;
    }
}
