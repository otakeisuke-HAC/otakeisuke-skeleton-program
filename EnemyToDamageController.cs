using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyToDamageController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //ブロックしていなかったらダメージを食らう
        if (other.gameObject.CompareTag("Player") 
            && other.gameObject.GetComponent<PlayerController>().m_Animator.GetBool("BlockBool") == false)
        {
            other.gameObject.GetComponent<PlayerController>().Damage();           
        }
        //ブロックしていたらダメージを食らわない
        else if (other.gameObject.CompareTag("Player") 
            && other.gameObject.GetComponent<PlayerController>().m_Animator.GetBool("BlockBool") == true)
        {
            other.gameObject.GetComponent<PlayerController>().BlockDamage();
        }
    }
}
