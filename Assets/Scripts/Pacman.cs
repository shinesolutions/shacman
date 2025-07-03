using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Movement))]
public class Pacman : MonoBehaviour
{
    [SerializeField]
    private AnimatedSprite deathSequence;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private Movement movement;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        movement = GetComponent<Movement>();
    }

    private void Update()
    {
        // Rotate pacman to face the movement direction every frame.
        float angle = Mathf.Atan2(movement.direction.y, movement.direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }

    // This method is called by the PlayerInput component when the "Move" action is triggered.
    private void OnMove(InputValue value)
    {
        if (GameManager.isBlocked) {
            return;
        }

        // Get the Vector2 from the joystick or WASD keys.
        Vector2 input = value.Get<Vector2>().normalized;

        // Prioritize vertical movement over horizontal
        if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
        {
            movement.SetDirection(new Vector2(0, input.y));
        }
        else if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            movement.SetDirection(new Vector2(input.x, 0));
        }
    }

    public void ResetState()
    {
        enabled = true;
        spriteRenderer.enabled = true;
        circleCollider.enabled = true;
        deathSequence.enabled = false;
        movement.ResetState();
        gameObject.SetActive(true);
    }

    public void DeathSequence()
    {
        enabled = false;
        spriteRenderer.enabled = false;
        circleCollider.enabled = false;
        movement.enabled = false;
        deathSequence.enabled = true;
        deathSequence.Restart();
    }

}
