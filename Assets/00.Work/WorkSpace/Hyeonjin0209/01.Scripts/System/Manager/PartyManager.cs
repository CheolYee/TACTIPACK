using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }

    public List<ICharacter> partySlot = new();
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
