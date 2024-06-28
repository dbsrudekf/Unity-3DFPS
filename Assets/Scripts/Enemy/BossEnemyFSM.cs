using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum BossEnemyState { None = -1, Idle = 0, Wander, Pursuit, Attack, Dash, Teleport }

public class BossEnemyFSM : MonoBehaviour
{
    [Header("MeshSetting")]
    [SerializeField]
    private float fadeSpeed = 4;
    private MeshRenderer meshRenderer;

    [Header("Pursuit")]
    [SerializeField]
    private float targetRecognitionRange = 8;

    [Header("Attack")]
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private Transform projectileSpawnPoint;
    [SerializeField]
    private float attackRange = 5;
    [SerializeField]
    private float attackRate = 1;

    [Header("Dash")]
    [SerializeField]
    private float targetDashRange = 20;
    private int DashDamage = 20;

    [Header("Teleport")]
    private float targetTeleportRange = 40;


    private BossEnemyState BossenemyState = BossEnemyState.None;
    private float lastAttackTime = 0;

    private GameObject Player;

    private Status status;
    private NavMeshAgent navMeshAgent;
    private Transform target;

    


    private void Awake()
    {
        
        status = GetComponent<Status>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
  
        navMeshAgent.updateRotation = false;
    }

    private void Update()
    {
    }

    private void OnEnable()
    {
        //Debug.Log("OnEnable");
        ChangeState(BossEnemyState.Idle);
        Player = GameObject.Find("Player");
        target = Player.transform;


    }

    private void OnDisable()
    {
        StopCoroutine(BossenemyState.ToString());
        BossenemyState = BossEnemyState.None;
    }

    public void ChangeState(BossEnemyState newState)
    {
        //Debug.Log("changeState: " + newState + "     " + "enemyState: " + enemyState);

        if (BossenemyState == newState) return;

        StopCoroutine(BossenemyState.ToString());

        BossenemyState = newState;

        StartCoroutine(BossenemyState.ToString());
    }

    private IEnumerator Idle()
    {
        //Debug.Log("Idle");
        StartCoroutine("AutoChangeFromIdleToPersuit");

        while (true)
        {
            CalculateDistanceToTargetAndSelctState();

            yield return null;
        }
    }

    private IEnumerator AutoChangeFromIdleToWander()
    {
        int changeTime = Random.Range(1, 5);

        yield return new WaitForSeconds(changeTime);

        ChangeState(BossEnemyState.Wander);
    }

    private IEnumerator AutoChangeFromIdleToPersuit()
    {
        int changeTime = Random.Range(1, 5);

        yield return new WaitForSeconds(changeTime);

        ChangeState(BossEnemyState.Dash);
    }

    private IEnumerator Wander()
    {
        Debug.Log("wander");
        float currentTime = 0;
        float maxTime = 10;

        navMeshAgent.speed = status.WalkSpeed;

        navMeshAgent.SetDestination(CalculateWanderPosition());

        Vector3 to = new Vector3(navMeshAgent.destination.x, 0, navMeshAgent.destination.z);
        Vector3 from = new Vector3(transform.position.x, 0, transform.position.z);
        transform.rotation = Quaternion.LookRotation(to - from);

        while (true)
        {
            currentTime += Time.deltaTime;

            to = new Vector3(navMeshAgent.destination.x, 0, navMeshAgent.destination.z);
            from = new Vector3(transform.position.x, 0, transform.position.z);
            if ((to - from).sqrMagnitude < 0.01f || currentTime >= maxTime)
            {
                ChangeState(BossEnemyState.Idle);
            }
            CalculateDistanceToTargetAndSelctState();

            yield return null;
        }
    }

    private IEnumerator Pursuit()
    {
        Debug.Log("Pursuit");
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

            if (Time.time - lastAttackTime > attackRate)
            {
                lastAttackTime = Time.time;

                GameObject clone = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
                clone.GetComponent<EnemyProjectile>().Setup(target.position);
            }
            yield return null;
        }
        
    }

    private IEnumerator Dash()
    {
        Debug.Log("dash");

        navMeshAgent.ResetPath();

        LookRotationToTarget();

        Vector3 targetPos = target.position;

        CalculateDistanceToTargetAndSelctState();

        while (true)
        {

            Vector3 Targetto = new Vector3(targetPos.x, 0, targetPos.z);
            Vector3 Bossfrom = new Vector3(transform.position.x, 0, transform.position.z);

            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 20);

            if ((Targetto - Bossfrom).sqrMagnitude < 0.01f)
            {
                yield return new WaitForSeconds(3.0f);
                Debug.Log("3.f");
                ChangeState(BossEnemyState.Idle);
            }

            yield return null;
        }

    }

    private IEnumerator Teleport()
    {
        //Debug.Log("Tele");

        navMeshAgent.ResetPath();

        LookRotationToTarget();

        while(true)
        {
            //Debug.Log("Telewhile");
            CalculateDistanceToTargetAndSelctState();

            yield return new WaitForSeconds(3);

            float distance = Random.Range(-4, 4);

            if(distance == 0 || distance == 1 || distance ==2 || distance == 3)
            {
                distance = 4;
            }
            else
            {
                distance = -4;
            }

            Vector3 TelePos = new Vector3(target.position.x + distance, transform.position.y, target.position.z + distance);

            transform.position = TelePos;

            LookRotationToTarget();
 
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

        if (distance <= attackRange) //타겟 거리가 공격범위안
        {
            ChangeState(BossEnemyState.Attack);
        }   
        else if (distance <= targetRecognitionRange)
        {
            ChangeState(BossEnemyState.Pursuit);
        }
        else if (distance <= targetDashRange)
        {
            ChangeState(BossEnemyState.Dash);
        }
        else
        {
            ChangeState(BossEnemyState.Teleport);
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
        //Gizmos.color = Color.black;
        //Gizmos.DrawRay(transform.position, navMeshAgent.destination - transform.position);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, targetRecognitionRange);

        Gizmos.color = new Color(0.39f, 0.04f, 0.04f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, targetDashRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, targetTeleportRange);
    }

    public void TakeDamage(int damage)
    {
        bool isDie = status.DecreaseHP(damage);

        if (isDie == true)
        {
            //enemyMemoryPool.DeactivateEnemy(gameObject, 1);
            Debug.Log("BossDie");
            gameObject.SetActive(false);
            Player.GetComponent<Status>().IncreaseGold(100);
            Player.GetComponent<Status>().IncreaseScore(1);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(DashDamage);
        }
    }
}
