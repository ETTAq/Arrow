using UnityEngine;
using System.Collections.Generic;

public class ArrowPool : MonoBehaviour
{
    public GameObject arrowPrefab;
    public int poolSize = 20;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject arrow = Instantiate(arrowPrefab);
            arrow.SetActive(false);
            pool.Enqueue(arrow);
        }
    }

    public GameObject GetArrow(Vector3 position, Quaternion rotation)
    {
        if (pool.Count > 0)
        {
            GameObject arrow = pool.Dequeue();
            arrow.transform.position = position;
            arrow.transform.rotation = rotation;
            arrow.SetActive(true);
            return arrow;
        }
        else
        {
            // 钱 何练 矫 货肺 积己
            return Instantiate(arrowPrefab, position, rotation);
        }
    }

    public void ReturnArrow(GameObject arrow)
    {
        arrow.SetActive(false);
        pool.Enqueue(arrow);
    }
}
