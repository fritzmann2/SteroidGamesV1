using UnityEngine;

public class Sword : Weapon
{
    override public void Attack1()
    {
        attackmulti = 1f;
        performattack(AttackTypeSword.Slash.ToString());
    }
    override public void Attack2()
    {
        attackmulti = 0.9f;
        performattack(AttackTypeSword.Stab.ToString());
    }
    override public void Attack3()
    {
        attackmulti = 1.5f;
        performattack(AttackTypeSword.Charge.ToString());
    }
    override public void Attack4()
    {
        attackmulti = 0.7f;
        performattack(AttackTypeSword.Throw.ToString());
    }

    protected override void Awake()
    {
        type = EquipmentType.Sword;
        base.Awake();
    }

    protected virtual void LateUpdate()
    {
        if (transform.parent == null) return;
        Vector2 inputVector = controls.Gameplay.Move.ReadValue<Vector2>();
        moveInput = inputVector.x;
        if (moveInput != 0f)
        {
            LastmoveInput = moveInput;
        }
        if (handPosition != null)
        {
            Vector3 handPos = handPosition.position;
        
            Vector3 finalPos = handPos + (handPosition.rotation * animOffset * LastmoveInput);

            transform.position = finalPos;
        }
    }
}


public enum AttackTypeSword
{
    Stab,   
    Slash,
    Charge,
    Throw
}