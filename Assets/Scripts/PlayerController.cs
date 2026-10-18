using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Death / Respawn")]
    [SerializeField] private float dieAnimationDuration = 0.8f;
    [SerializeField] private float fadeDuration = 0.6f;
    [SerializeField] private float respawnDelay = 0.1f;

    [Header("Slide")]
    [SerializeField] private float slideSpeed = 10f;
    [SerializeField] private float slideDuration = 0.5f;

    [Header("Joystick")]
    [SerializeField] private Joystick joystick;

    [Header("Audio")]
    [SerializeField] private AudioClip walkAudio;
    [SerializeField] private AudioClip destroyAudio;
    [SerializeField] private AudioClip winAudio;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D mainCollider;
    private SpriteRenderer[] spriteRenderers;

    private Vector2 movement;
    private Vector3 startPosition;

    private bool isFacingRight = true;
    private bool wasMoving = false;
    private bool isDead = false;
    private bool isSliding = false;

    // Double tap
    private float lastTapTime;
    private const float doubleTapThreshold = 0.3f;

    // Walking audio
    private GameObject walkAudioObject;
    private AudioSource walkAudioSource;

    public int KeyCount = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        mainCollider = GetComponent<Collider2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        startPosition = transform.position;
        FindJoystick();
    }
    void Start()
    {
        FindJoystick();
    }

    private void FindJoystick()
    {
        if (joystick != null) return;

        GameObject joyObj = GameObject.Find("Fixed Joystick");
        if (joyObj != null)
        {
            joystick = joyObj.GetComponent<Joystick>();
            Debug.Log("dONE");
        }
    }

    void Update()
    {
        if (isDead || isSliding) return;



        HandleKeyboardInput();


        HandleJoystickInput();
        HandleDoubleTapSlide();

        bool isMoving = movement.sqrMagnitude > 0.01f;

        HandleWalkAudio(isMoving);
        HandleAnimations(isMoving);
        HandleFlip();
    }

    void FixedUpdate()
    {
        if (isDead || isSliding) return;

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    // ================= INPUT =================

    private void HandleKeyboardInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartSlide();
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
#endif
    }

    private void HandleJoystickInput()
    {
        if (joystick == null) return;

        Vector2 joyInput = joystick.Direction;

        if (joyInput.sqrMagnitude > 0.01f)
        {
            movement = joyInput.normalized;
        }
        else
        {
            // Finger lifted → stop movement
            movement = Vector2.zero;
        }

        /*#if UNITY_ANDROID || UNITY_IOS
                TouchPhase phase = TouchPhase.Began;
                if (phase == TouchPhase.Began)
                {
                    joystick.transform.position = Input.GetTouch(0).position;
                }
        #endif*/


    }

    private void HandleDoubleTapSlide()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            return;

        if (touch.phase == TouchPhase.Began)
        {
            if (Time.time - lastTapTime < doubleTapThreshold)
            {
                StartSlide();
                lastTapTime = 0f;
            }
            else
            {
                lastTapTime = Time.time;
            }
        }
#endif
    }

    // ================= MOVEMENT =================

    private void HandleAnimations(bool isMoving)
    {
        if (isMoving && !wasMoving)
        {
            animator.ResetTrigger("Idle");
            animator.SetTrigger("Move");
        }
        else if (!isMoving && wasMoving)
        {
            animator.ResetTrigger("Move");
            animator.SetTrigger("Idle");
        }

        wasMoving = isMoving;
    }

    private void HandleFlip()
    {
        if (movement.x > 0 && !isFacingRight)
            Flip();
        else if (movement.x < 0 && isFacingRight)
            Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // ================= SLIDE =================

    private void StartSlide()
    {
        if (isSliding) return;

        StopWalkAudio();
        animator.SetTrigger("Slide");
        StartCoroutine(Slide());
    }

    private IEnumerator Slide()
    {
        isSliding = true;

        Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.fixedDeltaTime;
            rb.MovePosition(rb.position + dir * slideSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        isSliding = false;
    }

    // ================= AUDIO =================

    private void HandleWalkAudio(bool isMoving)
    {
        if (isMoving)
        {
            if (walkAudioObject == null && walkAudio != null)
            {
                walkAudioObject = new GameObject("WalkAudio");
                walkAudioObject.transform.SetParent(transform);
                walkAudioObject.transform.localPosition = Vector3.zero;

                walkAudioSource = walkAudioObject.AddComponent<AudioSource>();
                walkAudioSource.clip = walkAudio;
                walkAudioSource.loop = true;
                walkAudioSource.spatialBlend = 0f;
                walkAudioSource.Play();
            }
        }
        else
        {
            StopWalkAudio();
        }
    }

    private void StopWalkAudio()
    {
        if (walkAudioObject != null)
        {
            walkAudioSource.Stop();
            Destroy(walkAudioObject);
            walkAudioObject = null;
            walkAudioSource = null;
        }
    }

    private void PlayOneShotAudio(AudioClip clip)
    {
        if (clip == null) return;

        GameObject obj = new GameObject("OneShotAudio");
        obj.transform.position = transform.position;

        AudioSource src = obj.AddComponent<AudioSource>();
        src.clip = clip;
        src.spatialBlend = 0f;
        src.Play();

        Destroy(obj, clip.length);
    }

    // ================= COLLISIONS =================

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obs") && !isDead)
        {
            StopWalkAudio();
            PlayOneShotAudio(destroyAudio);
            StartCoroutine(DieAndRespawn());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Finish"))
        {
            StopWalkAudio();
            PlayOneShotAudio(winAudio);
            GameManager.Instance.LevelCompletePan.SetActive(true);
        }

        if (other.CompareTag("Key"))
        {
            KeyCount++;
            Destroy(other.gameObject);
        }

        if (other.gameObject.CompareTag("Obs") && !isDead)
        {
            StopWalkAudio();
            PlayOneShotAudio(destroyAudio);
            StartCoroutine(DieAndRespawn());
        }
    }

    // ================= DEATH =================

    private IEnumerator DieAndRespawn()
    {
        isDead = true;

        animator.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        mainCollider.enabled = false;

        yield return new WaitForSeconds(dieAnimationDuration);
        yield return StartCoroutine(FadeTo(0f, fadeDuration));

        transform.position = startPosition;
        animator.SetTrigger("Idle");

        yield return StartCoroutine(FadeTo(1f, fadeDuration));

        rb.simulated = true;
        mainCollider.enabled = true;
        isDead = false;
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float elapsed = 0f;
        float[] startAlphas = new float[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
            startAlphas[i] = spriteRenderers[i].color.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                Color c = spriteRenderers[i].color;
                c.a = Mathf.Lerp(startAlphas[i], targetAlpha, t);
                spriteRenderers[i].color = c;
            }

            yield return null;
        }
    }
}
