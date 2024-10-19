using UnityEngine;

[CreateAssetMenu(fileName = "New Character Stats", menuName = "CharacterStats")]
public class CharacterStats : ScriptableObject
{
    public float gravity = -100;
    public float apexGravity = -50;
    public float horizontalAccel = 130f;
    public float maxHorizontalSpeed = 15f;
    public float maxFallSpeed = -25;
    public float jumpStrength = 60f;
    public float groundFriction = 90f;
    public float fastFallSpeed = -500f;
    public float maxApexTime = 2f;
    public float apexYVel = 10f;
    public float apexXVel = 10f;
    public float coyoteTime = 5f;
}
