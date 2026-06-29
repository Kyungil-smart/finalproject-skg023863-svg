using System;
using UnityEngine;

namespace MyGame
{
    public class DrawBox : MonoBehaviour
    {
        private FighterView _player;
        private Fighter _fighter;

        void Awake()
        {
            _player = GetComponent<FighterView>();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            if (_fighter == null)
            {
                _fighter = _player.Fighter;
            }
            
            
            // Debug.Log($"{gameObject.name} : {_fighter}");
            
            DrawHitBox(Color.red);
            DrawHurtBox(Color.yellow);
        }

        private void DrawHitBox(Color color)
        {
            Gizmos.color = color;
            
            // if (_fighter.HitBoxes == null) return;
            
            foreach (HitBox box in _fighter.HitBoxes)
            {
                DrawBoxes(box);
            }
        }

        private void DrawHurtBox(Color color)
        {
            Gizmos.color = color;
            
            // if (_fighter.HurtBoxes == null) return;
            
            foreach (HurtBox box in _fighter.HurtBoxes)
            {
                DrawBoxes(box);
            }
        }

        private void DrawBoxes(BoxBase box)
        {
            // Debug.Log("박스 그림");
            //Debug.Log(_fighter.HitBoxes.Count);
            //Debug.Log(_fighter.HurtBoxes.Count);
            
            // DrawWireCube의 인자중에 center는 정중앙을 뜻한다.
            Vector3 boxcenter = new Vector3(box.rect.x, box.rect.y + box.rect.height / 2, 0);
            Vector3 boxsize = new Vector3(box.rect.width, box.rect.height, 0);
            Gizmos.DrawWireCube(boxcenter, boxsize);
        }
    }
}

