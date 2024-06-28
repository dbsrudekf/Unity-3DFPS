using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum EnemyState { None = -1, Idle = 0, Wander, Pursuit, Attack, }

public class EnemyFSM : MonoBehaviour
{
    [Header("Pursuit")]
    [SerializeField]
    private float targetRecognitionRange = 8; // 인식 범위 (이 범위 안에 들어오면 "Pursuit" 상태로 변경)
    [SerializeField]
    private float pursuitLimitRange = 10;     // 추적 범위 (이 범위 바깥으로 나가면 "Wander" 상태로 변경)

    [Header("Attack")]
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private Transform projectileSpawnPoint;
    [SerializeField]
    private float attackRange = 5;
    [SerializeField]
    private float attackRate = 1;

    [Header("Drop")]
    [SerializeField]
    private GameObject[] DropItem;

    private EnemyState enemyState = EnemyState.None;
    private float lastAttackTime = 0;

    private GameObject Player;

    private Status status;
    private NavMeshAgent navMeshAgent;
    private Transform target;
    private EnemyMemoryPool enemyMemoryPool;

    public void Setup(Transform target, EnemyMemoryPool enemyMemoryPool)
    {
        status = GetComponent<Status>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        this.target = target;
        this.enemyMemoryPool = enemyMemoryPool;

        navMeshAgent.updateRotation = false;
    }

    private void OnEnable()
    {
        ChangeState(EnemyState.Idle);
        Player = GameObject.Find("Player");
        
    }

    private void OnDisable()
    {
        StopCoroutine(enemyState.ToString());
        enemyState = EnemyState.None;
    }

    public void ChangeState(EnemyState newState)
    {
        //Debug.Log("changeState: " + newState + "     " + "enemyState: " + enemyState);

        if (enemyState == newState) return;

        StopCoroutine(enemyState.ToString());

        enemyState = newState;

        StartCoroutine(enemyState.ToString());
    }

    private IEnumerator Idle()
    {
        //Debug.Log("Idle");
        StartCoroutine("AutoChangeFromIdleToWander");

        while(true)
        {
            CalculateDistanceToTargetAndSelctState();

            yield return null;
        }
    }

    private IEnumerator AutoChangeFromIdleToWander()
    {
        int changeTime = Random.Range(1, 5);

        yield return new WaitForSeconds(changeTime);

        ChangeState(EnemyState.Wander);
    }

    private IEnumerator Wander()
    {
        //Debug.Log("wander");
        float currentTime = 0;
        float maxTime = 10;

        navMeshAgent.speed = status.WalkSpeed;

        navMeshAgent.SetDestination(CalculateWanderPosition());

        Vector3 to = new Vector3(navMeshAgent.destination.x, 0, navMeshAgent.destination.z);
        Vector3 from = new Vector3(transform.position.x, 0, transform.position.z);
        transform.rotation = Quaternion.LookRotation(to - from);

        while(true)
        {
            currentTime += Time.deltaTime;

            to = new Vector3(navMeshAgent.destination.x, 0, navMeshAgent.destination.z);
            from = new Vector3(transform.position.x, 0, transform.position.z);
            if((to - from).sqrMagnitude < 0.01f || currentTime >= maxTime)
            {
                ChangeState(EnemyState.Idle);
            }

            CalculateDistanceToTargetAndSelctState();
            yield return null;
        }
    }
    
    private IEnumerator Pursuit()
    {
        while (true)
        {
            navMeshAgent.speed = status.RunSpeed;

            navMeshAgent.SetDestination(target.position);

            LookRotationToTarget();

            CalculateDistanceToTargetAndSelctState();

            yield return null;
        }

    }

    private IEnumerator Attack()
    {
        navMeshAgent.ResetPath();

        while (true)
        {
            LookRotationToTarget();

            CalculateDistanceToTargetAndSelctState();

            if(Time.time - lastAttackTime > attackRate)
            {
                lastAttackTime = Time.time;

                GameObject clone = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
                clone.GetComponent<EnemyProjectile>().Setup(target.position);
            }

            yield return null;
        }
    }
    private void LookRotationToTarget()
    {
        Vector3 to = new Vector3(target.position.x, 0, target.position.z);

        Vector3 from = new Vector3(transform.position.x, 0, transform.position.z);

        transform.rotation = Quaternion.LookRotation(to - from);

    }

    private void CalculateDistanceToTargetAndSelctState()
    {
        if (target == null) return;

        float distance = Vector3.Distance(target.position, transform.position);

        if(distance <= attackRange) //타겟 거리가 공격범위안
        {
            ChangeState(EnemyState.Attack);
        }

        else if(distance <= targetRecognitionRange)
        {
            ChangeState(EnemyState.Pursuit);
        }
        else if(distance >= pursuitLimitRange)
        {
            ChangeState(EnemyState.Wander);
        }
    }
    private Vector3 CalculateWanderPosition()
    {
        float wanderRadius = 10;
        int wanderJitter = 0;
        int wanderJitterMin = 0;
        int wanderJitterMax = 360;

        Vector3 rangePosition = Vector3.zero;
        Vector3 rangeScale = Vector3.one * 100.0f;

        wanderJitter = Random.Range(wanderJitterMin, wanderJitterMax);
        Vector3 targetPosition = transform.position + SetAngle(wanderRadius, wanderJitter);

        targetPosition.x = Mathf.Clamp(targetPosition.x, rangePosition.x - rangeScale.x * 0.5f, rangePosition.x + rangeScale.x * 0.5f);
        targetPosition.y = 0.0f;
        targetPosition.z = Mathf.Clamp(targetPosition.z, rangePosition.z - rangeScale.z * 0.5f, rangePosition.z + rangeScale.z * 0.5f);

        return targetPosition;
    }

    private Vector3 SetAngle(float radius, int angle)
    {
        Vector3 position = Vector3.zero;

        position.x = Mathf.Cos(angle) * radius;
        position.z = Mathf.Sin(angle) * radius;

        return position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, navMeshAgent.destination - transform.position);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, targetRecognitionRange);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pursuitLimitRange);
        
        Gizmos.color = new Color(0.39f, 0.04f, 0.04f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void TakeDamage(int damage)
    {
        bool isDie = status.DecreaseHP(damage);
        Vector3 ItemPos = this.transform.position; 

        if (isDie == true)
        {
            ItemPos.y = 0.4f;
            enemyMemoryPool.DeactivateEnemy(gameObject, 0);

            int RandomPer = Random.Range(0, 3);
            if (RandomPer == 0)
            {
                int RandomItem = Random.Range(0, 2);
                Instantiate(DropItem[RandomItem], ItemPos, Quaternion.identity);
            }
            Player.GetComponent<Status>().IncreaseGold(100);
            Player.GetComponent<Status>().IncreaseScore(1);
        }
    }
}
