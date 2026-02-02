using UnityEngine;

[CreateAssetMenu(fileName = "Data_NewCustomer", menuName = "Game/Customer Data")]
public class CustomerData : ScriptableObject
{
    [Header("Identity")]
    public string customerName;
    public CustomerType type;

    [Header("Visuals")]
    public Sprite bodySprite;

    [Header("Stats")]
    [Tooltip("이동 속도 계수 (1.0 = 기본, 0.8 = 느림, 1.5 = 빠름)")]
    public float speedMultiplier = 1.0f;

    [Tooltip("기다리는 시간 계수 (1.0 = 기본, 1.2 = 참을성 많음)")]
    public float patienceMultiplier = 1.0f;
}