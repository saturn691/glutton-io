using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassSpawner : MonoBehaviour
{

    #region instance 
    public static MassSpawner ins;

    private void Awake()
    {
        if( ins == null)
        {
            ins = this;
        }
    }
    #endregion

    public GameObject Mass;

    //should be renamed to Splits
    
    public List<GameObject> Players = new List<GameObject>();
    public List<GameObject> CreatedMasses = new List<GameObject>();


    public int MaxMass = 50;
    public float Time_To_Instantiate = 0.5f;
    
    Map map;

    //should be renamed to MaxSplits
    public int MaxPlayers = 16;


    private void Start()
    {
        map = Map.ins;
        StartCoroutine(CreateMass());
    
    }

    public IEnumerator CreateMass()
    {
        // wait for seconds
        yield return new WaitForSecondsRealtime(Time_To_Instantiate);

        if(CreatedMasses.Count <= MaxMass)
        {
            Vector2 Position = new Vector2(Random.Range(-map.MapLimits.x, map.MapLimits.x), Random.Range(-map.MapLimits.y, map.MapLimits.y));
            Position /= 2;

            GameObject m =  Instantiate(Mass, Position, Quaternion.identity);

            AddMass(m);

        }

        StartCoroutine(CreateMass());


    }

public void AddMass(GameObject m)
{
    if (m != null && !m.Equals(null))
    {
        if (!CreatedMasses.Contains(m))
        {
            CreatedMasses.Add(m);

            for (int i = 0; i < Players.Count; i++)
            {
                PlayerEatMass pp = Players[i]?.GetComponent<PlayerEatMass>();
                if (pp != null)
                {
                    pp.AddMass(m);
                }
            }
        }
    }
    else
    {
        Debug.LogWarning("Tried to add null or destroyed GameObject to mass list.");
    }
}


    public void RemoveMass(GameObject m)
    {
        if(CreatedMasses.Contains(m) == true)
        {
            CreatedMasses.Remove(m);


            for (int i = 0; i < Players.Count; i++)
            {
                PlayerEatMass pp = Players[i].GetComponent<PlayerEatMass>();
                pp.RemoveMass(m);
            }
        }
    }

    public void AddPlayer(GameObject b)
    {
        if(Players.Contains(b) == false)
        {
            Players.Add(b);
        }
    }

    public void RemovePlayer(GameObject b)
    {
        Debug.Log("RemovePlayer" + b.name);

           bool removed = Players.Remove(b);
    }

}
