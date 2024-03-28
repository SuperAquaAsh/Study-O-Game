using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamColorManager : MonoBehaviour
{
    #region Singleton
    public static TeamColorManager instance;
    void SetSingleton(){
        if(instance == null){
            instance = this;
        }else{
            Debug.LogWarning("Two instaces of TeamColorManager found, deleting gameobject");
            Destroy(gameObject);
        }
    }
    #endregion

    [SerializeField] Color[] teamColors;
    [SerializeField] Color[] teamFillColors;

    public static Dictionary<int, Color> teamColorDictionary = new Dictionary<int, Color>();
    public static Dictionary<int, Color> teamFillerColorDictionary = new Dictionary<int, Color>();

    
    private void Awake() {
        SetSingleton();
        for (int i = 0; i < teamColors.Length; i++)
        {
            if(!teamColorDictionary.ContainsKey(i)) teamColorDictionary.Add(i, teamColors[i]);
        }
        for (int i = 0; i < teamFillColors.Length; i++)
        {
            if(!teamFillerColorDictionary.ContainsKey(i)) teamFillerColorDictionary.Add(i, teamFillColors[i]);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
