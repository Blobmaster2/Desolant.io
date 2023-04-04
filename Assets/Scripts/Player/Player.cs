using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    public float health;
    public float damage;

    public byte food;
    public short temp;
    public byte sanity;

    public float xp;
    public int level;

    public float xpMultiplier;
    public float sanityDecreaseMultiplier;
    public float temperatureDecreaseMultiplier;
    public float damageMultiplier;

    //public 

    public void TakeDamage(int damage)
    {
        health -= damage * damageMultiplier;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {

    }
}
