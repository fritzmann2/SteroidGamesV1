using UnityEngine;
using Unity.Netcode;
using System;

public class TankZomby : BaseEnemy
{
    protected float angle;
    protected float facingDirection;
    public float frontalDamageMultiplier = 0.3f;
    protected bool isRoling = false;

    [Header("Turn Settings")]
    public float turnDelay = 1.0f; 
    protected float turnTimer = 0f;
    protected bool isTurning = false;

    public override void Reset()
    {
        id = "TankZomby";
        maxHealth = 300;
        damage = 10f;
        movementSpeed = 2f;
        attackDistance = 1f;
        attackCooldown = 2f;
        frontalDamageMultiplier = 0.3f;
        
        turnTimer = 0f;
        isTurning = false;
        isRoling = false;
        base.Reset();
    }

    override public void Attack()
    {
        if (!IsServer) return;
        targetPlayer.GetComponent<BaseEntety>().TakeDamage(damage, false);
    }
    
    protected override void move()
    {
        if (activePlayers.Count > 0 && targetPlayer != null)
        {
            Vector3 direction = targetPlayer.position - transform.position;
            float distanceX = Mathf.Abs(direction.x);
            
            if (distanceX > 0.1f) 
            {
                float desiredFacingDirection = direction.x > 0 ? 1f : -1f;
                
                if (desiredFacingDirection != facingDirection)
                {
                    isTurning = true;
                    stop();
                    if (isRoling) stopAnimation(); 

                    turnTimer += Time.deltaTime;
                    if (turnTimer >= turnDelay)
                    {
                        facingDirection = desiredFacingDirection;
                        transform.localScale = new Vector3(facingDirection, 1f, 1f);
                        isTurning = false;
                        turnTimer = 0f;
                    }
                    return; 
                }
                else
                {
                    isTurning = false;
                    turnTimer = 0f;
                    
                    if (!isWallAhed && !isVoidAhed)
                    {
                        if (distanceX > attackDistance)
                        {
                            if (distanceX < maxdistance / 4)
                            {
                                walk();
                            }
                            else if (distanceX < maxdistance)
                            {
                                role();
                            }
                            else
                            {
                                stopAndReset();
                            }
                        }
                        else
                        {
                            stopAndReset(); 
                        }
                    }
                    else
                    {
                        stopAndReset(); 
                    }
                }
            }
            else
            {
                stopAndReset();
            }
        }
        else
        {
            stopAndReset();
        }
    }

    protected void walk()
    {
        rb.linearVelocity = new Vector2(facingDirection * movementSpeed, rb.linearVelocity.y);
        
        if (isRoling)
        {
            stopAnimation();
        }
    }

    protected void role()
    {
        rb.linearVelocity = new Vector2(facingDirection * movementSpeed * 3f, rb.linearVelocity.y);
        
        if (!isRoling)
        {
            isRoling = true;
            PlayAnimationClientsAndHostRpc("Roll");
        }
    }

    private void stopAndReset()
    {
        stop();
        if (isRoling)
        {
            stopAnimation();
        }
    }

    public void stopAnimation()
    {
        isRoling = false;
        PlayAnimationClientsAndHostRpc("Idle"); 
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PlayAnimationClientsAndHostRpc(string animationName)
    {
        if (anim != null)
        {
            anim.ResetTrigger(animationName); 
            anim.SetTrigger(animationName);
        }
    }

    override public void TakeDamage(float damage, bool isCrit)
    {
        if (targetPlayer != null)
        {
            float directionToPlayer = targetPlayer.position.x - transform.position.x;
            if (Mathf.Sign(directionToPlayer) == Mathf.Sign(facingDirection))
            {
                damage *= frontalDamageMultiplier;
            }
        }

        base.TakeDamage(damage, isCrit);
    }
}