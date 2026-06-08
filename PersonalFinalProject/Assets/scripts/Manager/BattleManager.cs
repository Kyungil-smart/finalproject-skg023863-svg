using MyGame;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private InputController _inputController;
    [SerializeField] private GameObject _player1;
    [SerializeField] private GameObject _player2;
    [SerializeField] private FighterData[] _fighterDataList;

    private Fighter _fighter1;
    private Fighter _fighter2;

    private FighterView _fighter1View;
    private FighterView _fighter2View;

    void Awake()
    {
        _fighter1View = _player1.GetComponent<FighterView>();
        
        _fighter1 = new Fighter();
        _fighter2 = new Fighter();
        
        _fighter1View.Initialize(_fighter1);
        _fighter1.BattleSetup(_fighterDataList[0], new Vector2(0, 0));
    }

    private void FixedUpdate()
    {
        _fighter1.UpdateInput(_inputController.Player1InputData);
        _fighter2.UpdateInput(_inputController.Player2InputData);
        
        _fighter1.UpdateMovement();
        _fighter2.UpdateMovement();
    }
}
