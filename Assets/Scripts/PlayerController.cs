using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private BoardManager m_Board;
    private Vector2Int m_CellPosition;
    private Animator m_Animator;
    private AudioSource m_AudioSource;

    private bool m_IsMoving;
    private Coroutine m_MoveCoroutine;

    [SerializeField] private float MoveSpeed = 5f;

    [SerializeField] private AudioClip moveSound;
    [SerializeField, Range(0f, 1f)]
    private float moveVolume = 0.8f;

    [SerializeField] private AudioClip attackSound;
    [SerializeField, Range(0f, 1f)]
    private float attackVolume = 0.8f;

    public Vector2Int Cell => m_CellPosition;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_AudioSource = GetComponent<AudioSource>();
    }

    public void Spawn(
        BoardManager boardManager,
        Vector2Int cell
    )
    {
        m_Board = boardManager;
        m_CellPosition = cell;
        transform.position = m_Board.CellToWorld(cell);
        m_IsMoving = false;
    }

    public void MoveTo(
        Vector2Int cell,
        bool immediate
    )
    {
        m_CellPosition = cell;

        if (immediate)
        {
            transform.position =
                m_Board.CellToWorld(cell);

            m_IsMoving = false;
            return;
        }

        if (moveSound != null &&
            m_AudioSource != null)
        {
            m_AudioSource.PlayOneShot(
                moveSound,
                moveVolume
            );
        }

        m_MoveCoroutine =
            StartCoroutine(
                MoveCoroutine(
                    m_Board.CellToWorld(cell)
                )
            );
    }

    private IEnumerator MoveCoroutine(
        Vector3 targetPosition
    )
    {
        m_IsMoving = true;
        m_Animator.SetBool("Moving", true);

        Vector3 startPosition =
            transform.position;

        float distance =
            Vector3.Distance(
                startPosition,
                targetPosition
            );

        float duration =
            distance / MoveSpeed;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsed / duration);

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    progress
                );

            yield return null;
        }

        transform.position = targetPosition;
        m_IsMoving = false;
        m_MoveCoroutine = null;

        m_Animator.SetBool("Moving", false);

        BoardManager.CellData cellData =
            m_Board.GetCellData(m_CellPosition);

        if (cellData != null &&
            cellData.ContainedObject != null)
        {
            CellObject cellObject =
                cellData.ContainedObject;

            cellData.ContainedObject = null;
            cellObject.PlayerEntered();
        }
    }

    private void Update()
    {
        if (m_Board == null ||
            m_IsMoving)
        {
            return;
        }

        Vector2Int newCellTarget =
            m_CellPosition;

        bool hasMoved = false;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y++;
            hasMoved = true;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y--;
            hasMoved = true;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x++;
            hasMoved = true;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x--;
            hasMoved = true;
        }

        if (!hasMoved)
        {
            return;
        }

        BoardManager.CellData targetCellData =
            m_Board.GetCellData(newCellTarget);

        if (targetCellData == null ||
            !targetCellData.Passable)
        {
            return;
        }

        GameManager.Instance.TurnManager.Tick();

        if (targetCellData.ContainedObject == null)
        {
            MoveTo(newCellTarget, false);
        }
        else if (
            targetCellData.ContainedObject
                .PlayerWantsToEnter()
        )
        {
            MoveTo(newCellTarget, false);
        }
        else
        {
            m_Animator.SetTrigger("Attack");

            if (attackSound != null &&
                m_AudioSource != null)
            {
                m_AudioSource.PlayOneShot(
                    attackSound,
                    attackVolume
                );
            }
        }
    }
}
