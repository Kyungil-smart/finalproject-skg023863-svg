using System.Collections.Generic;
using MyGame;
using UnityEngine;

namespace MyGame
{
    public class BattleManager : MonoBehaviour
    {
        [SerializeField] private InputController _inputController;
        [SerializeField] private GameObject _player1;
        [SerializeField] private GameObject _player2;
        [SerializeField] private FighterData[] _fighterDataList;

        private Fighter _fighter1;
        private Fighter _fighter2;

        public List<Fighter> fighters = new();

        private FighterView _fighter1View;
        private FighterView _fighter2View;

        void Awake()
        {
            _fighter1View = _player1.GetComponent<FighterView>();
            _fighter2View = _player2.GetComponent<FighterView>();
        
            _fighter1 = new Fighter();
            _fighter2 = new Fighter();
            
            fighters.Add(_fighter1);
            fighters.Add(_fighter2);
            
            _fighterDataList[0].DictionaryInit();
            
            _fighter1View.Initialize(_fighter1);
            _fighter2View.Initialize(_fighter2);
            _fighter1.BattleSetup(_fighterDataList[0], new Vector2(-2, 0), true);
            _fighter2.BattleSetup(_fighterDataList[0], new Vector2(2, 0), false);
        }

        private void FixedUpdate()
        {
            _fighter1.UpdateInput(_inputController.Player1InputData);
            _fighter2.UpdateInput(_inputController.Player2InputData);
            
            fighters.ForEach(f => f.IncrementActionFrame());
            
            _fighter1.UpdateFacingDirection(_fighter2);
            _fighter2.UpdateFacingDirection(_fighter1);
            
            fighters.ForEach(f => f.UpdateAction());
            fighters.ForEach(f => f.UpdateMovement());
            fighters.ForEach(f => f.UpdateBoxes());

            CheckHitAndHurtBox();
        }
        
        private void CheckHitAndHurtBox()
        {
            foreach (Fighter attacker in fighters)
            {
                bool isHit = false;
                int hitAttackID = -1;
                
                foreach (Fighter defender in fighters)
                {
                    if (attacker == defender) continue;

                    foreach (HitBox hitBox in attacker.HitBoxes)
                    {
                        if (!attacker.CanAttackMore(hitBox.attackID)) continue;
                        
                        foreach (HurtBox hurtBox in defender.HurtBoxes)
                        {
                            if (hitBox.BoxOverlap(hurtBox))
                            {
                                Debug.Log("충돌!");
                                
                                isHit = true;
                                hitAttackID = hitBox.attackID;
                            }
                        }
                        
                        if (isHit) break;
                    }
                    
                    if (isHit)
                    {
                        attacker.SuccessfulAttack();
                        defender.DamagedToAttacker();
                        attacker.SetHitStopFrame(hitAttackID);
                        defender.SetHitStopFrame(hitAttackID);
                    }
                }
            }
            
        }
    }
    
}
