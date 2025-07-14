using UnityEngine;

public class SpikeDealDame : MonoBehaviour
{
    [SerializeField] private int damageAmount = 5;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    public void DealDamage()
    {
        Debug.Log("GỌI DealDamage!");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        Debug.Log("Số enemy phát hiện: " + hitEnemies.Length);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Enemy bị trúng: " + enemy.name);
            enemy.SendMessage("TakeDamage", damageAmount, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
