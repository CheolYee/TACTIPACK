using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public List<ICharacter> partySlot = new(3);
    Wizard wizard = new Wizard();
    Warrior warrior = new Warrior();
    Healer Healer = new Healer();

    private void Start()
    {
        PlayerPartySet();
    }
        
    public void PlayerPartySet()
    {
        Debug.Log("파티 구성");
        partySlot.Add(wizard);
        partySlot.Add(warrior);
        partySlot.Add(Healer);
    }
}
