using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Controller : MonoBehaviour
{
    private float horizontal;
    private float vertical;

    public Character character;
    private Dictionary<string, bool> abilityUnlocked = new Dictionary<string, bool>();

    private float abilityFreeze = 0.4f;

    private bool abilitiesAvailable = true;

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        if (horizontal < 0)
            character.movement.face = -1;
        character.movement.moveUpdate(horizontal);
        if (Input.GetButtonDown("Jump"))
            character.movement.Jump();
        if (Input.GetButtonDown("Dash"))
            character.movement.Dash();
        useAbility();
    }

    void useAbility()
    {
        if (!abilitiesAvailable)
            return;
        if (Input.GetButtonDown("Swipe") && abilityUnlocked["swipe"])
        {
            PlayerAbilities.swipe.use(character.transform, character.getDirection());
            Debug.Log("Swipe");
        }
    }

    IEnumerator abilityCooldown()
    {
        yield return new WaitForSeconds(abilityFreeze);
        abilitiesAvailable = true;
    }
}
