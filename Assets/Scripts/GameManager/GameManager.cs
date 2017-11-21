using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using System.Collections.Generic;
using System.Linq;
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

        //private fields
        public List<GameObject> chests;
        public List<GameObject> skeletons;
        public List<GameObject> orcs;
        public List<GameObject> dragons;
        public List<GameObject> enemies;

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
            this.chests = GameObject.FindGameObjectsWithTag("Chest").ToList();
            this.skeletons = GameObject.FindGameObjectsWithTag("Skeleton").ToList();
            this.enemies.AddRange(this.skeletons);
            this.orcs = GameObject.FindGameObjectsWithTag("Orc").ToList();
            this.enemies.AddRange(this.orcs);
            this.dragons = GameObject.FindGameObjectsWithTag("Dragon").ToList();
            this.enemies.AddRange(this.dragons);
            
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
        }

        public void SwordAttack(GameObject enemy)
        {
            if (enemy != null && enemy.activeSelf && InMeleeRange(enemy))
            {
                this.enemies.Remove(enemy);
                enemy.SetActive(false);
                GameObject.DestroyObject(enemy);
                if(enemy.tag.Equals("Skeleton"))
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

                this.WorldChanged = true;
            }
        }

        public void Fireball(GameObject enemy)
        {
            if (enemy != null && enemy.activeSelf && InFireballRange(enemy) && this.characterData.Mana >= 5)
            {
                
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

                this.WorldChanged = true;
            }
        }

        public void PickUpChest(GameObject chest)
        {
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
