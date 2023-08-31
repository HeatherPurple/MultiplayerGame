using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [SerializeField] private float movementSpeed = 5f;
    
    private NetworkVariable<Vector2> movementDirection = 
        new NetworkVariable<Vector2>(Vector2.zero,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

    private Rigidbody2D _rigidbody2D;
    private InputActions _inputActions;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _inputActions = new InputActions();
        _inputActions.Player.Enable();
    }

    void Update()
    {
        if (!IsOwner) return;
        MoveServerRpc();
        movementDirection.Value = _inputActions.Player.Movement.ReadValue<Vector2>();
    }

    [ServerRpc]
    private void MoveServerRpc()
    {
        _rigidbody2D.velocity = movementDirection.Value * movementSpeed;
    }
    
}
