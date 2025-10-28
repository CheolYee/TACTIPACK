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
    Wizard wizard = new Wizard(); //현재는 파티 구성을 위해 객체만 생성 해 주었고, 추후 수정하며 인스턴스화를 할 예정
    Warrior warrior = new Warrior();
    Healer healer = new Healer();

    private void Start()
    {
        PlayerPartySet();
    }  
    public void PlayerPartySet()
    {
        Debug.Log("파티 구성");
        partySlot.Add(wizard);
        partySlot.Add(warrior);
        partySlot.Add(healer);
    }
}
