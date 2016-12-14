using UnityEngine;
using System.Collections;

public class Nudist : MonoBehaviour
{

    public float moveSpeed = 4f;  // enemy move speed when moving
    public int damageAmount = 10; // probably deal a lot of damage to kill player immediately
    public float attackSpeed = 6f; // enemy move while attacking and running away with armor

    public GameObject stunnedCheck; // what gameobject is the stunnedCheck
    public GameObject sparty;
    public float stunnedTime = 3f;   // how long to wait at a waypoint

    public string stunnedLayer = "StunnedEnemy";  // name of the layer to put enemy on when stunned
    public string playerLayer = "Player";  // name of the layer to put enemy on when stunned

    public bool isStunned = false;  // flag for isStunned

    // SFXs
    public AudioClip stunnedSFX;
    public AudioClip attackSFX;

    // private variables below

    // store references to components on the gameObject
    Transform _transform;
    Rigidbody2D _rigidbody;
    Animator _animator;
    AudioSource _audio;

    // movement tracking
    float _moveTime;
    float _vx = 0f;
    bool _grounded = true;
    bool _haveArmor = false;

    // store the layer number the enemy is on (setup in Awake)
    int _enemyLayer;

    // store the layer number the enemy should be moved to when stunned
    int _stunnedLayer;

    void Awake()
    {
        // get a reference to the components we are going to be changing and store a reference for efficiency purposes
        _transform = GetComponent<Transform>();

        _rigidbody = GetComponent<Rigidbody2D>();
        if (_rigidbody == null) // if Rigidbody is missing
            Debug.LogError("Rigidbody2D component missing from this gameobject");

        _animator = GetComponent<Animator>();
        if (_animator == null) // if Animator is missing
            Debug.LogError("Animator component missing from this gameobject");

        _audio = GetComponent<AudioSource>();
        if (_audio == null)
        { // if AudioSource is missing
            Debug.LogWarning("AudioSource component missing from this gameobject. Adding one.");
            // let's just add the AudioSource component dynamically
            _audio = gameObject.AddComponent<AudioSource>();
        }

        if (stunnedCheck == null)
        {
            Debug.LogError("stunnedCheck child gameobject needs to be setup on the enemy");
        }

        // setup moving defaults
        _grounded = true;

        // determine the enemies specified layer
        _enemyLayer = this.gameObject.layer;

        // determine the stunned enemy layer number
        _stunnedLayer = LayerMask.NameToLayer(stunnedLayer);

        // make sure collision are off between the playerLayer and the stunnedLayer
        // which is where the enemy is placed while stunned
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(playerLayer), _stunnedLayer, true);
    }

    // if not stunned then move the enemy when time is > _moveTime
    void Update()
    {
        if (!isStunned)
        {


            EnemyMovement();
        }

        else {
            _animator.SetBool("Moving", false);
        }
    }

    // Move the enemy through its rigidbody based on its waypoints
    void EnemyMovement()
    {
        // if there isn't anything in My_Waypoints
        if (_grounded)
        {


            // store the velocity of the nudist
            _vx = _rigidbody.velocity.x;
        }

        else {
            // enemy is moving
            _animator.SetBool("Moving", true);

            // Set the enemy's velocity to moveSpeed in the x direction.
            _rigidbody.velocity = new Vector2(_transform.localScale.x * moveSpeed, _rigidbody.velocity.y);
        }

        }



    // Attack player
    void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.tag == "Player") && !isStunned)
        {
            CharacterController2D player = collision.gameObject.GetComponent<CharacterController2D>();
            if (player.playerCanMove)
            {

                //Stole Armor
                _haveArmor = true;
                _animator.SetBool("HasArmor", true);
                player.SpartyGetsNaked();
                // attack sound
                playSound(attackSFX);



                // apply knockback to the player
                player.knockbackCount = player.knockbackLength;
                if(collision.transform.position.x < transform.position.x)
                {
                    player.knockFromRight = true;
                }
                else
                {
                    player.knockFromRight = false;
                }
            

            }
        }
    }

    // if the Enemy collides with a MovingPlatform, then make it a child of that platform
    // so it will go for a ride on the MovingPlatform
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "MovingPlatform")
        {
            this.transform.parent = other.transform;
        }
    }

    // if the enemy exits a collision with a moving platform, then unchild it
    void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.tag == "MovingPlatform")
        {
            this.transform.parent = null;
        }
    }

    // play sound through the audiosource on the gameobject
    void playSound(AudioClip clip)
    {
        _audio.PlayOneShot(clip);
    }

    // setup the enemy to be stunned
    public void Stunned( )
    {
        if (!isStunned)
        {
            isStunned = true;

            // provide the player with feedback that enemy is stunned
            playSound(stunnedSFX);
            _animator.SetTrigger("Stunned");
            if (_haveArmor == true)
            {
                _haveArmor = false;
                _animator.SetBool("HasArmor", false);
                Debug.LogError("pass");
                sparty.GetComponent<CharacterController2D>().SpartyGetsDressed();
                Debug.LogError("Pass2");
            }
            // stop moving
            _rigidbody.velocity = new Vector2(0, 0);

            // switch layer to stunned layer so no collisions with the player while stunned
            this.gameObject.layer = _stunnedLayer;
            stunnedCheck.layer = _stunnedLayer;

            // start coroutine to stand up eventually
            StartCoroutine(Stand());
        }
    }

    // coroutine to unstun the enemy and stand back up
    IEnumerator Stand()
    {
        yield return new WaitForSeconds(stunnedTime);

        // no longer stunned
        isStunned = false;

        // switch layer back to regular layer for regular collisions with the player
        this.gameObject.layer = _enemyLayer;
        stunnedCheck.layer = _enemyLayer;

        // provide the player with feedback
        _animator.SetTrigger("Stand");
    }
}