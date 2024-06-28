using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum BoomEnemyState { None = -1, Idle = 0, Wander, Pursuit, Attack, }

public class BoomEnemyFSM : MonoBehaviour
{
    [Header("Pursuit")]
    [SerializeField]
    private float targetRecognitionRange = 8; // 인식 범위 (이 범위 안에 들어오면 "Pursuit" 상태로 변경)
    [SerializeField]
    private float pursuitLimitRange = 10;     // 추적 범위 (이 범위 바깥으로 나가면 "Wander" 상태로 변경)

    [Header("MeshSetting")]
    [SerializeField]
    private float fadeSpeed = 4;
    private MeshRenderer meshRenderer;

    [Header("Explosion Barrel")]
    [SerializeField]
    private GameObject explosionPrefab;
    [SerializeField]
    private float explosionDelayTime = 0.3f;
    [SerializeField]
    private float explosionRadius = 10.0f;
    [SerializeField]
    private float explosionForce = 1000.0f;


    [SerializeField]
    private float attackRange = 5;
    [SerializeField]
    private float attackRate = 3;

    private BoomEnemyState BoomenemyState = BoomEnemyState.None;

    private GameObject Player;

    private Status status;
    private NavMeshAgent navMeshAgent;
    private Transform target;
    private EnemyMemoryPool enemyMemoryPool;

    private float lastAttackTime = 0;
    public float attackTime;

    public void Setup(Transform target, EnemyMemoryPool enemyMemoryPool)
    {
        status = GetComponent<Status>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        
        this.target = target;
        this.enemyMemoryPool = enemyMemoryPool;

        navMeshAgent.updateRotation = false;
    }

    private void Update()
    {
    }

    private void OnEnable()
    {
        ChangeState(BoomEnemyState.Idle);
        Player = GameObject.Find("Player");

    }

    private void OnDisable()
    {
        StopCoroutine(BoomenemyState.ToString());
        BoomenemyState = BoomEnemyState.None;
    }

    public void ChangeState(BoomEnemyState newState)
    {
        //Debug.Log("changeState: " + newState + "     " + "enemyState: " + enemyState);

        if (BoomenemyState == newState) return;

        StopCoroutine(BoomenemyState.ToString());

        BoomenemyState = newState;

        StartCoroutine(BoomenemyState.ToString());
    }

    private IEnumerator Idle()
    {
        //Debug.Log("Idle");
        StartCoroutine("AutoChangeFromIdleToWander");

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

        ChangeState(BoomEnemyState.Wander);
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

        while (true)
        {
            currentTime += Time.deltaTime;

            to = new Vector3(navMeshAgent.destination.x, 0, navMeshAgent.destination.z);
            from = new Vector3(transform.position.x, 0, transform.position.z);
            if ((to - from).sqrMagnitude < 0.01f || currentTime >= maxTime)
            {
                ChangeState(BoomEnemyState.Idle);       
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

            Color color = meshRenderer.material.color;
            color.a = Mathf.Lerp(1, 0, Mathf.PingPong(Time.time * fadeSpeed, 1));
            meshRenderer.material.color = color;

            attackTime += Time.deltaTime;
            if (attackTime > attackRate)
            {
                Bounds bounds = GetComponentInChildren<Collider>().bounds;
                Instantiate(explosionPrefab, new Vector3(bounds.center.x, bounds.min.y, bounds.center.z), transform.rotation);

                Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

                foreach (Collider hit in colliders)
                {
                    PlayerController player = hit.GetComponent<PlayerController>();
                    if (player != null)
                    {
                        player.TakeDamage(50);
                        continue;
                    }

                    InteractionObject interaction = hit.GetComponent<InteractionObject>();
                    if (interaction != null)
                    {
                        interaction.TakeDamage(300);
                    }

                }
                attackTime = 0;
                Destroy(gameObject);
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

        if (distance <= attackRange) //타겟 거리가 공격범위안
        {
            ChangeState(BoomEnemyState.Attack);
        }

        else if (distance <= targetRecognitionRange)
        {
            ChangeState(BoomEnemyState.Pursuit);
        }
        else if (distance >= pursuitLimitRange)
        {
            ChangeState(BoomEnemyState.Wander);
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

        if (isDie == true)
        {
            enemyMemoryPool.DeactivateEnemy(gameObject, 1);

            Player.GetComponent<Status>().IncreaseGold(100);
            Player.GetComponent<Status>().IncreaseScore(1);
        }
    }
}


