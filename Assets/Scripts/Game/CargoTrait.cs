using UnityEngine;
public abstract class CargoTrait : MonoBehaviour
{
    protected Cargo _cargo;

    public virtual void Initialize(Cargo cargo)
    {
        _cargo = cargo;
    }

    // 1초마다(혹은 턴마다) 호출될 함수
    public virtual void OnTick() { }

    public virtual void OnPickedUp() { }

    public virtual void OnDropped() { }
}