using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CharacterMovement : MonoBehaviour
{
    public Vector3 pointA = new Vector3(0, 0, -5); // 原点
    public Vector3 pointB = new Vector3(0, 0, 0);  // 目标点
    public Vector3 pointA_Changed = new Vector3(-5, 0, -5); // 变更后的Point A
    public Vector3 pointB_Changed = new Vector3(-5, 0, 0);  // 变更后的Point B
    public float waitTimeAtB = 60f;  // 目标点B的停留时间
    public float waitTimeAtA = 8f;   // 原点A的停留时间
    public float speed = 3.5f;
    public Transform cube;           // 目标Cube的Transform
    public float rotationSpeed = 2f; // 旋转速度
    public Button changePositionButton; // 切换位置按钮
    public float changePositionWaitTime = 7f; // 切换位置后的等待时间

    private NavMeshAgent agent;
    private Animator animator;
    private bool movingToPointB = true;
    private float waitTimer;
    private bool isWaiting = false;
    private bool isPositionChanged = false; // 标记是否使用变更后的位置
    private bool changePositionTriggered = false; // 标记是否触发了位置变更
    private float changePositionTimer = 0f; // 位置变更后的计时器

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = speed;
        agent.SetDestination(pointB);
        animator.SetBool("isWalking", true);

        // 绑定切换位置按钮点击事件
        if (changePositionButton != null)
            changePositionButton.onClick.AddListener(ChangePosition);
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
                // 根据是否切换位置选择目标点
                agent.SetDestination(movingToPointB ? (isPositionChanged ? pointB_Changed : pointB) 
                                                    : (isPositionChanged ? pointA_Changed : pointA));
                animator.SetBool("isWalking", true); // 播放行走动画
                isWaiting = false;
            }
        }

        // 如果位置变更被触发，开始处理计时
        if (changePositionTriggered)
        {
            changePositionTimer += Time.deltaTime;
            if (changePositionTimer >= changePositionWaitTime)
            {
                // 计时结束，开始行走
                changePositionTriggered = false;
                movingToPointB = true; // 重置为移动到Point B的状态
                agent.SetDestination(pointB_Changed); // 设定新的目标为Point B_Changed
                animator.SetBool("isWalking", true); // 让模型开始行走
            }
        }
    }

    // 切换位置的方法
    void ChangePosition()
    {
        // 切换到新的位置
        if (!isPositionChanged)
        {
            isPositionChanged = true;
            changePositionTriggered = true; // 标记触发位置变更
            changePositionTimer = 0f; // 重置计时器
            agent.Warp(pointA_Changed); // 重置到Point A_Changed
            animator.SetBool("isWalking", false); // 停止行走动画，等待计时结束
        }
    }
}
