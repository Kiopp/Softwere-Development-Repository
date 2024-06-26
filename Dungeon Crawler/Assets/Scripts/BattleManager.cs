using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using GameInputActions;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

//A battle manager responsible for managing battles between the player and enemies
public class BattleManager : MonoBehaviour
{
    private BattleTrigger callingTrigger; // The BattleTrigger that initiated the latest battle
    private IBattleEntity Player; //The player entity in the battle
    private IBattleEntity Enemy; //The enemy entity in the battle
    public UnityEvent BattleStart;
    public UnityEvent BattleEnded;
    public UnityEvent<BattleRoundEventArgs> BattleRound;
    private PlayerInputActions playerInputActions; //Handles the players input

    private bool playerIsAttacking = false;

    /// <summary>
    /// Saves a reference to the latest BattleTrigger that starts a battle
    /// </summary>
    /// <param name="caller"></param>
    public void BattleTriggerCheckIn(BattleTrigger caller)
    {
        callingTrigger = caller;
    }

    public void OnPlayerAttack()
    {
        playerIsAttacking = true;
    }

    public void StopPlayerAttack()
    {
        playerIsAttacking = false;
    }

    //Starts the battle between a player and an enemy
    public virtual void StartBattle(IBattleEntity player, IBattleEntity enemy)
    {
        Player = player;
        Enemy = enemy;

        BattleStart?.Invoke();
        Debug.Log("The battle has begun");
        StartCoroutine(BattleLoop()); //Starts the battle loop coroutine
    }

    //Turned based battle loop
    private IEnumerator BattleLoop()
    {
        while (!Player.Dead() && !Enemy.Dead()) //Continues the battle as long as both the player and the enemy is alive
        {
            //Player turn
            Debug.Log("Player turn");
            yield return new WaitUntil(() => playerIsAttacking); //Checks if the attack input is triggered
            double playerDamageDealt = Player.Attack(Enemy); //The player attacks the enemy
            playerIsAttacking = false; // Only attack once

            //Checks if the enemy is dead after the player turn
            if (CheckBattleResult())
            {
                Debug.Log("Enemy is dead, Player wins!");
                BattleEnded?.Invoke();
                EndBattle();
                break;
            }

            //Enemy turn
            Debug.Log("Enemy turn");

            double enemyDamageDealt = Enemy.Attack(Player); //Enemy attacks player

            //Checks if the player is dead after the enemy turn
            if (CheckBattleResult())
            {
                Debug.Log("Player is dead, Enemy wins!");
                EndBattle();
                SceneManager.LoadScene("EndScene");
                break;
            }

            // Update UI elements
            BattleRound.Invoke(new BattleRoundEventArgs(Player.CurrentHealth, ((Player)Player).startHealth, playerDamageDealt, Enemy.CurrentHealth, ((Enemy)Enemy).startHealth, enemyDamageDealt));
            yield return new WaitForSeconds(1);
            yield return null; //Yield control back to unity until next frame
        }
    }

    /// <summary>
    /// Called when either the player or the enemy dies
    /// </summary>
    private void EndBattle()
    {
        callingTrigger.BattleEnded(); // Notify calling triggerobject that the battle has ended
    }

    //Checks if the battle is over
    public bool CheckBattleResult()
    {
        return Player.Dead() ^ Enemy.Dead();
    }
}