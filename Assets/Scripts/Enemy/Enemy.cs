using UnityEngine;

namespace Odyssey.Script
{
    [RequireComponent(typeof(EnemyStatsManager))]
    [RequireComponent(typeof(EnemyStateManager))]
    [RequireComponent(typeof(WaypointManager))]
    [RequireComponent(typeof(Health))]
    [AddComponentMenu("Enemy/敌人实体")]
    public class Enemy : Entity<Enemy>
    {
        [Header("敌人事件")]
        public EnemyEvents enemyEvents;

        protected Player m_player;  // 玩家实例缓存

        // 视野检测和接触攻击的碰撞体缓存数组
        protected Collider[] m_sightOverlaps = new Collider[1024];
        protected Collider[] m_contactAttackOverlaps = new Collider[1024];

        /// <summary>
        /// 敌人状态管理器
        /// </summary>
        public EnemyStatsManager stats { get; protected set; }

        /// <summary>
        /// 路径点管理器
        /// </summary>
        public WaypointManager waypoints { get; protected set; }

        /// <summary>
        /// 生命值组件
        /// </summary>
        public Health health { get; protected set; }

        /// <summary>
        /// 当前锁定的玩家实例
        /// </summary>
        public Player player { get; protected set; }

        #region 初始化方法
        /// <summary>
        /// 初始化状态管理器
        /// </summary>
        protected virtual void InitializeStatsManager() => stats = GetComponent<EnemyStatsManager>();

        /// <summary>
        /// 初始化路径点管理器
        /// </summary>
        protected virtual void InitializeWaypointsManager() => waypoints = GetComponent<WaypointManager>();

        /// <summary>
        /// 初始化生命值组件
        /// </summary>
        protected virtual void InitializeHealth() => health = GetComponent<Health>();

        /// <summary>
        /// 设置敌人标签
        /// </summary>
        protected virtual void InitializeTag() => tag = GameTags.Enemy;
        #endregion

        #region 生命系统
        /// <summary>
        /// 对敌人造成伤害
        /// </summary>
        /// <param name="amount">伤害值</param>
        /// <param name="origin">伤害来源位置</param>
        public override void ApplyDamage(int amount, Vector3 origin)
        {
            if (!health.isEmpty && !health.recovering)
            {
                health.Damage(amount);
                enemyEvents.OnDamage?.Invoke();

                if (health.isEmpty)
                {
                    controller.enabled = false;  // 禁用控制器
                    enemyEvents.OnDie?.Invoke();
                }
            }
        }

        /// <summary>
        /// 复活敌人
        /// </summary>
        public virtual void Revive()
        {
            if (!health.isEmpty) return;

            health.Reset();
            controller.enabled = true;  // 重新启用控制器
            enemyEvents.OnRevive.Invoke();
        }
        #endregion

        #region 移动控制
        /// <summary>
        /// 沿指定方向加速移动
        /// </summary>
        /// <param name="direction">移动方向</param>
        /// <param name="acceleration">加速度</param>
        /// <param name="topSpeed">最高速度</param>
        public virtual void Accelerate(Vector3 direction, float acceleration, float topSpeed) =>
            Accelerate(direction, stats.current.turningDrag, acceleration, topSpeed);

        /// <summary>
        /// 减速移动
        /// </summary>
        public virtual void Decelerate() => Decelerate(stats.current.deceleration);

        /// <summary>
        /// 应用摩擦力减速
        /// </summary>
        public virtual void Friction() => Decelerate(stats.current.friction);

        /// <summary>
        /// 应用重力
        /// </summary>
        public virtual void Gravity() => Gravity(stats.current.gravity);

        /// <summary>
        /// 地面吸附效果
        /// </summary>
        public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);

        /// <summary>
        /// 平滑转向指定方向
        /// </summary>
        /// <param name="direction">目标方向</param>
        public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction, stats.current.rotationSpeed);
        #endregion

        #region 战斗系统
        /// <summary>
        /// 接触攻击检测
        /// </summary>
        public virtual void ContactAttack()
        {
            if (stats.current.canAttackOnContact)
            {
                // 检测范围内的碰撞体
                var overlaps = OverlapEntity(m_contactAttackOverlaps, stats.current.contactOffset);

                for (int i = 0; i < overlaps; i++)
                {
                    if (m_contactAttackOverlaps[i].CompareTag(GameTags.Player) &&
                        m_contactAttackOverlaps[i].TryGetComponent<Player>(out var player))
                    {
                        // 计算踩踏判定点
                        var stepping = controller.bounds.max + Vector3.down * stats.current.contactSteppingTolerance;

                        // 如果玩家不在踩踏点上
                        if (!player.IsPointUnderStep(stepping))
                        {
                            // 执行击退效果
                            if (stats.current.contactPushback)
                            {
                                lateralVelocity = -transform.forward * stats.current.contactPushBackForce;
                            }

                            // 对玩家造成伤害
                            player.ApplyDamage(stats.current.contactDamage, transform.position);
                            enemyEvents.OnPlayerContact?.Invoke();
                        }
                    }
                }
            }
        }
        #endregion

        #region 感知系统
        /// <summary>
        /// 处理敌人视野逻辑
        /// </summary>
        protected virtual void HandleSight()
        {
            if (!player)
            {
                // 在发现范围内检测玩家
                var overlaps = Physics.OverlapSphereNonAlloc(position, stats.current.spotRange, m_sightOverlaps);

                for (int i = 0; i < overlaps; i++)
                {
                    if (m_sightOverlaps[i].CompareTag(GameTags.Player))
                    {
                        if (m_sightOverlaps[i].TryGetComponent<Player>(out var player))
                        {
                            this.player = player;
                            enemyEvents.OnPlayerSpotted?.Invoke();
                            return;
                        }
                    }
                }
            }
            else
            {
                // 计算与玩家的距离
                var distance = Vector3.Distance(position, player.position);

                // 如果玩家死亡或超出视野范围
                if ((player.health.current == 0) || (distance > stats.current.viewRange))
                {
                    player = null;
                    enemyEvents.OnPlayerScaped?.Invoke();
                }
            }
        }
        #endregion

        #region Unity生命周期
        /// <summary>
        /// 每帧更新敌人逻辑
        /// </summary>
        protected override void OnUpdate()
        {
            HandleSight();      // 视野检测
            ContactAttack();    // 接触攻击
        }

        /// <summary>
        /// 初始化敌人组件
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            InitializeTag();             // 初始化标签
            InitializeStatsManager();    // 初始化状态管理器
            InitializeWaypointsManager();// 初始化路径点管理器
            InitializeHealth();          // 初始化生命值组件
        }
        #endregion
    }
}