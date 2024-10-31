using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyToDamageController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //�u���b�N���Ă��Ȃ�������_���[�W��H�炤
        if (other.gameObject.CompareTag("Player") 
            && other.gameObject.GetComponent<PlayerController>().m_Animator.GetBool("BlockBool") == false)
        {
            other.gameObject.GetComponent<PlayerController>().Damage();           
        }
        //�u���b�N���Ă�����_���[�W��H���Ȃ�
        else if (other.gameObject.CompareTag("Player") 
            && other.gameObject.GetComponent<PlayerController>().m_Animator.GetBool("BlockBool") == true)
        {
            other.gameObject.GetComponent<PlayerController>().BlockDamage();
        }
    }
}
