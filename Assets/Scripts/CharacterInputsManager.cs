using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInputsManager : MonoBehaviour
{
    #region Singleton
    private static CharacterInputsManager instance;

    public static CharacterInputsManager GetInstance()
    {
        if (instance == null)
        {
            instance = new CharacterInputsManager();
            Debug.Log("new CharacterController instance created");
        }
        return instance;
    }
    #endregion

    [SerializeField] private InputActionAsset _actionAsset;

    private InputAction up;
    public bool Up { get; private set; }

    private InputAction down;
    public bool Down { get; private set; }

    private InputAction left;
    public bool Left { get; private set; }

    private InputAction right;
    public bool Right { get; private set; }

    private void Awake()
    {
        #region Singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.Log("An instance of CharacterController already exists");
            Destroy(gameObject);
        }
        #endregion

        up = _actionAsset.FindAction("Up");
        up.performed += context => Up = true;
        up.canceled += context => Up = false;

        down = _actionAsset.FindAction("Down");
        down.performed += context => Down = true;
        down.canceled += context => Down = false;

        left = _actionAsset.FindAction("Left");
        left.performed += context => Left = true;
        left.canceled += context => Left = false;

        right = _actionAsset.FindAction("Right");
        right.performed += context => Right = true;
        right.canceled += context => Right = false;
    }
}
