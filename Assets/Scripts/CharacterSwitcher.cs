using System.Linq;
using UnityEngine;
using Zenject;

public class CharacterSwitcher : MonoBehaviour
{
   private PlayerInput _uiInput;
   private CharacterSelector _characterSelector;

   [Inject]
   private void Construct(PlayerInput uiInput, CharacterSelector characterSelector)
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
      var nextItem = list.SkipWhile(item => item != selected).Skip(1).FirstOrDefault();
      if (nextItem == null)
      {
         nextItem = list.FirstOrDefault(); 
      }

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
