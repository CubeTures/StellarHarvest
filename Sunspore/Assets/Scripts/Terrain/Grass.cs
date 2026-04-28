using UnityEngine;
using UnityEngine.AI;

namespace Sunspore
{
    public class Grass : MonoBehaviour
    {
        [SerializeField]
        float swayTriggerRadius = 0.42f;

        [SerializeField]
        float maxSwayDegrees = 14f;

        [SerializeField]
        float swayFrequency = 10f;

        [SerializeField]
        float blendInSpeed = 6f;

        [SerializeField]
        float blendOutSpeed = 2.5f;

        [SerializeField]
        float minHorizontalSpeed = 0.25f;

        SpriteRenderer _spriteRenderer;
        Vector3 _baseEuler;
        float _swayBlend;

        bool _movingWalkerThisPhysicsStep;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sortingOrder = (int)(-transform.position.z * 100) - 50 * 100;
            _baseEuler = transform.localEulerAngles;

            EnsureSwayTrigger();
        }

        void EnsureSwayTrigger()
        {
            foreach (var c in GetComponents<Collider>())
            {
                if (c.isTrigger)
                    return;
            }

            var sphere = gameObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = swayTriggerRadius;
            sphere.center = Vector3.zero;
        }

        static bool IsWalker(Collider c)
        {
            return c.GetComponentInParent<Player>() != null
                || c.GetComponentInParent<Enemy>() != null;
        }

        /// <summary>XZ horizontal speed – ignores idle jitter.</summary>
        static float HorizontalPlaneSpeed(Collider other)
        {
            var rb =
                other.attachedRigidbody != null
                    ? other.attachedRigidbody
                    : other.GetComponentInParent<Rigidbody>();
            if (rb != null)
            {
                Vector3 v = rb.linearVelocity;
                v.y = 0f;
                return v.magnitude;
            }

            NavMeshAgent agent = other.GetComponentInParent<NavMeshAgent>();
            if (agent != null && agent.enabled)
            {
                Vector3 v = agent.velocity;
                v.y = 0f;
                return v.magnitude;
            }

            return 0f;
        }

        void OnTriggerStay(Collider other)
        {
            if (!IsWalker(other))
                return;
            if (HorizontalPlaneSpeed(other) < minHorizontalSpeed)
                return;
            _movingWalkerThisPhysicsStep = true;
        }

        void LateUpdate()
        {
            float target = _movingWalkerThisPhysicsStep ? 1f : 0f;
            _movingWalkerThisPhysicsStep = false;

            float speed = target > _swayBlend ? blendInSpeed : blendOutSpeed;
            _swayBlend = Mathf.MoveTowards(_swayBlend, target, Time.deltaTime * speed);

            float sway = Mathf.Sin(Time.time * swayFrequency) * maxSwayDegrees * _swayBlend;
            transform.localEulerAngles = new Vector3(
                _baseEuler.x,
                _baseEuler.y,
                _baseEuler.z + sway
            );
        }

        public void DestroyGrass()
        {
            SFXManager.Play("cutgrass");
            Destroy(gameObject);
        }
    }
}
