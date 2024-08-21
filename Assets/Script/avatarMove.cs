using UnityEngine;
using UnityEngine.AI;

public class CharacterMovement : MonoBehaviour
{
    public Vector3 pointA = new Vector3(0, 0, -5); // 原点
    public Vector3 pointB = new Vector3(0, 0, 0); // 目标点
    public float waitTimeAtB = 60f; // 目标点B的停留时间
    public float waitTimeAtA = 8f;  // 原点A的停留时间
    public float speed = 3.5f;
    public Transform cube; // 目标Cube的Transform
    public float rotationSpeed = 2f; // 旋转速度

    private NavMeshAgent agent;
    private Animator animator;
    private bool movingToPointB = true;
    private float waitTimer;
    private bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = speed;
        agent.SetDestination(pointB);
        animator.SetBool("isWalking", true);
    }

    void Update()
    {
        if (!isWaiting && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // 到达目的地，开始等待
            isWaiting = true;
            waitTimer = movingToPointB ? waitTimeAtB : waitTimeAtA; // 根据位置设置等待时间
            animator.SetBool("isWalking", false); // 停止行走动画
        }

        if (isWaiting)
        {
            if (movingToPointB && cube != null)
            {
                // 逐渐转向Cube物体
                Vector3 direction = (cube.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }

            // 处理等待计时
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                // 等待结束，继续移动
                movingToPointB = !movingToPointB;
                agent.SetDestination(movingToPointB ? pointB : pointA);
                animator.SetBool("isWalking", true); // 播放行走动画
                isWaiting = false;
            }
        }
    }
}

