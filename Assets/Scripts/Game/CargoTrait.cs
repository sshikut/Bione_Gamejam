using UnityEngine;
public abstract class CargoTrait : MonoBehaviour
{
    protected Cargo _cargo;

    public virtual void Initialize(Cargo cargo)
    {
        _cargo = cargo;
    }

    protected virtual void OnEnable()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnTickEvent += OnTick;
        }
    }

    protected virtual void OnDisable()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnTickEvent -= OnTick;
        }
    }

    public virtual void OnTick() { }

    public virtual void OnPickedUp() { }

    public virtual void OnDropped() { }
}