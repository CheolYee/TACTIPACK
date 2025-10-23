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
        _image.color = Color.gray;
        CharacterSelectManager.Instance.SelecteCharacter(characterType);

    }
}
