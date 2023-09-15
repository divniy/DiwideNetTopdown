using System.Collections;
using UnityEngine;

namespace Diwide.Topdown
{
    public class ProjectileController : MonoBehaviour
    {
        [SerializeField, Range(1f, 10f)] 
        private float _moveSpeed = 3f;
        [SerializeField, Range(1f, 10f)] 
        private float _damage = 1f;
        [SerializeField, Range(1f, 15f)] 
        private float _lifetime = 7f;

        public float Damage => _damage;
        public GameObject Parent { get; set; }
        
        void Start()
        {
            StartCoroutine(OnDieCoroutine());
        }

        void Update()
        {
            transform.position += transform.forward * (_moveSpeed * Time.deltaTime);
        }

        private IEnumerator OnDieCoroutine()
        {
            yield return new WaitForSeconds(_lifetime);
            Destroy(gameObject);
        }
    }
}
