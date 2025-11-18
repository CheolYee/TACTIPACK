using UnityEngine;
using UnityEngine.SceneManagement;

public class PartyLoader : MonoBehaviour
{
    public void OnClickSavepartyButton()
    {
        var chManager = CharacterSelectManager.Instance;

        SaveData data = new SaveData();
        if(chManager.choiceCharacter.Count >= 3)
        {
            foreach (var c in chManager.choiceCharacter)
            {
                data.party.Add(c.characterType);
            }
        }

        PartyInfo.Save(data, "party_002");
        SceneManager.LoadScene("HJ.Main");
    }
}
