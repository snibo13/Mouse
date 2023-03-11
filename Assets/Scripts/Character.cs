using UnityEngine;

public class Character : MonoBehaviour
{
    private Movement movement;
    private Ability[] abilities;

    private Controller controller;
    private float hp = 100;

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
