using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NetworkGame.Client;

public class EnemyFloatyName : MonoBehaviour
{
    [SerializeField]
    private TextMesh playerName;

    private void Start()
    {
        playerName.text = gameObject.name;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameObject.name.Equals(playerName.text))
        {
            playerName.text = gameObject.name;
        }
    }
}
