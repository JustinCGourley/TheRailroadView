using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }



    [SerializeField]ParticleSystem explosionBasic;
    [SerializeField]ParticleSystem explosionFire;
    [SerializeField]ParticleSystem explosionWater;
    [SerializeField]ParticleSystem explosionEarth;
    [SerializeField]ParticleSystem explosionAir;
    [SerializeField]ParticleSystem explosionIce;
    [SerializeField]ParticleSystem explosionMud;
    [SerializeField]ParticleSystem explosionExplosion;
    [SerializeField]ParticleSystem explosionLava;
    [SerializeField]ParticleSystem explosionSteam;
    [SerializeField]ParticleSystem explosionElectric;

    public void CreateExplosionAt(Vector3 pos, Element element)
    {
        ParticleSystem p = Instantiate(GetExplosion(element));
        p.transform.position = pos;
        p.Play();
        StartCoroutine(CheckFinishParticle(p));
    }

    private ParticleSystem GetExplosion(Element element)
    {
        switch (element)
        {
            case Element.air:
                return explosionAir;
            case Element.fire:
                return explosionFire;
            case Element.water:
                return explosionWater;
            case Element.earth:
                return explosionEarth;
            case Element.ice:
                return explosionIce;
            case Element.mud:
                return explosionMud;
            case Element.explosion:
                return explosionExplosion;
            case Element.lava:
                return explosionLava;
            case Element.steam:
                return explosionSteam;
            case Element.electric:
                return explosionElectric;
            case Element.none:
                return explosionBasic;
            default:
                Debug.LogError($"Unable to grab explosion of type [{element}]");
                return null;
        }

    }

    private IEnumerator CheckFinishParticle(ParticleSystem particleSystem)
    {
        bool finished = false;
        while(!finished)
        {
            yield return new WaitForSeconds(0.5f);
            if (particleSystem.isStopped)
            {
                Destroy(particleSystem.gameObject);
                finished = true;
            }
        }

        yield return null;
    }
}
