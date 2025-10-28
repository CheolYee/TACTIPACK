using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectManager : MonoBehaviour
{
    private CharacterType _type;

    public static CharacterSelectManager Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }
    public List<CharacterType> _choiceCharacter = new List<CharacterType>(3);

    public bool SelecteCharacter(CharacterType type)
    {
        if (_choiceCharacter.Contains(type))
        {
            _choiceCharacter.Remove(type);
            return false;
        }
        else
        {
            if (_choiceCharacter.Count < 3)
                _choiceCharacter.Add(type);
            return true;
        }     
    }
}
