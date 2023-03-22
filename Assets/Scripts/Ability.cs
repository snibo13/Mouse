using UnityEngine;
using System.Collections;

public static class PlayerAbilities
{
    public static LayerMask enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
    public static GameObject shockwavePrefab = (GameObject)Resources.Load("Shockwave Prefab");
    public static GameObject swipePrefab = (GameObject)Resources.Load("Swipe Prefab");
    public static GameObject shotPrefab = (GameObject)Resources.Load("SimpleProjectile");

    public static Effect shotEffect = new Effect(0, 0);
    public static RangedAbility shot = new RangedAbility(
        shotEffect,
        enemyLayer,
        0f,
        0f,
        shotPrefab,
        0.7f
    );

    public static Effect swipeEffect = new Effect(30, 1);
    public static CloseAbility swipe = new CloseAbility(
        swipeEffect,
        enemyLayer,
        5f,
        0.4f,
        swipePrefab
    );

    public static Effect shockwaveEffect = new Effect(30, 2);
    public static CloseAbility shockwaveAbility = new CloseAbility(
        shockwaveEffect,
        enemyLayer,
        3.0f,
        2.0f,
        shockwavePrefab
    );
}

public static class EnemyAbilities
{
    public static LayerMask playerLayer = 1 << LayerMask.NameToLayer("Player");

    public static Effect slashEffect = new Effect(5, 1);
    public static CloseAbility slashAbility = new CloseAbility(
        slashEffect,
        playerLayer,
        2.0f,
        2.0f,
        null
    );
}

public abstract class Ability
{
    protected Effect effect { get; set; }
    protected LayerMask targetLayer { get; set; }
    protected float range { get; set; }
    protected float cooldown { get; set; }
    protected GameObject obj { get; set; }

    protected float lastUsed { get; set; }

    public Ability(Effect e, LayerMask t, float r, float c, GameObject o)
    {
        effect = e;
        targetLayer = t;
        range = r;
        cooldown = c;
        obj = o;
        lastUsed = Time.time - cooldown;
    }

    public bool available()
    {
        return (Time.time - lastUsed) >= cooldown;
    }

    public abstract void use(Transform transform, int direction);
}

public class CloseAbility : Ability
{
    public CloseAbility(Effect e, LayerMask t, float r, float c, GameObject o) : base(e, t, r, c, o)
    { }

    override public void use(Transform transform, int direction)
    {
        if (!available())
            return;
        if (obj != null)
        {
            Vector2 spawnPoint = (Vector2)transform.position + Vector2.right * direction * 1f;
            UnityEngine.Object.Instantiate(this.obj, spawnPoint, transform.rotation);
        }
        Vector2 attackVector = range * Vector2.right * direction;
        Collider2D[] abilityHits = Physics2D.OverlapCircleAll(
            (Vector2)transform.position + attackVector,
            range,
            targetLayer
        );
        Character enemy;
        foreach (Collider2D hit in abilityHits)
        {
            hit.TryGetComponent<Character>(out enemy);
            enemy.takeDamage(effect.damage);
            Vector2 pushDirection = direction * Vector2.right;
            enemy.movement.push(effect.knockback * pushDirection.normalized);
        }
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
        if (!available())
            return;

        Vector2 spawnPoint = (Vector2)transform.position + Vector2.right * direction * 0.5f;
        GameObject projectile = (GameObject)
            UnityEngine.Object.Instantiate(this.obj, spawnPoint, transform.rotation);
        Projectile p = projectile.GetComponent<Projectile>();
        p.addForce(Vector2.right * direction * p.speed); // TODO: Move into initialisation of projectile
        lastUsed = Time.time;
    }
}
