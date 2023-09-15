using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Diwide.Topdown
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private ProjectileController bulletPrefab;
        
        [SerializeField, Range(1f, 10f)] 
        private float moveSpeed = 2f;
        [SerializeField, Range(.5f, 5f)] 
        private float maxSpeed = 2f;
        [SerializeField] 
        private bool isFirstPlayer = true;
        [SerializeField, Range(.1f, 1f)] 
        private float attackDelay = .1f;
        [SerializeField, Range(.1f, 1f)] 
        private float rotateDelay = .25f;

        [SerializeField] private Vector3 firePoint;
        
        [Range(1f, 30f)]
        public float health = 5f;
        
        private PlayerControls _controls;

        private InputActionMap _actionMap;

        private Rigidbody _rigidbody;
        
        void Awake()
        {
            _controls = new PlayerControls();
            _actionMap = (isFirstPlayer) ? _controls.Player1 : _controls.Player2;
        }

        private void OnEnable() => _actionMap.Enable();
        private void OnDisable() => _actionMap.Disable();

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();

            StartCoroutine(Fire());
            StartCoroutine(Focus());
        }

        private IEnumerator Fire()
        {
            while (true)
            {
                var bullet = Instantiate(bulletPrefab, transform.TransformPoint(firePoint), transform.rotation);
                bullet.Parent = gameObject;
                yield return new WaitForSeconds(attackDelay);
            }
        }

        private IEnumerator Focus()
        {
            while (true)
            {
                transform.LookAt(target);
                transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
                yield return new WaitForSeconds(rotateDelay);
            }
        }

        private void FixedUpdate()
        {
            Vector2 moveVector = _actionMap.FindAction("Move").ReadValue<Vector2>();
            
            if (moveVector == Vector2.zero) return;

            var velocity = _rigidbody.velocity;
            velocity += new Vector3(moveVector.x, 0, moveVector.y) * moveSpeed * Time.fixedDeltaTime;
            velocity.y = 0;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

            _rigidbody.velocity = velocity;
        }

        private void OnTriggerEnter(Collider other)
        {
            var bullet = other.GetComponent<ProjectileController>();
            if (bullet == null || bullet.Parent == gameObject) return;

            health -= bullet.Damage;
            Destroy(other.gameObject);
            if (health <= 0f) Debug.Log($"Player with name {name} is dead");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(firePoint, .2f);
        }
    }
}
