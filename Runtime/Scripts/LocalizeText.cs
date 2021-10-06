using FredericRP.EventManagement;
using FredericRP.SimpleLocalization;
using UnityEngine;
using UnityEngine.UI;

public class LocalizeText : MonoBehaviour
{
  private Text textComponent;

  [SerializeField]
  GameEvent LanguageUpdateEvent;
  [SerializeField]
  string textId;

  void Awake()
  {
    textComponent = GetComponent<Text>();
  }

  private void Start()
  {
    UpdateText(LocalizationManager.CurrentLanguageId);
  }

  private void OnEnable()
  {
    LanguageUpdateEvent.Listen<int>(UpdateText);
  }

  private void OnDisable()
  {
    LanguageUpdateEvent.Delete<int>(UpdateText);
  }

  private void UpdateText(int languageId)
  {
    textComponent.text = LocalizationManager.GetString(textId);
  }
}
