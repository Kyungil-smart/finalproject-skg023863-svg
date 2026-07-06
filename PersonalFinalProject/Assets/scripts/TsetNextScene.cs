using UnityEngine;
using UnityEngine.SceneManagement;

public class TsetNextScene : MonoBehaviour
{
    void Awake()
    {
        SceneManager.LoadScene("BattleScene");
    }
}
