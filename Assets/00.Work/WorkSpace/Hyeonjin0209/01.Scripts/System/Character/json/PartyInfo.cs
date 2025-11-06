using UnityEngine;
using System.IO;

public class PartyInfo 
{
    private static string SavePath => Application.dataPath + "/Saves/";

    public static void Save(SaveData saveData, string saveFileName)
    {
        if (!Directory.Exists(SavePath))// 경로에 폴더가 실제로 존재하는지
        {
            Directory.CreateDirectory(SavePath); //없으면 만들어라
        }

        string saveJson = JsonUtility.ToJson(saveData);//Json형태의 string 변수로 변환

        string saveFilePath = SavePath + saveFileName + ".json";//파일 경로 및 이름 저장
        File.WriteAllText(saveFilePath, saveJson);//경로에 있는 파일에다가 saveData에 있는 정보들을 saveJson에서 Tostring 함수를 이용해 문자열로 변환하고,
                                                  // 그걸 문자열로 작성
        Debug.Log("Save Success: " + saveFilePath); //저장 성공 했는지 디버깅
    }

    public static T Load<T>(string saveFileName)//데이터 로드
    {
        string saveFilePath = SavePath + saveFileName + ".json";

        if (!File.Exists(saveFilePath))
        {
            Debug.LogError("No such saveFile exists");
        }

        string saveFile = File.ReadAllText(saveFilePath);
        T saveData = JsonUtility.FromJson<T>(saveFile);
        return saveData;
    }
}
