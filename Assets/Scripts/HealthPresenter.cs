using UniRx;
using System;
using UnityEngine;

namespace Diwide.Topdown
{
    [RequireComponent(typeof(MeshRenderer))]
    public class HealthPresenter : MonoBehaviour
    {
        private static readonly int HealthID = Shader.PropertyToID("_Health");
        
        [SerializeField] private PlayerController player;
        
        private MeshRenderer _renderer;
        private MaterialPropertyBlock _block;

        private void Start()
        {
            _renderer = GetComponent<MeshRenderer>();
            _block = new MaterialPropertyBlock();
            if (player == null) player = GetComponentInParent<PlayerController>();
            
            player.Health.Subscribe(OnHealthChanged);
        }

        private void OnHealthChanged(float value)
        {
            _renderer.GetPropertyBlock(_block);
            _block.SetFloat(HealthID, value / player.maxHealth);
            _renderer.SetPropertyBlock(_block);
        }
    }
}