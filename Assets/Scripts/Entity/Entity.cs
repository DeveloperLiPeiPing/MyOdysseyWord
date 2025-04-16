using UnityEngine;
using UnityEngine.Splines;

namespace Odyssey.Script
{
    /// <summary>
    /// 实体基类，提供基础物理和运动功能
    /// </summary>
    public abstract class Entity : MonoBehaviour
    {
        public EntityEvents entityEvents;

        protected Collider[] m_contactBuffer = new Collider[10]; // 接触检测缓冲区
        protected Collider[] m_penetrationBuffer = new Collider[32]; // 穿透检测缓冲区

        protected readonly float m_groundOffset = 0.1f; // 地面检测偏移量
        protected readonly float m_penetrationOffset = -0.1f; // 穿透检测偏移量
        protected readonly float m_slopingGroundAngle = 20f; // 斜坡判定角度

        /// <summary>
        /// 角色控制器组件
        /// </summary>
        public CharacterController controller { get; protected set; }

        /// <summary>
        /// 当前速度向量
        /// </summary>
        public Vector3 velocity { get; set; }

        /// <summary>
        /// 水平面(XZ)速度
        /// </summary>
        public Vector3 lateralVelocity
        {
            get { return new Vector3(velocity.x, 0, velocity.z); }
            set { velocity = new Vector3(value.x, velocity.y, value.z); }
        }

        /// <summary>
        /// 垂直(Y轴)速度
        /// </summary>
        public Vector3 verticalVelocity
        {
            get { return new Vector3(0, velocity.y, 0); }
            set { velocity = new Vector3(velocity.x, value.y, velocity.z); }
        }

        /// <summary>
        /// 上一帧的位置
        /// </summary>
        public Vector3 lastPosition { get; set; }

        /// <summary>
        /// 当前中心位置(包含碰撞体偏移)
        /// </summary>
        public Vector3 position => transform.position + center;

        /// <summary>
        /// 原始位置(忽略碰撞体尺寸变化)
        /// </summary>
        public Vector3 unsizedPosition => position - transform.up * height * 0.5f + transform.up * originalHeight * 0.5f;

        /// <summary>
        /// 考虑台阶高度的底部位置
        /// </summary>
        public Vector3 stepPosition => position - transform.up * (height * 0.5f - controller.stepOffset);

        /// <summary>
        /// 上一帧到当前帧的移动距离
        /// </summary>
        public float positionDelta { get; protected set; }

        /// <summary>
        /// 最后一次接触地面的时间
        /// </summary>
        public float lastGroundTime { get; protected set; }

        /// <summary>
        /// 是否在地面上
        /// </summary>
        public bool isGrounded { get; protected set; } = true;

        /// <summary>
        /// 是否在轨道上
        /// </summary>
        public bool onRails { get; set; }

        // 各种运动参数乘数
        public float accelerationMultiplier { get; set; } = 1f;
        public float gravityMultiplier { get; set; } = 1f;
        public float topSpeedMultiplier { get; set; } = 1f;
        public float turningDragMultiplier { get; set; } = 1f;
        public float decelerationMultiplier { get; set; } = 1f;

        /// <summary>
        /// 地面碰撞信息
        /// </summary>
        public RaycastHit groundHit;

        /// <summary>
        /// 当前附着的轨道
        /// </summary>
        public SplineContainer rails { get; protected set; }

        /// <summary>
        /// 当前地面角度
        /// </summary>
        public float groundAngle { get; protected set; }

        /// <summary>
        /// 地面法线
        /// </summary>
        public Vector3 groundNormal { get; protected set; }

        /// <summary>
        /// 局部斜坡方向
        /// </summary>
        public Vector3 localSlopeDirection { get; protected set; }

        /// <summary>
        /// 原始高度
        /// </summary>
        public float originalHeight { get; protected set; }

        /// <summary>
        /// 当前碰撞体高度
        /// </summary>
        public float height => controller.height;

        /// <summary>
        /// 碰撞体半径
        /// </summary>
        public float radius => controller.radius;

        /// <summary>
        /// 碰撞体中心点
        /// </summary>
        public Vector3 center => controller.center;

        // 组件引用
        protected CapsuleCollider m_collider;
        protected BoxCollider m_penetratorCollider;
        protected Rigidbody m_rigidbody;

        /// <summary>
        /// 判断点是否在台阶下方
        /// </summary>
        /// <param name="point">要检测的点</param>
        public virtual bool IsPointUnderStep(Vector3 point) => stepPosition.y > point.y;

        /// <summary>
        /// 是否在斜坡上
        /// </summary>
        public virtual bool OnSlopingGround()
        {
            if (isGrounded && groundAngle > m_slopingGroundAngle)
            {
                if (Physics.Raycast(transform.position, -transform.up, out var hit, height * 2f,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    return Vector3.Angle(hit.normal, Vector3.up) > m_slopingGroundAngle;
                else
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 调整碰撞体高度
        /// </summary>
        /// <param name="height">目标高度</param>
        public virtual void ResizeCollider(float height)
        {
            var delta = height - this.height;
            controller.height = height;
            controller.center += Vector3.up * delta * 0.5f;
        }

        // 各种物理检测方法
        public virtual bool CapsuleCast(Vector3 direction, float distance, int layer = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            return CapsuleCast(direction, distance, out _, layer, queryTriggerInteraction);
        }

        public virtual bool CapsuleCast(Vector3 direction, float distance,
            out RaycastHit hit, int layer = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            var origin = position - direction * radius + center;
            var offset = transform.up * (height * 0.5f - radius);
            var top = origin + offset;
            var bottom = origin - offset;
            return Physics.CapsuleCast(top, bottom, radius, direction,
                out hit, distance + radius, layer, queryTriggerInteraction);
        }

        public virtual bool SphereCast(Vector3 direction, float distance, int layer = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            return SphereCast(direction, distance, out _, layer, queryTriggerInteraction);
        }

        public virtual bool SphereCast(Vector3 direction, float distance,
            out RaycastHit hit, int layer = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            var castDistance = Mathf.Abs(distance - radius);
            return Physics.SphereCast(position, radius, direction,
                out hit, castDistance, layer, queryTriggerInteraction);
        }

        /// <summary>
        /// 执行重叠检测
        /// </summary>
        /// <param name="result">结果缓冲区</param>
        /// <param name="skinOffset">皮肤宽度偏移</param>
        public virtual int OverlapEntity(Collider[] result, float skinOffset = 0)
        {
            var contactOffset = skinOffset + controller.skinWidth + Physics.defaultContactOffset;
            var overlapsRadius = radius + contactOffset;
            var offset = (height + contactOffset) * 0.5f - overlapsRadius;
            var top = position + Vector3.up * offset;
            var bottom = position + Vector3.down * offset;
            return Physics.OverlapCapsuleNonAlloc(top, bottom, overlapsRadius, result);
        }

        /// <summary>
        /// 应用伤害(需子类实现)
        /// </summary>
        public virtual void ApplyDamage(int damage, Vector3 origin) { }
    }

    /// <summary>
    /// 泛型实体类，添加状态管理功能
    /// </summary>
    public abstract class Entity<T> : Entity where T : Entity<T>
    {
        protected IEntityContact[] m_contacts; // 接触监听器数组

        /// <summary>
        /// 实体状态管理器
        /// </summary>
        public EntityStateManager<T> states { get; protected set; }

        /// <summary>
        /// 初始化角色控制器
        /// </summary>
        protected virtual void InitializeController()
        {
            controller = GetComponent<CharacterController>();

            if (!controller)
            {
                controller = gameObject.AddComponent<CharacterController>();
            }

            controller.skinWidth = 0.005f;
            controller.minMoveDistance = 0;
            originalHeight = controller.height;
        }

        /// <summary>
        /// 初始化穿透检测碰撞体
        /// </summary>
        protected virtual void InitializePenetratorCollider()
        {
            var xzSize = radius * 2f - controller.skinWidth;
            m_penetratorCollider = gameObject.AddComponent<BoxCollider>();
            m_penetratorCollider.size = new Vector3(xzSize, height - controller.stepOffset, xzSize);
            m_penetratorCollider.center = center + Vector3.up * controller.stepOffset * 0.5f;
            m_penetratorCollider.isTrigger = true;
        }

        /// <summary>
        /// 初始化胶囊碰撞体
        /// </summary>
        protected virtual void InitializeCollider()
        {
            m_collider = gameObject.AddComponent<CapsuleCollider>();
            m_collider.height = controller.height;
            m_collider.radius = controller.radius;
            m_collider.center = controller.center;
            m_collider.isTrigger = true;
            m_collider.enabled = false;
        }

        /// <summary>
        /// 初始化刚体
        /// </summary>
        protected virtual void InitializeRigidbody()
        {
            m_rigidbody = gameObject.AddComponent<Rigidbody>();
            m_rigidbody.isKinematic = true;
        }

        /// <summary>
        /// 初始化状态管理器
        /// </summary>
        protected virtual void InitializeStateManager() => states = GetComponent<EntityStateManager<T>>();

        /// <summary>
        /// 处理状态更新
        /// </summary>
        protected virtual void HandleStates() => states.Step();

        /// <summary>
        /// 处理控制器移动
        /// </summary>
        protected virtual void HandleController()
        {
            if (controller.enabled)
            {
                controller.Move(velocity * Time.deltaTime);
                return;
            }

            transform.position += velocity * Time.deltaTime;
        }

        /// <summary>
        /// 处理轨道逻辑
        /// </summary>
        protected virtual void HandleSpline()
        {
            var distance = (height * 0.5f) + height * 0.5f;

            if (SphereCast(-transform.up, distance, out var hit) &&
                hit.collider.CompareTag(GameTags.InteractiveRail))
            {
                if (!onRails && verticalVelocity.y <= 0)
                {
                    EnterRail(hit.collider.GetComponent<SplineContainer>());
                }
            }
            else
            {
                ExitRail();
            }
        }

        /// <summary>
        /// 处理地面检测
        /// </summary>
        protected virtual void HandleGround()
        {
            if (onRails) return;

            var distance = (height * 0.5f) + m_groundOffset;

            if (SphereCast(Vector3.down, distance, out var hit) && verticalVelocity.y <= 0)
            {
                if (!isGrounded)
                {
                    if (EvaluateLanding(hit))
                    {
                        EnterGround(hit);
                    }
                    else
                    {
                        HandleHighLedge(hit);
                    }
                }
                else if (IsPointUnderStep(hit.point))
                {
                    UpdateGround(hit);

                    if (Vector3.Angle(hit.normal, Vector3.up) >= controller.slopeLimit)
                    {
                        HandleSlopeLimit(hit);
                    }
                }
                else
                {
                    HandleHighLedge(hit);
                }
            }
            else
            {
                ExitGround();
            }
        }

        /// <summary>
        /// 处理接触检测
        /// </summary>
        protected virtual void HandleContacts()
        {
            var overlaps = OverlapEntity(m_contactBuffer);

            for (int i = 0; i < overlaps; i++)
            {
                if (!m_contactBuffer[i].isTrigger && m_contactBuffer[i].transform != transform)
                {
                    OnContact(m_contactBuffer[i]);

                    var listeners = m_contactBuffer[i].GetComponents<IEntityContact>();

                    foreach (var contact in listeners)
                    {
                        contact.OnEntityContact((T)this);
                    }

                    if (m_contactBuffer[i].bounds.min.y > controller.bounds.max.y)
                    {
                        verticalVelocity = Vector3.Min(verticalVelocity, Vector3.zero);
                    }
                }
            }
        }

        /// <summary>
        /// 更新位置信息
        /// </summary>
        protected virtual void HandlePosition()
        {
            positionDelta = (position - lastPosition).magnitude;
            lastPosition = position;
        }

        /// <summary>
        /// 处理穿透问题
        /// </summary>
        protected virtual void HandlePenetration()
        {
            var xzSize = m_penetratorCollider.size.x * 0.5f;
            var ySize = (height - controller.stepOffset * 0.5f) * 0.5f;
            var origin = position + Vector3.up * controller.stepOffset * 0.5f;
            var halfExtents = new Vector3(xzSize, ySize, xzSize);
            var overlaps = Physics.OverlapBoxNonAlloc(origin, halfExtents, m_penetrationBuffer,
                Quaternion.identity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < overlaps; i++)
            {
                if (!m_penetrationBuffer[i].isTrigger && m_penetrationBuffer[i].transform != transform &&
                    (lateralVelocity.sqrMagnitude == 0 || m_penetrationBuffer[i].CompareTag(GameTags.Platform)))
                {
                    if (Physics.ComputePenetration(m_penetratorCollider, position, Quaternion.identity,
                        m_penetrationBuffer[i], m_penetrationBuffer[i].transform.position,
                        m_penetrationBuffer[i].transform.rotation, out var direction, out float distance))
                    {
                        var pushDirection = new Vector3(direction.x, 0, direction.z).normalized;
                        transform.position += pushDirection * distance;
                    }
                }
            }
        }

        // 各种状态转换方法
        protected virtual void EnterGround(RaycastHit hit)
        {
            if (!isGrounded)
            {
                groundHit = hit;
                isGrounded = true;
                entityEvents.OnGroundEnter?.Invoke();
            }
        }

        protected virtual void ExitGround()
        {
            if (isGrounded)
            {
                isGrounded = false;
                transform.parent = null;
                lastGroundTime = Time.time;
                verticalVelocity = Vector3.Max(verticalVelocity, Vector3.zero);
                entityEvents.OnGroundExit?.Invoke();
            }
        }

        protected virtual void EnterRail(SplineContainer rails)
        {
            if (!onRails)
            {
                onRails = true;
                this.rails = rails;
                entityEvents.OnRailsEnter.Invoke();
            }
        }

        public virtual void ExitRail()
        {
            if (onRails)
            {
                onRails = false;
                entityEvents.OnRailsExit.Invoke();
            }
        }

        protected virtual void UpdateGround(RaycastHit hit)
        {
            if (isGrounded)
            {
                groundHit = hit;
                groundNormal = groundHit.normal;
                groundAngle = Vector3.Angle(Vector3.up, groundHit.normal);
                localSlopeDirection = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
                transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
            }
        }

        /// <summary>
        /// 评估是否可以着陆
        /// </summary>
        protected virtual bool EvaluateLanding(RaycastHit hit)
        {
            return IsPointUnderStep(hit.point) && Vector3.Angle(hit.normal, Vector3.up) < controller.slopeLimit;
        }

        /// <summary>
        /// 处理斜坡限制(需子类实现)
        /// </summary>
        protected virtual void HandleSlopeLimit(RaycastHit hit) { }

        /// <summary>
        /// 处理高台阶情况(需子类实现)
        /// </summary>
        protected virtual void HandleHighLedge(RaycastHit hit) { }

        /// <summary>
        /// 更新逻辑(需子类实现)
        /// </summary>
        protected virtual void OnUpdate() { }

        /// <summary>
        /// 接触回调
        /// </summary>
        protected virtual void OnContact(Collider other)
        {
            if (other)
            {
                states.OnContact(other);
            }
        }

        // 运动控制方法
        /// <summary>
        /// 加速移动
        /// </summary>
        /// <param name="direction">移动方向</param>
        /// <param name="turningDrag">转向阻力</param>
        /// <param name="acceleration">加速度</param>
        /// <param name="topSpeed">最高速度</param>
        public virtual void Accelerate(Vector3 direction, float turningDrag, float acceleration, float topSpeed)
        {
            if (direction.sqrMagnitude > 0)
            {
                var speed = Vector3.Dot(direction, lateralVelocity);
                var velocity = direction * speed;
                var turningVelocity = lateralVelocity - velocity;
                var turningDelta = turningDrag * turningDragMultiplier * Time.deltaTime;
                var targetTopSpeed = topSpeed * topSpeedMultiplier;

                if (lateralVelocity.magnitude < targetTopSpeed || speed < 0)
                {
                    speed += acceleration * accelerationMultiplier * Time.deltaTime;
                    speed = Mathf.Clamp(speed, -targetTopSpeed, targetTopSpeed);
                }

                velocity = direction * speed;
                turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);
                lateralVelocity = velocity + turningVelocity;
            }
        }

        /// <summary>
        /// 减速停止
        /// </summary>
        /// <param name="deceleration">减速度</param>
        public virtual void Decelerate(float deceleration)
        {
            var delta = deceleration * decelerationMultiplier * Time.deltaTime;
            lateralVelocity = Vector3.MoveTowards(lateralVelocity, Vector3.zero, delta);
        }

        /// <summary>
        /// 应用重力
        /// </summary>
        /// <param name="gravity">重力强度</param>
        public virtual void Gravity(float gravity)
        {
            if (!isGrounded)
            {
                verticalVelocity += Vector3.down * gravity * gravityMultiplier * Time.deltaTime;
            }
        }

        /// <summary>
        /// 斜坡因子影响
        /// </summary>
        /// <param name="upwardForce">上坡力</param>
        /// <param name="downwardForce">下坡力</param>
        public virtual void SlopeFactor(float upwardForce, float downwardForce)
        {
            if (!isGrounded || !OnSlopingGround()) return;

            var factor = Vector3.Dot(Vector3.up, groundNormal);
            var downwards = Vector3.Dot(localSlopeDirection, lateralVelocity) > 0;
            var multiplier = downwards ? downwardForce : upwardForce;
            var delta = factor * multiplier * Time.deltaTime;
            lateralVelocity += localSlopeDirection * delta;
        }

        /// <summary>
        /// 吸附到地面
        /// </summary>
        /// <param name="force">吸附力度</param>
        public virtual void SnapToGround(float force)
        {
            if (isGrounded && (verticalVelocity.y <= 0))
            {
                verticalVelocity = Vector3.down * force;
            }
        }

        /// <summary>
        /// 立即转向
        /// </summary>
        /// <param name="direction">目标方向</param>
        public virtual void FaceDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude > 0)
            {
                var rotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = rotation;
            }
        }

        /// <summary>
        /// 平滑转向
        /// </summary>
        /// <param name="direction">目标方向</param>
        /// <param name="degreesPerSecond">转向速度(度/秒)</param>
        public virtual void FaceDirection(Vector3 direction, float degreesPerSecond)
        {
            if (direction != Vector3.zero)
            {
                var rotation = transform.rotation;
                var rotationDelta = degreesPerSecond * Time.deltaTime;
                var target = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(rotation, target, rotationDelta);
            }
        }

        /// <summary>
        /// 检测实体是否能放入指定位置
        /// </summary>
        /// <param name="position">目标位置</param>
        public virtual bool FitsIntoPosition(Vector3 position)
        {
            var bounds = controller.bounds;
            var radius = controller.radius - controller.skinWidth;
            var offset = height * 0.5f - radius;
            var top = position + Vector3.up * offset;
            var bottom = position - Vector3.up * offset;

            return !Physics.CheckCapsule(top, bottom, radius,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// 启用/禁用自定义碰撞
        /// </summary>
        /// <param name="value">是否启用</param>
        public virtual void UseCustomCollision(bool value)
        {
            controller.enabled = !value;

            if (value)
            {
                InitializeCollider();
                InitializeRigidbody();
            }
            else
            {
                Destroy(m_collider);
                Destroy(m_rigidbody);
            }
        }

        protected virtual void Awake()
        {
            InitializeController();
            InitializePenetratorCollider();
            InitializeStateManager();
        }

        protected virtual void Update()
        {
            if (controller.enabled || m_collider != null)
            {
                HandleStates();
                HandleController();
                HandleSpline();
                HandleGround();
                HandleContacts();
                OnUpdate();
            }
        }

        protected virtual void LateUpdate()
        {
            if (controller.enabled)
            {
                HandlePosition();
                HandlePenetration();
            }
        }
    }
}