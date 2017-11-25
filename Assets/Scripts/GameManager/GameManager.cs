﻿using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GameManager
{
    public class GameManager : MonoBehaviour
    {
        
        private const float UPDATE_INTERVAL = 0.5f;
        //public fields, seen by Unity in Editor
        public GameObject character;
        public AutonomousCharacter autonomousCharacter;

        public Text HPText;
        public Text ManaText;
        public Text TimeText;
        public Text XPText;
        public Text LevelText;
        public Text MoneyText;
        public GameObject GameEnd;
        public Camera TopCamera;
        public Camera PlayerCamera;


        //private fields
        public List<GameObject> chests;
        public List<GameObject> skeletons;
        public List<GameObject> orcs;
        public List<GameObject> dragons;
        public List<GameObject> enemies;
        public List<GameObject> healthPots;
        public List<GameObject> manaPots;


        public CharacterData characterData;
        public bool WorldChanged { get; set; }
        private DynamicCharacter enemyCharacter;
        private GameObject currentEnemy;
 
        private float nextUpdateTime = 0.0f;
        private Vector3 previousPosition;

        

        public void Start()
        {
            this.WorldChanged = false;
            this.characterData = new CharacterData(this.character);
            this.previousPosition = this.character.transform.position;

            this.enemies = new List<GameObject>();
            this.chests = GameObject.FindGameObjectsWithTag("Chest").OrderBy(go => go.name).ToList();
            this.skeletons = GameObject.FindGameObjectsWithTag("Skeleton").OrderBy(go => go.name).ToList();
            this.enemies.AddRange(this.skeletons);
            this.orcs = GameObject.FindGameObjectsWithTag("Orc").OrderBy(go => go.name).ToList();
            this.enemies.AddRange(this.orcs);
            this.dragons = GameObject.FindGameObjectsWithTag("Dragon").OrderBy(go => go.name).ToList();
            this.enemies.AddRange(this.dragons);
            this.healthPots = GameObject.FindGameObjectsWithTag("HealthPotion").OrderBy(go => go.name).ToList();
            this.manaPots = GameObject.FindGameObjectsWithTag("ManaPotion").OrderBy(go => go.name).ToList();
            //debugEnemies = enemies.ToArray();
            TopCamera = GameObject.Find("Camera").GetComponent<Camera>();
            PlayerCamera = GameObject.Find("CameraPerson").GetComponent<Camera>();
            TopCamera.enabled = true;
            PlayerCamera.enabled = false;
            var offlineTableHeuristic = OfflineTableHeuristic.Instance; // just to initialize singleton
        }

        public void Update()
        {

            if (Time.time > this.nextUpdateTime)
            {
                this.nextUpdateTime = Time.time + UPDATE_INTERVAL;
                this.characterData.Time += UPDATE_INTERVAL;
            }

            if (enemyCharacter != null && currentEnemy != null && currentEnemy.activeSelf)
            {
                this.enemyCharacter.Movement.Target.position = this.character.transform.position;
                this.enemyCharacter.Update();
                this.SwordAttack(currentEnemy);
            }
            else
            {
                foreach (var enemy in this.enemies)
                {
                    if ((enemy.transform.position - this.character.transform.position).sqrMagnitude <= 400)
                    {
                        this.currentEnemy = enemy; 
                        this.enemyCharacter = new DynamicCharacter(enemy)
                        {
                            MaxSpeed = 100
                        };
                        enemyCharacter.Movement = new DynamicSeek()
                        {
                            Character = enemyCharacter.KinematicData,
                            MaxAcceleration = 100,
                            Target = new IAJ.Unity.Movement.KinematicData()
                        };

                        break;
                    }
                }
            }

            this.HPText.text = "HP: " + this.characterData.HP;
            this.XPText.text = "XP: " + this.characterData.XP;
            this.LevelText.text = "Level: " + this.characterData.Level;
            this.TimeText.text = "Time: " + this.characterData.Time;
            this.ManaText.text = "Mana: " + this.characterData.Mana;
            this.MoneyText.text = "Money: " + this.characterData.Money;

            if(this.characterData.HP <= 0 || this.characterData.Time >= 200)
            {
                this.GameEnd.SetActive(true);
                this.GameEnd.GetComponentInChildren<Text>().text = "Game Over";
            }
            else if(this.characterData.Money >= 25)
            {
                this.GameEnd.SetActive(true);
                this.GameEnd.GetComponentInChildren<Text>().text = "Victory";
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                TopCamera.enabled = !TopCamera.enabled;
                PlayerCamera.enabled = !PlayerCamera.enabled;
            }
        }

        public void SwordAttack(GameObject enemy)
        {
            if (enemy != null && enemy.activeSelf && InMeleeRange(enemy))
            {

                //foreach(var i in debugEnemies) {
                //    Debug.Log(i);
                //}
                this.enemies.Remove(enemy);
                enemy.SetActive(false);
                //GameObject.DestroyObject(enemy);
                
                

                if (enemy.tag.Equals("Skeleton"))
                {
                    this.characterData.HP -= 5;
                    this.characterData.XP += 5;
                }
                else if(enemy.tag.Equals("Orc"))
                {
                    this.characterData.HP -= 10;
                    this.characterData.XP += 10;
                }
                else if(enemy.tag.Equals("Dragon"))
                {
                    this.characterData.HP -= 20;
                    this.characterData.XP += 20;
                }
                Destroy(enemy);
                enemy = null;

                //Debug.Log("Depois");
                //foreach (var i in debugEnemies) {
                //    Debug.Log(i);
                //}

                this.WorldChanged = true;
            }
        }

        public void Fireball(GameObject enemy)
        {
            if (enemy != null && enemy.activeSelf && InFireballRange(enemy) && this.characterData.Mana >= 5)
            {
                //foreach (var i in debugEnemies) {
                //    Debug.Log(i);
                //}

                if (enemy.tag.Equals("Skeleton"))
                {
                    this.characterData.XP += 5;
                    this.enemies.Remove(enemy);
                    GameObject.DestroyObject(enemy);
                }
                else if (enemy.tag.Equals("Orc"))
                {
                    this.characterData.XP += 10;
                    this.enemies.Remove(enemy);
                    GameObject.DestroyObject(enemy);
                }
                else if (enemy.tag.Equals("Dragon"))
                {
                }
                this.characterData.Mana -= 5;



                Destroy(enemy);
                enemy = null;

                //Debug.Log("Depois");
                //foreach (var i in debugEnemies) {
                //    Debug.Log(i);
                //}
                this.WorldChanged = true;
            }
        }

        public void PickUpChest(GameObject chest)
        {
            //Debug.Log(chest != null);
            //Debug.Log(chest.activeSelf);
            //Debug.Log(InChestRange(chest));
            if (chest != null && chest.activeSelf && InChestRange(chest))
            {
                

           
                this.chests.Remove(chest);
                GameObject.DestroyObject(chest);
                this.characterData.Money += 5;
                this.WorldChanged = true;

                
            }
        }

        public void LevelUp()
        {
            if (this.characterData.Level == 3) return;
            else if (this.characterData.Level == 2)
            {
                if(this.characterData.XP >= 30)
                {
                    this.characterData.Level = 3;
                    this.characterData.MaxHP = 30;
                    this.characterData.HP = 30;
                    this.WorldChanged = true;
                    return;
                }
            } 
            else if (this.characterData.XP >= 10)
            {
                this.characterData.Level = 2;
                this.characterData.MaxHP = 20;
                this.characterData.HP = 20;
                this.WorldChanged = true;
            }
        }

        public void GetManaPotion(GameObject manaPotion)
        {
            if (manaPotion != null && manaPotion.activeSelf && InPotionRange(manaPotion))
            {
                GameObject.DestroyObject(manaPotion);
                this.characterData.Mana = 10;
                this.WorldChanged = true;
            }
        }

        public void GetHealthPotion(GameObject potion)
        {
            if (potion != null && potion.activeSelf && InPotionRange(potion))
            {
                GameObject.DestroyObject(potion);
                this.characterData.HP = this.characterData.MaxHP;
                this.WorldChanged = true;
            }
        }


        private bool CheckRange(GameObject obj, float maximumSqrDistance)
        {
            return (obj.transform.position - this.characterData.CharacterGameObject.transform.position).sqrMagnitude <= maximumSqrDistance;
        }


        public bool InMeleeRange(GameObject enemy)
        {
            return this.CheckRange(enemy, 16.0f);
        }

        public bool InFireballRange(GameObject enemy)
        {
            return this.CheckRange(enemy, 900.0f);
        }

        public bool InChestRange(GameObject chest)
        {
            return this.CheckRange(chest, 9.0f);
        }

        public bool InPotionRange(GameObject potion)
        {
            return this.CheckRange(potion, 9.0f);
        }
    }
}
