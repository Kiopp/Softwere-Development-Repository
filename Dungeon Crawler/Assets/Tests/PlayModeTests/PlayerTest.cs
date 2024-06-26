using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class PlayerTests
{
    private MockPlayer player;
    private Weapon weapon;

    [SetUp] // Creates a mock player and mock weapon for each test
    public void Setup()
    {
        // Create a new GameObject and attach the MockPlayer script
        GameObject playerObject = new GameObject();
        player = playerObject.AddComponent<MockPlayer>(); // Adds the MockPlayer component to the GameObject

        weapon = ScriptableObject.CreateInstance<MockWeapon>(); // Creates a new instance of MockWeapon
        ((MockWeapon)weapon).MockStart(); // Initializes the mock weapon

        player.MockStart(); // Initializes the mock player
    }

    [Test] // Tests if the player health increases when the heal method is called
    public void Heal_PlayerHealthIncreases()
    {
        // Arrange
        double healAmount = 20;
        double startHP = player.CurrentHealth;

        // Act
        player.Heal(healAmount);

        // Assert
        Assert.AreEqual((startHP + healAmount), player.CurrentHealth); // Checks if healing increases the player health
    }

    [Test] // Tests if player health can exceed the maximum health of the player
    public void Heal_PlayerHealthDoesNotExceedStartHealth()
    {
        // Arrange
        double healAmount = 1000;
        double maxHealth = player.startHealth;

        // Act
        player.Heal(healAmount);

        // Assert
        Assert.AreEqual(maxHealth, player.CurrentHealth); // Checks if player health exceeds the player maximum health
    }

    [Test] // Test to see if the player deals the right amount of damage without a weapon equipped
    public void Attack_WithoutWeapon_ReturnsBaseDamage()
    {
        // Arrange
        MockBattleEntity enemy = new MockBattleEntity();

        // Act
        double damageDealt = player.Attack(enemy);

        // Assert
        Assert.AreEqual(10, damageDealt); // Checks if the player deals the right amount of damage without a weapon equipped 
    }

    [Test] // Test to see if the player deals the right aamount of damage with a weapon equipped
    public void Attack_WithWeapon_ReturnsBaseDamagePlusWeaponDamage()
    {
        // Arrange
        MockBattleEntity enemy = new MockBattleEntity();
        player.EquipWeapon(weapon); // Equip a mock weapon to the mock player

        // Act
        double damageDealt = player.Attack(enemy);

        // Assert
        Assert.AreEqual(35, damageDealt); // 10 base damage + 25 weapon damage, Checks if the player deals the right amount of damage with a weapon equipped
    }

    // Simulates a mock player
    public class MockPlayer : Player
    {
        public void MockStart()
        {
            startHealth = 100;
            playerAttackDamage = 10;
            dodgeProbability = 0F;
            CurrentHealth = startHealth / 2;
        }
    }

    // Simulates a mock weapon
    public class MockWeapon : Weapon
    {
        public void MockStart()
        {
            dmg = 25;
        }

        // Returns the damage made by the mock weapon
        public override double dmgDealt()
        {
            return dmg;
        }
    }

    // Simulates a mock battle entity 
    public class MockBattleEntity : IBattleEntity
    {
        public double StartingHealth { get; } // Starting health of the battle entity
        public double AttackDamage { get; } // Attack damage of the battle entity
        public float DodgeProbability { get; private set; } // Dodge probability of the battle entity
        public double CurrentHealth { get; private set; } // Current health of the battle entity

        // Initializes battle entity attributes
        public MockBattleEntity(double health = 100, double damage = 10)
        {
            StartingHealth = health;
            AttackDamage = damage;
            DodgeProbability = 0F;
            CurrentHealth = health;
        }

        // Simulates an attack by the battle entity
        public double Attack(IBattleEntity opponent)
        {
            opponent.TakeDamage(AttackDamage);
            return AttackDamage;
        }

        // Simulates a battle entity taking damage
        public bool TakeDamage(double damage)
        {
            // Checks if the battle entity dodges the attack
            if (Random.Range(0F, 1F) >= DodgeProbability)
            {
                CurrentHealth -= damage;
                return true;
            }
            return false;
        }

        // Checks if the battle entity is dead
        public bool Dead()
        {
            return CurrentHealth <= 0;
        }
    }
}
