using UnityEngine;

public class Enemy : CellObject
{
    [SerializeField, Range(1, 10)]
    private int health = 3;

    [SerializeField] private AudioClip attackSound;

    [SerializeField, Range(0f, 1f)]
    private float attackVolume = 0.8f;

    [SerializeField] private AudioClip deathSound;

    [SerializeField, Range(0f, 1f)]
    private float deathVolume = 0.8f;

    [SerializeField] private ParticleSystem deathEffect;

    private int m_CurrentHealth;

    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TurnManager.OnTick +=
                TurnHappened;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.TurnManager != null)
        {
            GameManager.Instance.TurnManager.OnTick -=
                TurnHappened;
        }
    }

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        m_CurrentHealth = health;
    }

    public override bool PlayerWantsToEnter()
    {
        m_CurrentHealth--;

        if (m_CurrentHealth <= 0)
        {
            if (deathSound != null)
            {
                AudioSource.PlayClipAtPoint(
                    deathSound,
                    transform.position,
                    deathVolume
                );
            }

            if (deathEffect != null)
            {
                ParticleSystem effect = Instantiate(
                    deathEffect,
                    transform.position,
                    Quaternion.identity
                );

                effect.Play();
                Destroy(effect.gameObject, 2f);
            }

            GameManager.Instance.BoardManager
                .GetCellData(m_Cell)
                .ContainedObject = null;

            gameObject.SetActive(false);
        }

        return false;
    }

    private bool MoveTo(Vector2Int coord)
    {
        BoardManager board =
            GameManager.Instance.BoardManager;

        BoardManager.CellData targetCell =
            board.GetCellData(coord);

        if (targetCell == null ||
            !targetCell.Passable ||
            targetCell.ContainedObject != null)
        {
            return false;
        }

        board.GetCellData(m_Cell)
            .ContainedObject = null;

        targetCell.ContainedObject = this;
        m_Cell = coord;

        transform.position =
            board.CellToWorld(coord);

        return true;
    }

    private void TurnHappened()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        PlayerController player =
            GameManager.Instance.PlayerController;

        Vector2Int playerCell = player.Cell;

        int xDistance = playerCell.x - m_Cell.x;
        int yDistance = playerCell.y - m_Cell.y;

        int absXDistance = Mathf.Abs(xDistance);
        int absYDistance = Mathf.Abs(yDistance);

        bool nextToPlayer =
            (xDistance == 0 && absYDistance == 1) ||
            (yDistance == 0 && absXDistance == 1);

        if (nextToPlayer)
        {
            if (attackSound != null)
            {
                AudioSource.PlayClipAtPoint(
                    attackSound,
                    transform.position,
                    attackVolume
                );
            }

            GameManager.Instance.ChangeFood(-3);
            return;
        }

        if (absXDistance > absYDistance)
        {
            if (!TryMoveInX(xDistance))
                TryMoveInY(yDistance);
        }
        else
        {
            if (!TryMoveInY(yDistance))
                TryMoveInX(xDistance);
        }
    }

    private bool TryMoveInX(int distance)
    {
        if (distance > 0)
            return MoveTo(m_Cell + Vector2Int.right);

        if (distance < 0)
            return MoveTo(m_Cell + Vector2Int.left);

        return false;
    }

    private bool TryMoveInY(int distance)
    {
        if (distance > 0)
            return MoveTo(m_Cell + Vector2Int.up);

        if (distance < 0)
            return MoveTo(m_Cell + Vector2Int.down);

        return false;
    }
}
