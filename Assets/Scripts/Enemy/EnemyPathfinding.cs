using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
   [SerializeField] private float moveSpeed = 2f;
   private Rigidbody2D rb;
   private Vector2 moveDirection;
   private KnockBack knockBack;

   private void Awake()
   {
    rb = GetComponent<Rigidbody2D>();
    knockBack = GetComponent<KnockBack>();
   }

   private void FixedUpdate()
   {
    if (!knockBack.gettingKnockedBack)
    {
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }
   }

   public void MoveTo(Vector2 targetPosition)
   {
    moveDirection = targetPosition;
   }



   
   
}
