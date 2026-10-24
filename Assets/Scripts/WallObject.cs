using UnityEngine;
using UnityEngine.Tilemaps;

public class WallObject : CellObject
{
    [SerializeField] private Tile obstacleTile;

    [SerializeField, Range(1, 10)]
    private int maxHealth = 3;

    [SerializeField] private ParticleSystem breakEffect;

    private int m_HealthPoint;
    private Tile m_OriginalTile;

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);

        m_HealthPoint = maxHealth;

        m_OriginalTile =
            GameManager.Instance.BoardManager
                .GetCellTile(cell);

        GameManager.Instance.BoardManager
            .SetCellTile(cell, obstacleTile);
    }

    public override bool PlayerWantsToEnter()
    {
        m_HealthPoint--;

        if (m_HealthPoint <= 0)
        {
            if (breakEffect != null)
            {
                ParticleSystem effect = Instantiate(
                    breakEffect,
                    transform.position,
                    Quaternion.identity
                );

                effect.Play();
                Destroy(effect.gameObject, 2f);
            }

            GameManager.Instance.BoardManager
                .SetCellTile(m_Cell, m_OriginalTile);

            GameManager.Instance.BoardManager
                .GetCellData(m_Cell)
                .ContainedObject = null;

            gameObject.SetActive(false);
            return true;
        }

        return false;
    }
}
