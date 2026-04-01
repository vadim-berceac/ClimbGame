using System;
using UnityEngine;

public interface IDamageable
{
    public float CurrentHealth { get; set; }
    public float MaxHealth { get; set; }
    public bool IsDead { get; set; }
    public Transform Transform { get; set; }
    public event Action<float, Transform> OnDamageTaken;
    public event Action OnDeath;
    
    public void TakeDamage(float damage, Transform source);
}
