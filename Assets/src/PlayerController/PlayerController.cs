using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /*****************************************************************/

    public float moveSpeed;
    public float jumpForce;
    public bool onGround;

    private Animator _animator;
    private Rigidbody2D _rb;

    private float _horizontal;
    private int _worldSize;

    private bool _hit;

    public Vector2 spawnPos;

    /*****************************************************************/

    public void Spawn(int worldSize)
    {
        GetComponent<Transform>().position = spawnPos;
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        this._worldSize = worldSize;
    }

    /*****************************************************************/

    private void OnTriggerStay2D(Collider2D col) {
        if (col.CompareTag("Ground"))
            onGround = true;
    }

    /*****************************************************************/

    private void OnTriggerExit2D(Collider2D col) {

        if (col.CompareTag("Ground"))
            onGround = false;
    }

    /*****************************************************************/

    private void FixedUpdate() {
        
        _horizontal = Input.GetAxis("Horizontal");
        var jump = Input.GetAxis("Jump");
        var vertical = Input.GetAxisRaw("Vertical");
        
        if ( GetComponent<Transform>().position.x < 2 && _horizontal < 0
             || (GetComponent<Transform>().position.x > _worldSize - 2 && _horizontal > 0) )
        {
            _rb.velocity = new Vector2(0, _rb.velocity.y);
            return;
        }
            
        var movement = new Vector2(_horizontal * moveSpeed, _rb.velocity.y);

        transform.localScale = new Vector3((_horizontal > 0 ? -1 : 1), 1, 1);

        if (vertical > 0.1f || jump > 0.1f)
        {
            if (onGround)
                movement.y = jumpForce;
        }

        _rb.velocity = movement;
    }
    
    /*****************************************************************/

    private void Update()
    {
        _animator.SetFloat("horizontal", _horizontal);
        _animator.SetBool("hit", _hit);
    }
    /*****************************************************************/

}
