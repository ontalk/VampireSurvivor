using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public Dictionary<int, Data.WeaponData> WeaponData { get; private set; } = new Dictionary<int, Data.WeaponData>();
    public Dictionary<int, Data.Player> PlayerData { get; private set; } = new Dictionary<int, Data.Player>();
    public Dictionary<int, Data.Monster> MonsterData { get; private set; } = new Dictionary<int, Data.Monster>();

    public void Init()
    {
        PlayerData = LoadJson<Data.PlayerData, int, Data.Player>("PlayerData").MakeDict();
        WeaponData = LoadJson<Data.WeaponDataLoader, int, Data.WeaponData>("WeaponData").MakeDict();
        MonsterData = LoadJson<Data.MonsterData, int, Data.Monster>("MonsterData").MakeDict();
    }

    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Resources.Load<TextAsset>($"Data/{path}");
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }

}
