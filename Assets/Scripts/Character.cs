using UnityEngine;

public class Character : MonoBehaviour //TODO: Convert to Player Character singleton
{
    public Movement movement;
    public Ability[] abilities;

    private float hp = 100;

    public Animator Ub;
    public SpriteRenderer sprite;
    public Rigidbody2D body;

    public bool attacking = false;

    void Enable()
    {
        body = GetComponent<Rigidbody2D>();
        Ub = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    public float getHP()
    {
        return hp;
    }

    public int getDirection()
    {
        return movement.face;
    }

    public void takeDamage(float damage)
    {
        hp -= damage;
        if (hp <= 0)
            die();
        Debug.Log(hp);
    }

    private void die()
    {
        Destroy(gameObject);
    }
}
