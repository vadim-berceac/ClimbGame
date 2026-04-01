using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Text/Text Database")]
public class TextDatabase : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        [field: SerializeField] public string Key { get; private set; }= string.Empty;      
        [field: SerializeField, TextArea(3, 15)] public string Text { get; private set; } = string.Empty;
    }

    public List<Entry> entries = new List<Entry>();

    private readonly Dictionary<string, string> _cache = new Dictionary<string, string>(StringComparer.Ordinal);

    
    public string Get(string key)
    {
        if (string.IsNullOrEmpty(key))
            return "<empty key>";

        if (_cache.Count == 0 && entries.Count > 0)
            BuildCache();

        if (_cache.TryGetValue(key, out var result))
            return result;

        return $"<missing key: {key}>";
    }

    private void BuildCache()
    {
        _cache.Clear();

        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.Key))
                continue;                  

            if (_cache.ContainsKey(entry.Key))
            {
                Debug.LogWarning($"TextDatabase '{name}': Дублирующийся ключ '{entry.Key}'! " +
                                 "Второй будет проигнорирован.", this);
                continue;
            }

            _cache[entry.Key] = entry.Text;  
        }
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (_cache.Count > 0)
            BuildCache();
#endif
    }
}