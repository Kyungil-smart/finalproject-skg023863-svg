using UnityEngine;
using UnityEngine.EventSystems;

namespace MyGame
{
    // 옵션 메뉴에서 선택 가능한 볼륨 항목을 구분하기 위한 열거형
    public enum OptionType
    {
        None,
        MasterVolume,
        BgmVolume,
        SeVolume
    }
    
    // 옵션 메뉴에서 현재 선택된 옵션 항목을 설정하는 기능을 담당
    public class OptionMenuSelect : MonoBehaviour, ISelectHandler
    {
        [SerializeField] private OptionMenuController _menu;
        [SerializeField] private OptionType _optionType;
        
        public void OnSelect(BaseEventData eventData)
        {
            _menu.SetCursorIndex(_optionType);
        }
    }
}

