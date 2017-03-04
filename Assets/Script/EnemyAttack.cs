﻿using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour {

    public float time_between_attack = 0.5f;
    public int attack_damage = 10;
    bool player_in_range;
    float timer;
    public Animation anim;
    GameObject player;
    CharacterHealth player_health;
    public string attack_animation = "Attack", idle_animation = "Idle",;

    // Use this for initialization
    void Awake ()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        player_health = player.GetComponent<CharacterHealth>();
        anim.CrossFade(attack_animation);
    }

    void OnTriggerEnter (Collider other)
    {
        if(other.gameObject == player)
        {
            player_in_range = true;
        }
    }

    void OnTriggerExit (Collider other)
    {
        if(other.gameObject == player)
        {
            player_in_range = false;
        }
    }
	
    void Update()
    {
        timer += Time.deltaTime;

        if(timer >= time_between_attack && player_in_range)
        {
            Attack();
        }

        if (player_health.current_health <= 0)
        {
            anim.CrossFade(idle_animation);
        }
    }

    void Attack()
    {
        timer = 0f;

        if(player_health.current_health > 0)
        {
            player_health.TakeDamage(attack_damage);
        }
    }
}
