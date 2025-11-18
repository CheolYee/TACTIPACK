using _00.Work.WorkSpace.Soso7194._04.Scripts.SO;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDataSO", menuName = " CharacterSO/CharacterDataSO")]
public class CharacterDataSO : AgentDataSo
{
    //힐러 힐량
    [Header("Character Data")]
    [field: SerializeField] public int healAmount { get; set; }
}
