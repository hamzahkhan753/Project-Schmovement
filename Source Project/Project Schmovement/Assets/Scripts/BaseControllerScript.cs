using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public abstract class BaseControllerScript : MonoBehaviour
{
    internal Animator animator;
    //internal string EnemyTag;
    //internal List<ScriptableAttack> HitAttacks = new List<ScriptableAttack>();
    //public bool IsAttacking { get { return animator.GetCurrentAnimatorStateInfo(1).IsTag("AttackState"); } }

    [Header("Stats", order = 0)]
    public float MaxHP;
    public float HP;
    public bool Alive { get { return HP > 0; } }

    // Start is called before the first frame update
    internal void Start()
    {
        SetRigidbodyKinematics(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public void TakeDamage(ScriptableAttack scriptableAttack)
    //{
    //    if (scriptableAttack != null)
    //    {
    //        if (HP > 0)
    //        {
    //            HP -= scriptableAttack.damage;
    //        }

    //        if (HP <= 0)
    //        {
    //            //Kill Enemy Here
    //            print($"{name} has died!");
    //            HP = 0;
    //        }
    //        else
    //        {
    //            animator.SetTrigger("WasHit");
    //        }
    //    }
    //}

    internal virtual void Die()
    {
        // Play Death animation
        animator.enabled = false;
        SetRigidbodyKinematics(false);
    }

    internal void SetRigidbodyKinematics(bool isKine)
    {
        Rigidbody[] playerRigids = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in playerRigids)
        {
            if (rb != null)
            {
                rb.isKinematic = isKine;
                if (!isKine && !rb.useGravity)
                {
                    rb.useGravity = true;
                }
            }
        }
    }
}
