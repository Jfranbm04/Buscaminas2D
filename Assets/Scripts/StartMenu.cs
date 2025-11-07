using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class StartMenu : MonoBehaviour{

    [SerializeField] public TMP_InputField width;
    [SerializeField] public TMP_InputField height;
    [SerializeField] public TMP_InputField bombs;

    private int flags;

    public static StartMenu instance;

    public void Start(){

        instance = this;

        int bombCount;

        if (int.TryParse(bombs.text, out bombCount)){

            flags = bombCount;
        }
    }
}

