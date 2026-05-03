using System.Linq;
using UnityEngine;
using Zenject;

public class CharacterSwitcher : MonoBehaviour
{
   private PlayerInputSO _uiInput;
   private CharacterSelector _characterSelector;

   [Inject]
   private void Construct(PlayerInputSO uiInput, CharacterSelector characterSelector)
   {
      _uiInput = uiInput;
      _characterSelector = characterSelector;
   }

   private void Start()
   {
      _uiInput.OnCharacterSwitch += SelectNextCharacter;
   }

   private int _currentIndex = 0;

   private void SelectNextCharacter()
   {
      var list = _characterSelector.GetInputBrainModules();
      if (list == null || list.Count == 0) return;

      var filteredList = list
         .Where(item => item != null && item.CurrentInputSourceMode != InputSourceMode.Player)
         .ToList();

      if (filteredList.Count == 0)
      {
         Debug.Log("Нет доступных AI персонажей");
         return;
      }

      _currentIndex = (_currentIndex + 1) % filteredList.Count;
      var nextBrain = filteredList[_currentIndex];
      var originalIndex = list.IndexOf(nextBrain);

      if (originalIndex >= 0)
      {
         _characterSelector.SelectByIndex(originalIndex);
      }
   }

   private void OnDisable()
   {
      _uiInput.OnCharacterSwitch -= SelectNextCharacter;
   }
}
