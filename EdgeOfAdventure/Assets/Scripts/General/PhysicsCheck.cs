using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCheck : MonoBehaviour
{    
    [Header("Arguments")]
    [SerializeField]private bool manual;
    [SerializeField]private LayerMask Ground;
    private Rigidbody2D rb;
    [SerializeField]private Vector2 bottomOffset;
    [SerializeField]private Vector2 leftOffset;
    [SerializeField]private Vector2 rightOffset;
    [SerializeField]private Vector2 upperLeftoffset;
    [SerializeField]private Vector2 lowerLeftoffset;
    [SerializeField]private Vector2 upperRightoffset;
    [SerializeField]private Vector2 lowerRightoffset;

    public float checkRadius;
    private CapsuleCollider2D coll;
    
    [Header("States")]
    [SerializeField]private bool onGround;
    [SerializeField]private bool touchRightwall;
    [SerializeField]private bool touchLeftwall;

    private void Awake() {
        coll = GetComponent<CapsuleCollider2D>();

        if (!manual) {
            rightOffset = new Vector2((coll.bounds.size.x + coll.offset.x) / 2, coll.bounds.size.y / 2);
            leftOffset = new Vector2(-rightOffset.x, rightOffset.y);
            upperRightoffset = new Vector2(rightOffset.x, rightOffset.y * 1.5f);
            lowerRightoffset = new Vector2(rightOffset.x, rightOffset.y / 2f);
            upperLeftoffset = new Vector2(leftOffset.x, leftOffset.y * 1.5f);
            lowerLeftoffset = new Vector2(leftOffset.x, leftOffset.y / 2f);
        }
    }

    private void Update() {
        Check();
    }

    private void Check(){
        // checks if player object is overlapping with the platform layer mask
        onGround = Physics2D.OverlapCircle((Vector2)transform.position + 
            new Vector2(bottomOffset.x * transform.localScale.x, bottomOffset.y), checkRadius, Ground);

        // check if player is overlapping with the left wall
        touchLeftwall = Physics2D.OverlapCircle((Vector2)transform.position + 
            new Vector2(leftOffset.x, leftOffset.y), checkRadius, Ground) 
            || Physics2D.OverlapCircle((Vector2)transform.position + 
                new Vector2(upperLeftoffset.x, upperLeftoffset.y), checkRadius, Ground)
            || Physics2D.OverlapCircle((Vector2)transform.position + 
                new Vector2(lowerLeftoffset.x, lowerLeftoffset.y), checkRadius, Ground);


        // check if player is overlapping with the right wall
        touchRightwall = Physics2D.OverlapCircle((Vector2)transform.position + 
            new Vector2(rightOffset.x, rightOffset.y), checkRadius, Ground)
            || Physics2D.OverlapCircle((Vector2)transform.position + 
                new Vector2(upperRightoffset.x, upperRightoffset.y), checkRadius, Ground)
            || Physics2D.OverlapCircle((Vector2)transform.position + 
                new Vector2(lowerRightoffset.x, lowerRightoffset.y), checkRadius, Ground);
    }

    private void OnDrawGizmosSelected() {
        // to visualize the size of overlapping area so as to set a better checkRadius and bottomOffset
        Gizmos.DrawSphere((Vector2) transform.position + bottomOffset * transform.localScale.x, checkRadius);
        Gizmos.DrawSphere((Vector2) transform.position + leftOffset, checkRadius);
        Gizmos.DrawSphere((Vector2) transform.position + rightOffset, checkRadius);
        Gizmos.DrawSphere((Vector2) transform.position + upperLeftoffset, checkRadius);
        Gizmos.DrawSphere((Vector2) transform.position + lowerLeftoffset, checkRadius);
        Gizmos.DrawSphere((Vector2) transform.position + upperRightoffset, checkRadius);
        Gizmos.DrawSphere((Vector2) transform.position + lowerRightoffset, checkRadius);
    }

    public bool OnGround() {
        return onGround;
    }

    public bool TouchLeftWall() {
        return touchLeftwall;
    }

    public bool TouchRightWall() {
        return touchRightwall;
    }

    public void ResetLeftOffset() {
        leftOffset = new Vector2(-rightOffset.x, rightOffset.y);
    }

    public void ResetRightOffset(Vector2 vector2) {
        rightOffset = vector2;
    }


}
