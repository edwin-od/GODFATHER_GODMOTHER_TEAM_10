using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy.asset", menuName = "Enemy")]
public class EnemySO : ScriptableObject
{
    public GameManager.Dynamic type = GameManager.Dynamic.Type1;
    public Transform prefab;
}
