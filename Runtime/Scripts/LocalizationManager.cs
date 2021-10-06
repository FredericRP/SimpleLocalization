using UnityEngine;
using System.Collections.Generic;
using System.IO;
using FredericRP.EventManagement;
using FredericRP.GenericSingleton;
using FredericRP.StringDataList;

namespace FredericRP.SimpleLocalization
{
  public class LocalizationManager : Singleton<LocalizationManager>
  {
    const string LANGUAGE_LIST = "sl-language-list";

    [SerializeField]
    GameEvent LanguageUpdateEvent;
    [SerializeField]
    char fieldSeparator = '\t';

    [SerializeField]
    [Select(LANGUAGE_LIST)]
    int defaultLanguage;

    int currentLanguageId;
    string currentLanguageName;
    List<string> availableLanguageList;

    private IDictionary<string, string> _localizedStrings = new Dictionary<string, string>();

    public static int CurrentLanguageId { get { return Instance.currentLanguageId; } }

    private void Awake()
    {
      availableLanguageList = DataListLoader.GetDataList(LANGUAGE_LIST);
      InitLanguage();
    }

    protected void InitLanguage()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
      SystemLanguage currentSystemLangage = Application.systemLanguage;
#elif UNITY_SWITCH && !UNITY_EDITOR
    SystemLanguage currentSystemLangage = SwitchLocalizationLanguage.GetSwitchSystemLanguage();
#endif
      // Use lower string format
      string requestedLanguage = currentSystemLangage.ToString().ToLower();
      // Find index of request language
      int requestedLanguageId = availableLanguageList.FindIndex(item => item.ToLower().Equals(requestedLanguage));
      // If language has been found in available language list, set it
      if (requestedLanguageId >= 0)
      {
        SetLanguage(requestedLanguageId);
      }
      else
      {
        // otherwise, set the default language
        SetLanguage(defaultLanguage);
      }
    }

    public void SetLanguage(int languageId)
    {
      Debug.Log("SetLanguage " + availableLanguageList[languageId]);
      currentLanguageName = availableLanguageList[languageId];
      currentLanguageId = languageId;
      LoadLocalizedStrings();
      LanguageUpdateEvent?.Raise<int>(languageId);
    }

    private void LoadLocalizedStrings()
    {
#if UNITY_EDITOR
      Debug.Log("load localization strings at : " + "Localized/" + currentLanguageName + "/messages");
#endif
      string text = (Resources.Load("Localized/" + currentLanguageName + "/messages") as TextAsset).text;

      _localizedStrings.Clear();
      using (StringReader reader = new StringReader(text))
      {
        int lineNumber = 0;
        string line;
        while ((line = reader.ReadLine()) != null)
        {
          ++lineNumber;
          int commentPos = line.IndexOf('#');
          if (commentPos != -1) line = line.Substring(0, commentPos);
          if (line.Length == 0) continue;
          string[] words = line.Split(new char[] { fieldSeparator });
          if (words.Length != 1 && words.Length != 2)
          {
            //Debug.Log("Error in column count line " + lineNumber);
            //error number of columns
          }
          if ((byte)(line[0]) == 255) continue;
          string id = words[0];
          if (!_localizedStrings.ContainsKey(id))
          {
            _localizedStrings[id] = words.Length >= 2 ? words[1] : string.Empty;
            // Replace underscore by unbreakable space
            _localizedStrings[id] = _localizedStrings[id].Replace("_", "\u00A0");
            // Replace wrong points
            _localizedStrings[id] = _localizedStrings[id].Replace("…", "...");
            // Replace line feeds
            _localizedStrings[id] = _localizedStrings[id].Replace("\\n", "\n");
          }
          else
          {
            Debug.LogError("Duplicate ID: <" + id + "> on line: " + lineNumber);
          }
        }
      }
    }

#if UNITY_EDITOR
    public bool IsDefined(string localizationIdString)
    {
      return _localizedStrings.ContainsKey(localizationIdString);
    }
#endif

    public static string GetString(string localizationId)
    {
      return Instance.GetLocalizedString(localizationId);
    }

    protected string GetLocalizedString(string localizationId)
    {
      if (_localizedStrings.ContainsKey(localizationId) && _localizedStrings[localizationId] != string.Empty)
        return _localizedStrings[localizationId];
      else
        return localizationId.ToString();
    }
  }
}