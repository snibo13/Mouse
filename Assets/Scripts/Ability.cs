using UnityEngine;
using System.Collections;

public abstract class Ability : MonoBehaviour
{
    protected Effect effect { get; set; }
    protected LayerMask targetLayer { get; set; }
    protected float range { get; set; }
    protected float cooldown { get; set; }
    protected bool available { get; set; }
    protected GameObject obj { get; set; }

    public Ability(Effect e, LayerMask t, float r, float c, GameObject o)
    {
        effect = e;
        targetLayer = t;
        range = r;
        cooldown = c;
        available = true;
        obj = o;
    }

    public static IEnumerator resetAvailability(Ability a)
    {
        yield return new WaitForSeconds(a.cooldown);
        a.available = true;
    }

    public abstract void use(Transform transform, int direction);
}

public class CloseAbility : Ability
{
    public CloseAbility(Effect e, LayerMask t, float r, float c, GameObject o) : base(e, t, r, c, o)
    { }

    override public void use(Transform transform, int direction)
    {
        if (!available)
            return;
        Vector2 attackVector = range * Vector2.right * direction;
        Collider2D[] abilityHits = Physics2D.OverlapCircleAll(
            (Vector2)transform.position + attackVector,
            range,
            targetLayer
        );
        IEnumerator rA = resetAvailability(this);
        StartCoroutine(rA);
    }
}

public class RangedAbility : Ability
{
    private float spawnOffset;

    public RangedAbility(Effect e, LayerMask t, float r, float c, GameObject o, float so)
        : base(e, t, r, c, o)
    {
        spawnOffset = so;
    }

    override public void use(Transform transform, int direction)
    {
        if (!available)
            return;
        Vector2 spawnPoint = (Vector2)transform.position + Vector2.right * direction * spawnOffset;
        GameObject projectile = (GameObject)Instantiate(this.obj, spawnPoint, transform.rotation);

        Projectile p = projectile.GetComponent<Projectile>();
        p.addForce(Vector2.right * direction * p.speed); // TODO: Move into initialisation of projectile
        available = false;
        IEnumerator rA = resetAvailability(this);
        StartCoroutine(rA);
    }
}
