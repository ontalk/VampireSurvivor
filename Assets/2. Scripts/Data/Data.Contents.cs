using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    #region Character

    [Serializable]
    public class Player
    {
        public int id;
        public string name;
        public int weaponID;
        public int maxHp;
        public int damage;
        public int defense;
        public float moveSpeed;
        public int coolDown;
        public int amount;
    }

    [Serializable]
    public class PlayerData : ILoader<int, Player>
    {
        public List<Player> _players = new List<Player>();

        public Dictionary<int, Player> MakeDict()
        {
            Dictionary<int, Player> dict = new Dictionary<int, Player>();
            foreach (Player player in _players)
                dict.Add(player.id, player);
            return dict;
        }
    }

    #endregion

    #region Monster

    [Serializable]
    public class Monster
    {
        public int id;
        public string name;
        public int maxHp;
        public int damage;
        public int defense;
        public float moveSpeed;
        public int expMul;
    }

    [Serializable]
    public class MonsterData : ILoader<int, Monster>
    {
        public List<Monster> monsters = new List<Monster>();

        public Dictionary<int, Monster> MakeDict()
        {
            Dictionary<int, Monster> dict = new Dictionary<int, Monster>();
            foreach (Monster monster in monsters)
            {
                if (!dict.ContainsKey(monster.id))
                {
                    dict.Add(monster.id, monster);
                }
            }
            return dict;
        }
    }


    #endregion
    #region Weapon

    [Serializable]
    public class WeaponData
    {
        public int weaponID;
        public string weaponName;
        public List<WeaponLevelData> weaponLevelData = new List<WeaponLevelData>();

    }

    [Serializable]
    public class WeaponLevelData
    {
        public int level;
        public int damage;
        public float movSpeed;
        public float force;
        public float cooldown;
        public float size;
        public int penetrate;
        public int countPerCreate;
    }

    [Serializable]
    public class WeaponDataLoader : ILoader<int, WeaponData>
    {
        public List<WeaponData> weapons = new List<WeaponData>();

        public Dictionary<int, WeaponData> MakeDict()
        {
            Dictionary<int, WeaponData> dict = new Dictionary<int, WeaponData>();
            foreach (WeaponData weapon in weapons)
            {
                dict.Add(weapon.weaponID, weapon);
            }
            return dict;
        }
    }
    #endregion

}