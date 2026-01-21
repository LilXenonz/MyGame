using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public InputActionReference moveAction;

    public CharacterController CharacterController;

    public float moveSpeed;

    private float ySpeed;

    public InputActionReference jumpAction;
    public float jumpForce;

    public InputActionReference lookAction;
    private float HoriRot, VertRot;
    public float LookSpeed;
    public Camera Camera;
    public float minLookAngle, maxLookAngle;

    public LayerMask StockLayer;
    public float InteractionRange;

    private StockObject heldPickup;
    public Transform holdPoint;

    public float throwForce;

    public LayerMask ShelfLayer;

    public LayerMask StockBoxLayer;
    public StockBoxController heldBox;
    public Transform boxHoldPoint;

    public float waitToPlaceStock;
    private float placeStockCounter;

    public LayerMask BinLayer;

    public LayerMask checkoutLayer;

    public LayerMask InteractLayer;

    public static PlayerController instance;

    public bool lockPlayer;

    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (lockPlayer == false)
        { 
            Vector2 lookInput = lookAction.action.ReadValue<Vector2>();
    
            HoriRot += lookInput.x * Time.deltaTime * LookSpeed;
            transform.rotation = Quaternion.Euler(0f, HoriRot, 0f);
    
            VertRot -= lookInput.y * Time.deltaTime * LookSpeed;
            VertRot = Mathf.Clamp(VertRot, minLookAngle, maxLookAngle);
            Camera.transform.localRotation = Quaternion.Euler(VertRot, 0f, 0f);
    
    
    
    
            Vector2 MoveInput = moveAction.action.ReadValue<Vector2>();
    
    
            //debug.log(MoveInput);
    
            //transform.position = transform.position + new Vector3(moveInput.x * Time.deltaTime * moveSpeed, 0f, moveInput.y * Time.deltaTime * moveSpeed);
    
            //Vector3 moveAmount = new Vector3(MoveInput.x, 0f, MoveInput.y);
    
            Vector3 vertMove = transform.forward * MoveInput.y;
    
            Vector3 horiMove = transform.right * MoveInput.x;
    
            //Debug.Log(vertMove + "-" + horiMove);
    
            Vector3 moveAmount = horiMove + vertMove;
            moveAmount = moveAmount.normalized;
    
    
            moveAmount = moveAmount * moveSpeed;
    
    
            if (CharacterController.isGrounded == true)
            {
                ySpeed = 0f;
    
                if (jumpAction.action.WasPressedThisFrame())
                {
                    ySpeed = jumpForce;
                }
            }
    
            ySpeed = ySpeed + (Physics.gravity.y * Time.deltaTime);
    
    
    
    
            moveAmount.y = ySpeed;
    
            CharacterController.Move(moveAmount * Time.deltaTime);
    
        }

        //check pickups 

        Ray ray = Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        RaycastHit hit;


        if (heldPickup == null && heldBox == null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (Physics.Raycast(ray, out hit, InteractionRange, StockLayer))
                {
                    //Debug.Log("i see a pickup");

                    /*heldPickup = hit.collider.gameObject;
                    heldPickup.transform.SetParent(holdPoint.transform);
                    heldPickup.transform.localPosition = Vector3.zero;
                    heldPickup.transform.localRotation = Quaternion.identity;

                    heldPickup.GetComponent<Rigidbody>().isKinematic = true;*/

                    heldPickup = hit.collider.GetComponent<StockObject>();
                    heldPickup.transform.SetParent(holdPoint);
                    heldPickup.PickUp();

                    return;
                }
                if (Physics.Raycast(ray, out hit, InteractionRange, StockBoxLayer))
                {

                    heldBox = hit.collider.GetComponent<StockBoxController>();
                    heldBox.transform.SetParent(boxHoldPoint);
                    heldBox.PickUp();

                    if (heldBox.flap1.activeSelf == true)
                    {
                        heldBox.OpenClose();
                    }

                    return;

                }
                if (Physics.Raycast(ray, out hit, InteractionRange, checkoutLayer))
                {
                    hit.collider.GetComponent<Checkout>().CheckoutCustomer();
                }

                if (Physics.Raycast(ray, out hit, InteractionRange, InteractLayer))
                {
                    hit.collider.GetComponent<InteractCallEvent>().InteractCall();
                }


            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (Physics.Raycast(ray, out hit, InteractionRange, ShelfLayer))
                {
                    heldPickup = hit.collider.GetComponent<ShelfSpaceController>().GetStock();

                    if(heldPickup != null)
                    {
                        heldPickup.transform.SetParent(holdPoint);
                        heldPickup.PickUp();
                    }

                    return;
                }

                if (Physics.Raycast(ray, out hit, InteractionRange, StockBoxLayer))
                {
                    hit.collider.GetComponent<StockBoxController>().OpenClose();
                }

            }
        }
        else
        {
            if (heldPickup != null) 
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    if (Physics.Raycast(ray, out hit, InteractionRange, ShelfLayer))
                    {
                        /*heldPickup.transform.position = hit.transform.position;
                        heldPickup.transform.rotation = hit.transform.rotation;


                        heldPickup.transform.SetParent(null);
                        heldPickup = null;*/

                        /*heldPickup.MakePlaced();

                        heldPickup.transform.SetParent(hit.transform);
                        heldPickup = null;*/

                        hit.collider.GetComponent<ShelfSpaceController>().placeStock(heldPickup);

                        if (heldPickup.isPlaced == true)
                        {
                            heldPickup = null;
                        }
                    }
                }

                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    //Rigidbody pickupRB = heldPickup.GetComponent<Rigidbody>();
                    //pickupRB.isKinematic = false;

                    heldPickup.Release();

                    heldPickup.RB.AddForce(Camera.transform.forward * throwForce, ForceMode.Impulse);

                    heldPickup.transform.SetParent(null);
                    heldPickup = null;


                }
            }

            if (heldBox != null)
            {
                if (Mouse.current.rightButton.wasPressedThisFrame)
                {

                    heldBox.Release();

                    heldBox.RB.AddForce(Camera.transform.forward * throwForce, ForceMode.Impulse);

                    heldBox.transform.SetParent(null);
                    heldBox = null;


                }

                if(Keyboard.current.eKey.wasPressedThisFrame)
                {
                    heldBox.OpenClose();
                }

                if(Mouse.current.leftButton.wasPressedThisFrame)
                {
                    if (heldBox.stockInBox.Count > 0)
                    {
                        if (Physics.Raycast(ray, out hit, InteractionRange, ShelfLayer))
                        {
                            heldBox.PlaceStockOnShelf(hit.collider.GetComponent<ShelfSpaceController>());

                            placeStockCounter = waitToPlaceStock;

                        }
                    }
                    else
                    {

                        if (Physics.Raycast(ray, out hit, InteractionRange, BinLayer))
                        {

                            Destroy(heldBox.gameObject);

                            heldBox = null;

                        }
                    }

                }

                if (Mouse.current.leftButton.isPressed)
                {
                    placeStockCounter -= Time.deltaTime;

                    if(placeStockCounter <= 0)
                    {
                        if (Physics.Raycast(ray, out hit, InteractionRange, ShelfLayer))
                        {
                            heldBox.PlaceStockOnShelf(hit.collider.GetComponent<ShelfSpaceController>());

                            placeStockCounter = waitToPlaceStock;

                        }
                    }
                }
            }
                
           
        }

    }




}
