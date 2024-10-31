using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class SkeletonController : MonoBehaviour
{
    
    private GameObject m_PlayerObj; // プレイヤー
    public GameObject[] m_EnemyObj; 

    public float m_EnemySpeed; //Skeletonの移動スピード
    Animator m_Animator;

    
    public int hp = 5;

    bool m_Invincibility = false; //無敵フラグ
    float m_InvincibilityTime = 0; //無敵時間

    //エネミーのAttackColliderを取得
    public SphereCollider AttackCollider;

    public Transform[] m_PatrolPoints;
    private int m_CurrentPatrolPointIndex = -1;
    NavMeshAgent m_NavMeshAgent;

    bool m_Found = false;

    public float m_ViewingDistance; //見える距離
    public float m_ViewingAngle; //視野角
    public Transform m_PlayerLookPoint; //プレイヤーの注視点
    public Transform m_EyePoint; //自身の目の位置

    AudioSource m_AudioSource;
    public AudioClip a_Damage; //攻撃によるダメージSE
    public AudioClip a_Attack; //攻撃SE
    public AudioClip a_Surprise; //驚きSE

    public UnityEvent OnDestroyed = new UnityEvent(); //このオブジェクトが消えたときにおこるイベント

    enum SkeletonState
    {
        Patrolling, //巡回
        Chasing, //追跡
        ChasingButLosed //見失い中
    }SkeletonState skeletonState;

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerObj = GameObject.Find("Player");
        m_Animator = GetComponent<Animator>();
        AttackCollider = transform.GetChild(0).GetComponent<SphereCollider>();
        //NavMeshAgentの参照を取得
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        //NavMeshAgentのスピードを取得
        m_EnemySpeed = m_NavMeshAgent.speed;
        m_AudioSource = GetComponent<AudioSource>();
        //パトロール中の状態にする
        skeletonState = SkeletonState.Patrolling;
        //最初のパトロールポイントを指定
        SetNewPatrolPointToDestination();
        m_PlayerLookPoint = m_PlayerObj.transform.Find("LookPoint");
        m_EyePoint = GameObject.Find("EyePoint").transform;
        m_EnemyObj = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < m_EnemyObj.Length; i++)
        {
            SetActiveCollision(true, gameObject, m_EnemyObj[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //パトロール中
        if(skeletonState == SkeletonState.Patrolling)
        {
            GetComponent<NavMeshAgent>().speed = 2;
            if (CanSeePlayer())
            {
                Debug.Log("Mokutekti0");
                skeletonState = SkeletonState.Chasing;
                m_NavMeshAgent.destination = m_PlayerObj.transform.position;
            }else if(HasArrived())
            {
                SetNewPatrolPointToDestination();               
            }

        }
        //追跡中
        else if(skeletonState == SkeletonState.Chasing)
        {
            if (m_Found == false)
            {
                Found();
                m_Found = true;
            }

            m_EnemySpeed = GetComponent<NavMeshAgent>().speed = 5;

            if (CanSeePlayer())
            {
                m_NavMeshAgent.destination = m_PlayerObj.transform.position;
            }
            else
            {
                skeletonState = SkeletonState.ChasingButLosed;
            }

            float distance = Vector3.Distance(m_PlayerObj.transform.position, transform.position);
            //指定した距離まで近づいたらEnemyのスピードを0にする
            if (distance <= 3 && hp > 0)
            {
                GetComponent<NavMeshAgent>().speed = 0;

                //Attackアニメーションの再生を止める
                if (m_PlayerObj.CompareTag("Player"))　//プレイヤーが死んでいたら攻撃の対象としないための条件文
                {
                    m_Animator.SetBool("Attack", true);
                    Debug.Log("攻撃");
                }
                else
                {
                    m_Animator.SetBool("Attack", false);
                }
            }
            else
            //範囲外ならスピードを戻す
            //Attackアニメーションの再生を止める
            {
                GetComponent<NavMeshAgent>().speed = 5;
                m_Animator.SetBool("Attack", false);
            }

        }
        //見失い中
        else if(skeletonState == SkeletonState.ChasingButLosed)
        {
            if(CanSeePlayer())
            {
                skeletonState = SkeletonState.Chasing;
                m_NavMeshAgent.destination = m_PlayerObj.transform.position;
            }
            else if(HasArrived())
            {
                skeletonState = SkeletonState.Patrolling;
            }
        }
        m_Animator.SetFloat("Walk", m_EnemySpeed);
        EnemyinvincibilityTime();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Sword") && m_Invincibility == false)
        {
            hp--;
            m_Invincibility = true;
            m_AudioSource.PlayOneShot(a_Damage);
            if (hp > 0)
            {
                m_Animator.SetTrigger("Damage");
                
            }
            else if (hp <= 0)
            {
                m_Animator.SetTrigger("Die");
                gameObject.GetComponent<BoxCollider>().enabled = false;
                gameObject.GetComponent<Rigidbody>().useGravity = false;
            }
            m_InvincibilityTime = 0.1f;
        }
    }
    //無敵時間の処理
    void EnemyinvincibilityTime()
    {
        if (m_InvincibilityTime >= 0)
        {
            m_InvincibilityTime -= Time.deltaTime;
        }
        else if (m_InvincibilityTime <= 0)
        {
            m_Invincibility = false;
        }
    }

    //エネミーが攻撃したら判定を出す関数
    void SkeletonAttack()
    {
        AttackCollider.enabled = !AttackCollider.enabled;
        m_AudioSource.PlayOneShot(a_Attack);
    }
    //発見
    void Found()
    {
        m_AudioSource.PlayOneShot(a_Surprise);
    }
    //パトロールポイントの処理
    void SetNewPatrolPointToDestination()
    {
        m_CurrentPatrolPointIndex += 1;
        if (m_CurrentPatrolPointIndex >= m_PatrolPoints.Length)
        {
            m_CurrentPatrolPointIndex = 0;
        }

        m_NavMeshAgent.destination = m_PatrolPoints[m_CurrentPatrolPointIndex].position;
    }

    //到着したか？
    bool HasArrived()
    {
        return Vector3.Distance(m_NavMeshAgent.destination, transform.position) < 0.5f;
    }

    //プレイヤーが見える距離内にいるか？
    bool IsPlayerInViewingDistance()
    {
        float distanceToPlayer = 
            Vector3.Distance(m_PlayerLookPoint.position, m_EyePoint.position);

        return distanceToPlayer <= m_ViewingDistance;
    }
    //挙動がおかしくなってしまったためコメントアウトしている
    //プレイヤーが見える視野角内にいるか
    //bool IsPlayerInViewingAngle()
    //{
    //    Vector3 directionToPlayer = 
    //        m_PlayerLookPoint.position - m_EyePoint.position;

    //    float angleToPlayer = Vector3.Angle(m_EyePoint.forward, directionToPlayer);
    //    //見える視野角の範囲内にプレイヤーがいるかどうかを返却する
    //    return angleToPlayer <= m_ViewingAngle;
    //}

    //プレイヤーにRayを飛ばしたら当たるのか?
    bool CanHitRayToPlayer()
    {
        Vector3 directionToPlayer = m_PlayerLookPoint.position - m_EyePoint.position;
        //プレイヤーに向けてRaycastする
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(m_EyePoint.position, directionToPlayer, out hitInfo);
        //プレイヤーにRayが当たったらどうかをへんきゃくする
        return hit && hitInfo.collider.CompareTag("Player");
    }

    //プレイヤーが見えるか?
    bool CanSeePlayer()
    {
        if (!IsPlayerInViewingDistance()) return false;
        　　　　　　　　　　　　　　　　　　　　　　　//挙動がおかしくなってしまったためコメントアウトしている
        if (skeletonState == SkeletonState.Patrolling)/* && !IsPlayerInViewingAngle()) return false*/;

        if (!CanHitRayToPlayer()) return false;

        return true;
    }
    //死亡
    void Die()
    {
        Destroy(gameObject);
        
    }

    public static void SetActiveCollision(bool isCollide, GameObject targetObj1, GameObject targetObj2)
    {
        var colliders1 = targetObj1.GetComponentsInChildren<Collider>();
        var colliders2 = targetObj2.GetComponentsInChildren<Collider>();

        foreach (var col1 in colliders1)
        {
            foreach (var col2 in colliders2)
            {
                Physics.IgnoreCollision(col1, col2, !isCollide);
            }
        }
    }
    //オブジェクトが消えたらイベントを発生させる
    //スケルトンが死んだら次のスケルトンを出現させる
    //最後のスケルトンが死んだら指定した階段のオブジェクトのスクリプトを起動する
    private void OnDestroy()
    {
        OnDestroyed.Invoke();
    }
}
