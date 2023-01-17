using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /*****************************************************************/
    public Vector2Int mousePosition;
    
    public float moveSpeed;
    public float jumpForce;
    public bool onGround;
    public int playerRange;

    public TileClass selectedTile;

    private Animator _animator;
    private Rigidbody2D _rb;

    private float _horizontal;
    private int _worldSize;

    private bool _hit;
    private bool _place;
    
    [HideInInspector]
    public Vector2 spawnPos;
    
    public TerrainGeneration terrainGeneration;

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
        _place = Input.GetMouseButton(1);

        _hit = Input.GetMouseButton(0);

        if ((Vector2.Distance(transform.position, mousePosition) <= playerRange)) // cant place inside player
        {
            
            if (_hit && mousePosition.y > 0)
                terrainGeneration.RemoveTile(mousePosition.x, mousePosition.y);
            
            else if (_place)
                terrainGeneration.PlaceTile(selectedTile, mousePosition.x, mousePosition.y, false);
        } 
        if ( GetComponent<Transform>().position.x < 2 && _horizontal < 0
             || (GetComponent<Transform>().position.x > _worldSize - 2 && _horizontal > 0) ) //walk
        {
            _rb.velocity = new Vector2(0, _rb.velocity.y);
            return;
        }
            
        var movement = new Vector2(_horizontal * moveSpeed, _rb.velocity.y);
        
        if (_horizontal > 0) // looking direction
            transform.localScale = new Vector3(-1, 1, 1);
        else if (_horizontal < 0)
            transform.localScale = new Vector3(1, 1, 1);
        
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
        //set mouse position
        mousePosition.x = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x - 0.5f);
        mousePosition.y= Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y - 0.5f);
        
        _animator.SetFloat("horizontal", _horizontal);
        _animator.SetBool("hit", _hit || _place);
    }
    /*****************************************************************/

}
