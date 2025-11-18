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

        var chmanager = CharacterSelectManager.Instance;

        if (chmanager.choiceCharacter.Contains(this))
        {
            chmanager.choiceCharacter.Remove(this);
            _image.color = Color.white;
            return;
        }
        if (chmanager.choiceCharacter.Count >= 3) return;

        if(!chmanager.choiceCharacter.Contains(this))
        {
            bool isSelected = chmanager.SelectCharacter(this);
            if (isSelected)
                _image.color = Color.gray;
        }
    }
}
