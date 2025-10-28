using UnityEngine;
using UnityEngine.UI;


public enum CharacterType
{
    wizard,//마법사
    barbara,//힐러
    darious,//전사
    iceWizard,//마법사
    leeYuri,//힐러
    garen// 전사
}

public class CharacterSelect : MonoBehaviour
{

    public CharacterType characterType;
    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void PointerDown()
    {

        if (CharacterSelectManager.Instance._choiceCharacter.Count >= 3) return;

        if(!CharacterSelectManager.Instance._choiceCharacter.Contains(characterType))
        {
            bool isSelected = CharacterSelectManager.Instance.SelecteCharacter(characterType);
            if (isSelected)
                _image.color = Color.gray;
            else
                _image.color = Color.white;
        }
    }
}
