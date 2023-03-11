using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Controller : MonoBehaviour
{
    public float horizontal;
    private float vertical;

    private Character character;
    private Dictionary<string, bool> abilityUnlocked = new Dictionary<string, bool>();

    private float abilityFreeze = 0.4f;

    private bool abilitiesAvailable = true;

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
    }

    void useAbility()
    {
        if (!abilitiesAvailable)
            return;
        if (Input.GetButtonDown("swipe") && abilityUnlocked["swipe"])
            PlayerAbilities.swipe.use(character.transform, character.getDirection());
    }

    IEnumerator abilityCooldown()
    {
        yield return new WaitForSeconds(abilityFreeze);
        abilitiesAvailable = true;
    }
}
