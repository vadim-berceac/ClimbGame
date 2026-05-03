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

   private void SelectNextCharacter()
   {
      var list = _characterSelector.GetInputBrainModules();
      if (list == null || !list.Any())
      {
         return;
      }

      var selected = _characterSelector.GetSelectedBrain();
    
      var filteredList = list
         .Where(item => item.CurrentInputSourceMode != InputSourceMode.Player)
         .ToList();
    
      if (!filteredList.Any())
      {
         return; 
      }

      var selectedIndex = filteredList.IndexOf(selected);
    
      var nextItem = selectedIndex >= 0 && selectedIndex < filteredList.Count - 1
         ? filteredList[selectedIndex + 1]
         : filteredList[0];

      var indexOfNextItem = list.IndexOf(nextItem);
      if (indexOfNextItem >= 0)
      {
         _characterSelector.SelectByIndex(indexOfNextItem);
      }
   }

   private void OnDisable()
   {
      _uiInput.OnCharacterSwitch -= SelectNextCharacter;
   }
}
