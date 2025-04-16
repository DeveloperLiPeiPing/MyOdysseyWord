using UnityEngine;

namespace Odyssey.Script
{
    [RequireComponent(typeof(PlayerInputManager))]
    [RequireComponent(typeof(PlayerStatsManager))]
    [RequireComponent(typeof(PlayerStateManager))]
    [RequireComponent(typeof(Health))]
    [AddComponentMenu("Player/Player")]
    public class Player : Entity<Player>
    {
        [Header("玩家事件")]
        public PlayerEvents playerEvents;

        [Header("可拾取物品插槽")]
        public Transform pickableSlot;

        [Header("玩家皮肤模型")]
        public Transform skin;

        protected Vector3 m_respawnPosition;  // 重生位置
        protected Quaternion m_respawnRotation; // 重生旋转角度

        protected Vector3 m_skinInitialPosition; // 皮肤初始位置
        protected Quaternion m_skinInitialRotation; // 皮肤初始旋转

        /// <summary>
        /// 玩家输入管理器
        /// </summary>
        public PlayerInputManager inputs { get; protected set; }

        /// <summary>
        /// 玩家状态管理器
        /// </summary>
        public PlayerStatsManager stats { get; protected set; }

        /// <summary>
        /// 玩家生命值组件
        /// </summary>
        public Health health { get; protected set; }

        /// <summary>
        /// 是否在水中
        /// </summary>
        public bool onWater { get; protected set; }

        /// <summary>
        /// 是否持有物品
        /// </summary>
        public bool holding { get; protected set; }

        /// <summary>
        /// 当前跳跃次数
        /// </summary>
        public int jumpCounter { get; protected set; }

        /// <summary>
        /// 空中旋转次数
        /// </summary>
        public int airSpinCounter { get; protected set; }

        /// <summary>
        /// 空中冲刺次数
        /// </summary>
        public int airDashCounter { get; protected set; }

        /// <summary>
        /// 最后一次冲刺时间
        /// </summary>
        public float lastDashTime { get; protected set; }

        /// <summary>
        /// 最后接触的墙面法线
        /// </summary>
        public Vector3 lastWallNormal { get; protected set; }

        /// <summary>
        /// 当前抓取的杆子
        /// </summary>
        public Pole pole { get; protected set; }

        /// <summary>
        /// 当前所在水域碰撞体
        /// </summary>
        public Collider water { get; protected set; }

        /// <summary>
        /// 当前持有的可拾取物品
        /// </summary>
        public Pickable pickable { get; protected set; }

        /// <summary>
        /// 玩家是否存活
        /// </summary>
        public virtual bool isAlive => !health.isEmpty;

        /// <summary>
        /// 是否可以站立
        /// </summary>
        public virtual bool canStandUp => !SphereCast(Vector3.up, originalHeight);

        // 离开水域时的偏移量
        protected const float k_waterExitOffset = 0.25f;

        #region 初始化方法
        protected virtual void InitializeInputs() => inputs = GetComponent<PlayerInputManager>();
        protected virtual void InitializeStats() => stats = GetComponent<PlayerStatsManager>();
        protected virtual void InitializeHealth() => health = GetComponent<Health>();
        protected virtual void InitializeTag() => tag = GameTags.Player;

        /// <summary>
        /// 初始化重生点
        /// </summary>
        protected virtual void InitializeRespawn()
        {
            m_respawnPosition = transform.position;
            m_respawnRotation = transform.rotation;
        }

        /// <summary>
        /// 初始化皮肤模型
        /// </summary>
        protected virtual void InitializeSkin()
        {
            if (skin)
            {
                m_skinInitialPosition = skin.localPosition;
                m_skinInitialRotation = skin.localRotation;
            }
        }
        #endregion

        #region 核心功能
        /// <summary>
        /// 重置玩家状态、生命值、位置和旋转
        /// </summary>
        public virtual void Respawn()
        {
            health.Reset();
            transform.SetPositionAndRotation(m_respawnPosition, m_respawnRotation);
            states.Change<IdlePlayerState>();
        }

        /// <summary>
        /// 设置重生点位置和旋转
        /// </summary>
        public virtual void SetRespawn(Vector3 position, Quaternion rotation)
        {
            m_respawnPosition = position;
            m_respawnRotation = rotation;
        }

        /// <summary>
        /// 对玩家造成伤害
        /// </summary>
        /// <param name="amount">伤害值</param>
        /// <param name="origin">伤害来源位置</param>
        public override void ApplyDamage(int amount, Vector3 origin)
        {
            if (!health.isEmpty && !health.recovering)
            {
                health.Damage(amount);
                var damageDir = origin - transform.position;
                damageDir.y = 0;
                damageDir = damageDir.normalized;
                FaceDirection(damageDir);
                lateralVelocity = -transform.forward * stats.current.hurtBackwardsForce;

                if (!onWater)
                {
                    verticalVelocity = Vector3.up * stats.current.hurtUpwardForce;
                    states.Change<HurtPlayerState>();
                }

                playerEvents.OnHurt?.Invoke();

                if (health.isEmpty)
                {
                    Throw();
                    playerEvents.OnDie?.Invoke();
                }
            }
        }

        /// <summary>
        /// 杀死玩家
        /// </summary>
        public virtual void Die()
        {
            health.Set(0);
            playerEvents.OnDie?.Invoke();
        }
        #endregion

        #region 环境交互
        /// <summary>
        /// 进入水域
        /// </summary>
        /// <param name="water">水域碰撞体</param>
        public virtual void EnterWater(Collider water)
        {
            if (!onWater && !health.isEmpty)
            {
                Throw();
                onWater = true;
                this.water = water;
                states.Change<SwimPlayerState>();
            }
        }

        /// <summary>
        /// 离开水域
        /// </summary>
        public virtual void ExitWater()
        {
            if (onWater)
            {
                onWater = false;
            }
        }

        /// <summary>
        /// 抓取杆子
        /// </summary>
        /// <param name="other">杆子碰撞体</param>
        public virtual void GrabPole(Collider other)
        {
            if (stats.current.canPoleClimb && velocity.y <= 0
                && !holding && other.TryGetComponent(out Pole pole))
            {
                this.pole = pole;
                states.Change<PoleClimbingPlayerState>();
            }
        }
        #endregion

        #region 移动控制
        /// <summary>
        /// 在给定方向上加速移动
        /// </summary>
        /// <param name="direction">移动方向</param>
        public virtual void Accelerate(Vector3 direction)
        {
            var turningDrag = isGrounded && inputs.GetRun() ? stats.current.runningTurningDrag : stats.current.turningDrag;
            var acceleration = isGrounded && inputs.GetRun() ? stats.current.runningAcceleration : stats.current.acceleration;
            var finalAcceleration = isGrounded ? acceleration : stats.current.airAcceleration;
            var topSpeed = inputs.GetRun() ? stats.current.runningTopSpeed : stats.current.topSpeed;

            Accelerate(direction, turningDrag, finalAcceleration, topSpeed);

            if (inputs.GetRunUp())
            {
                lateralVelocity = Vector3.ClampMagnitude(lateralVelocity, topSpeed);
            }
        }

        /// <summary>
        /// 根据输入方向加速移动(考虑相机方向)
        /// </summary>
        public virtual void AccelerateToInputDirection()
        {
            var inputDirection = inputs.GetMovementCameraDirection();
            Accelerate(inputDirection);
        }

        /// <summary>
        /// 应用标准坡度系数
        /// </summary>
        public virtual void RegularSlopeFactor()
        {
            if (stats.current.applySlopeFactor)
                SlopeFactor(stats.current.slopeUpwardForce, stats.current.slopeDownwardForce);
        }

        /// <summary>
        /// 水中移动
        /// </summary>
        /// <param name="direction">移动方向</param>
        public virtual void WaterAcceleration(Vector3 direction) =>
            Accelerate(direction, stats.current.waterTurningDrag, stats.current.swimAcceleration, stats.current.swimTopSpeed);

        /// <summary>
        /// 爬行移动
        /// </summary>
        /// <param name="direction">移动方向</param>
        public virtual void CrawlingAccelerate(Vector3 direction) =>
            Accelerate(direction, stats.current.crawlingTurningSpeed, stats.current.crawlingAcceleration, stats.current.crawlingTopSpeed);

        /// <summary>
        /// 后空翻移动
        /// </summary>
        public virtual void BackflipAcceleration()
        {
            var direction = inputs.GetMovementCameraDirection();
            Accelerate(direction, stats.current.backflipTurningDrag, stats.current.backflipAirAcceleration, stats.current.backflipTopSpeed);
        }

        /// <summary>
        /// 减速
        /// </summary>
        public virtual void Decelerate() => Decelerate(stats.current.deceleration);

        /// <summary>
        /// 摩擦减速
        /// </summary>
        public virtual void Friction()
        {
            if (OnSlopingGround())
                Decelerate(stats.current.slopeFriction);
            else
                Decelerate(stats.current.friction);
        }

        /// <summary>
        /// 应用重力
        /// </summary>
        public virtual void Gravity()
        {
            if (!isGrounded && verticalVelocity.y > -stats.current.gravityTopSpeed)
            {
                var speed = verticalVelocity.y;
                var force = verticalVelocity.y > 0 ? stats.current.gravity : stats.current.fallGravity;
                speed -= force * gravityMultiplier * Time.deltaTime;
                speed = Mathf.Max(speed, -stats.current.gravityTopSpeed);
                verticalVelocity = new Vector3(0, speed, 0);
            }
        }

        /// <summary>
        /// 吸附到地面
        /// </summary>
        public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);

        /// <summary>
        /// 平滑转向
        /// </summary>
        /// <param name="direction">目标方向</param>
        public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction, stats.current.rotationSpeed);

        /// <summary>
        /// 水中转向
        /// </summary>
        /// <param name="direction">目标方向</param>
        public virtual void WaterFaceDirection(Vector3 direction) => FaceDirection(direction, stats.current.waterRotationSpeed);
        #endregion

        #region 跳跃系统
        /// <summary>
        /// 进入下落状态
        /// </summary>
        public virtual void Fall()
        {
            if (!isGrounded)
            {
                states.Change<FallPlayerState>();
            }
        }

        /// <summary>
        /// 处理跳跃输入
        /// </summary>
        public virtual void Jump()
        {
            var canMultiJump = (jumpCounter > 0) && (jumpCounter < stats.current.multiJumps);
            var canCoyoteJump = (jumpCounter == 0) && (Time.time < lastGroundTime + stats.current.coyoteJumpThreshold);
            var holdJump = !holding || stats.current.canJumpWhileHolding;

            if ((isGrounded || onRails || canMultiJump || canCoyoteJump) && holdJump)
            {
                if (inputs.GetJumpDown())
                {
                    Jump(stats.current.maxJumpHeight);
                }
            }

            if (inputs.GetJumpUp() && (jumpCounter > 0) && (verticalVelocity.y > stats.current.minJumpHeight))
            {
                verticalVelocity = Vector3.up * stats.current.minJumpHeight;
            }
        }

        /// <summary>
        /// 执行跳跃
        /// </summary>
        /// <param name="height">跳跃高度</param>
        public virtual void Jump(float height)
        {
            jumpCounter++;
            verticalVelocity = Vector3.up * height;
            states.Change<FallPlayerState>();
            playerEvents.OnJump?.Invoke();
        }

        /// <summary>
        /// 方向性跳跃
        /// </summary>
        /// <param name="direction">跳跃方向</param>
        /// <param name="height">垂直高度</param>
        /// <param name="distance">水平距离</param>
        public virtual void DirectionalJump(Vector3 direction, float height, float distance)
        {
            jumpCounter++;
            verticalVelocity = Vector3.up * height;
            lateralVelocity = direction * distance;
            playerEvents.OnJump?.Invoke();
        }
        #endregion

        #region 技能系统
        /// <summary>
        /// 重置空中冲刺次数
        /// </summary>
        public virtual void ResetAirDash() => airDashCounter = 0;

        /// <summary>
        /// 重置跳跃次数
        /// </summary>
        public virtual void ResetJumps() => jumpCounter = 0;

        /// <summary>
        /// 设置跳跃次数
        /// </summary>
        /// <param name="amount">跳跃次数</param>
        public virtual void SetJumps(int amount) => jumpCounter = amount;

        /// <summary>
        /// 重置空中旋转次数
        /// </summary>
        public virtual void ResetAirSpins() => airSpinCounter = 0;

        /// <summary>
        /// 执行旋转攻击
        /// </summary>
        public virtual void Spin()
        {
            var canAirSpin = (isGrounded || stats.current.canAirSpin) && airSpinCounter < stats.current.allowedAirSpins;

            if (stats.current.canSpin && canAirSpin && !holding && inputs.GetSpinDown())
            {
                if (!isGrounded)
                {
                    airSpinCounter++;
                }

                states.Change<SpinPlayerState>();
                playerEvents.OnSpin?.Invoke();
            }
        }

        /// <summary>
        /// 拾取/投掷物品
        /// </summary>
        public virtual void PickAndThrow()
        {
            if (stats.current.canPickUp && inputs.GetPickAndDropDown())
            {
                if (!holding)
                {
                    if (CapsuleCast(transform.forward,
                        stats.current.pickDistance, out var hit))
                    {
                        if (hit.transform.TryGetComponent(out Pickable pickable))
                        {
                            PickUp(pickable);
                        }
                    }
                }
                else
                {
                    Throw();
                }
            }
        }

        /// <summary>
        /// 拾取物品
        /// </summary>
        /// <param name="pickable">可拾取物品</param>
        public virtual void PickUp(Pickable pickable)
        {
            if (!holding && (isGrounded || stats.current.canPickUpOnAir))
            {
                holding = true;
                this.pickable = pickable;
                pickable.PickUp(pickableSlot);
                pickable.onRespawn.AddListener(RemovePickable);
                playerEvents.OnPickUp?.Invoke();
            }
        }

        /// <summary>
        /// 投掷物品
        /// </summary>
        public virtual void Throw()
        {
            if (holding)
            {
                var force = lateralVelocity.magnitude * stats.current.throwVelocityMultiplier;
                pickable.Release(transform.forward, force);
                pickable = null;
                holding = false;
                playerEvents.OnThrow?.Invoke();
            }
        }

        /// <summary>
        /// 移除持有的物品
        /// </summary>
        public virtual void RemovePickable()
        {
            if (holding)
            {
                pickable = null;
                holding = false;
            }
        }

        /// <summary>
        /// 空中俯冲
        /// </summary>
        public virtual void AirDive()
        {
            if (stats.current.canAirDive && !isGrounded && !holding && inputs.GetAirDiveDown())
            {
                states.Change<AirDivePlayerState>();
                playerEvents.OnAirDive?.Invoke();
            }
        }

        /// <summary>
        /// 下坠攻击
        /// </summary>
        public virtual void StompAttack()
        {
            if (!isGrounded && !holding && stats.current.canStompAttack && inputs.GetStompDown())
            {
                states.Change<StompPlayerState>();
            }
        }

        /// <summary>
        /// 抓取边缘
        /// </summary>
        public virtual void LedgeGrab()
        {
            if (stats.current.canLedgeHang && velocity.y < 0 && !holding &&
                states.ContainsStateOfType(typeof(LedgeHangingPlayerState)) &&
                DetectingLedge(stats.current.ledgeMaxForwardDistance, stats.current.ledgeMaxDownwardDistance, out var hit))
            {
                if (!(hit.collider is CapsuleCollider) && !(hit.collider is SphereCollider))
                {
                    var ledgeDistance = radius + stats.current.ledgeMaxForwardDistance;
                    var lateralOffset = transform.forward * ledgeDistance;
                    var verticalOffset = Vector3.down * height * 0.5f - center;
                    velocity = Vector3.zero;
                    transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
                    transform.position = hit.point - lateralOffset + verticalOffset;
                    states.Change<LedgeHangingPlayerState>();
                    playerEvents.OnLedgeGrabbed?.Invoke();
                }
            }
        }

        /// <summary>
        /// 后空翻
        /// </summary>
        /// <param name="force">后空翻力度</param>
        public virtual void Backflip(float force)
        {
            if (stats.current.canBackflip && !holding)
            {
                verticalVelocity = Vector3.up * stats.current.backflipJumpHeight;
                lateralVelocity = -transform.forward * force;
                states.Change<BackflipPlayerState>();
                playerEvents.OnBackflip.Invoke();
            }
        }

        /// <summary>
        /// 冲刺
        /// </summary>
        public virtual void Dash()
        {
            var canAirDash = stats.current.canAirDash && !isGrounded &&
                airDashCounter < stats.current.allowedAirDashes;
            var canGroundDash = stats.current.canGroundDash && isGrounded &&
                Time.time - lastDashTime > stats.current.groundDashCoolDown;

            if (inputs.GetDashDown() && (canAirDash || canGroundDash))
            {
                if (!isGrounded) airDashCounter++;

                lastDashTime = Time.time;
                states.Change<DashPlayerState>();
            }
        }

        /// <summary>
        /// 滑翔
        /// </summary>
        public virtual void Glide()
        {
            if (!isGrounded && inputs.GetGlide() &&
                verticalVelocity.y <= 0 && stats.current.canGlide)
                states.Change<GlidingPlayerState>();
        }
        #endregion

        #region 其他功能
        /// <summary>
        /// 设置皮肤父对象
        /// </summary>
        /// <param name="parent">目标父对象</param>
        public virtual void SetSkinParent(Transform parent)
        {
            if (skin)
            {
                skin.parent = parent;
            }
        }

        /// <summary>
        /// 重置皮肤位置和旋转
        /// </summary>
        public virtual void ResetSkinParent()
        {
            if (skin)
            {
                skin.parent = transform;
                skin.localPosition = m_skinInitialPosition;
                skin.localRotation = m_skinInitialRotation;
            }
        }

        /// <summary>
        /// 墙面滑行
        /// </summary>
        /// <param name="other">墙面碰撞体</param>
        public virtual void WallDrag(Collider other)
        {
            if (stats.current.canWallDrag && velocity.y <= 0 &&
                !holding && !other.TryGetComponent<Rigidbody>(out _))
            {
                if (CapsuleCast(transform.forward, 0.25f, out var hit,
                    stats.current.wallDragLayers) && !DetectingLedge(0.25f, height, out _))
                {
                    if (hit.collider.CompareTag(GameTags.Platform))
                        transform.parent = hit.transform;

                    lastWallNormal = hit.normal;
                    states.Change<WallDragPlayerState>();
                }
            }
        }

        /// <summary>
        /// 推动刚体
        /// </summary>
        /// <param name="other">可推动物体的碰撞体</param>
        public virtual void PushRigidbody(Collider other)
        {
            if (!IsPointUnderStep(other.bounds.max) &&
                other.TryGetComponent(out Rigidbody rigidbody))
            {
                var force = lateralVelocity * stats.current.pushForce;
                rigidbody.velocity += force / rigidbody.mass * Time.deltaTime;
            }
        }
        #endregion

        #region Unity回调
        protected override void Awake()
        {
            base.Awake();
            InitializeInputs();
            InitializeStats();
            InitializeHealth();
            InitializeTag();
            InitializeRespawn();

            // 地面接触事件
            entityEvents.OnGroundEnter.AddListener(() =>
            {
                ResetJumps();
                ResetAirSpins();
                ResetAirDash();
            });

            // 轨道接触事件
            entityEvents.OnRailsEnter.AddListener(() =>
            {
                ResetJumps();
                ResetAirSpins();
                ResetAirDash();
                StartGrind();
            });
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            if (other.CompareTag(GameTags.VolumeWater))
            {
                if (!onWater && other.bounds.Contains(unsizedPosition))
                {
                    EnterWater(other);
                }
                else if (onWater)
                {
                    var exitPoint = position + Vector3.down * k_waterExitOffset;

                    if (!other.bounds.Contains(exitPoint))
                    {
                        ExitWater();
                    }
                }
            }
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 检测边缘
        /// </summary>
        protected virtual bool DetectingLedge(float forwardDistance, float downwardDistance, out RaycastHit ledgeHit)
        {
            var contactOffset = Physics.defaultContactOffset + positionDelta;
            var ledgeMaxDistance = radius + forwardDistance;
            var ledgeHeightOffset = height * 0.5f + contactOffset;
            var upwardOffset = transform.up * ledgeHeightOffset;
            var forwardOffset = transform.forward * ledgeMaxDistance;

            Debug.DrawRay(position + upwardOffset, transform.forward, Color.green, 0.2f);
            Debug.DrawRay(position + forwardOffset * .01f, transform.up, Color.blue, 0.2f);

            if (Physics.Raycast(position + upwardOffset, transform.forward, ledgeMaxDistance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) ||
                Physics.Raycast(position + forwardOffset * .01f, transform.up, ledgeHeightOffset,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                ledgeHit = new RaycastHit();
                return false;
            }

            var origin = position + upwardOffset + forwardOffset;
            var distance = downwardDistance + contactOffset;
            Debug.DrawRay(origin, Vector3.down * distance, Color.red, 0.1f);
            return Physics.Raycast(origin, Vector3.down, out ledgeHit, distance,
                stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// 开始滑轨
        /// </summary>
        public virtual void StartGrind() => states.Change<RailGrindPlayerState>();
        #endregion
    }
}