using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
public class NoteSystem : MonoBehaviour
{

    public GameObject Note; // on the wall

    public GameObject NoteGB; // the whole image

    public Text NoteText;

    public AudioSource Source;

    public AudioClip PaperSound;

    public AudioClip TakeNote;

    public FirstPersonController FpsController;

    public Text InteractionText;

    bool canInteract = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (canInteract == true)
        {
            Ray ray1 = new Ray(transform.position, transform.forward);
            RaycastHit hit1;

            if (Physics.Raycast(ray1, out hit1, 10f))
            {
                if (hit1.collider.CompareTag("Paper"))
                {
                    InteractionText.text = "Press E to take note";

                    if(Input.GetKeyDown(KeyCode.E))
                    {
                        canInteract = false;
                        InteractionText.text = "";
                        NoteGB.SetActive(true);
                        NoteText.text = "THIS IS A TEST WE DID. I HOPE YOU LIVE";
                        Source.PlayOneShot(PaperSound);
                        FpsController.enabled = false;

                        //cursor

                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;

                        //cursor
                    }
                }
                else
                {
                    InteractionText.text = "";
                }

            }
            

        }

    }

    public void TakeNoteFunc()
    {
        Note.SetActive(false);
        NoteGB.SetActive(false);
        NoteText.text = "";
        Source.PlayOneShot(TakeNote);

        FpsController.enabled = true;

        //cursor

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //cursor

    }
}
