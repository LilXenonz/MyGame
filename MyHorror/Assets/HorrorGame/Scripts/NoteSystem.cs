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

    public AudioClip[] NextPageSound;

    int PageNumber = 1;

    string ParentText;

    [SerializeField] string Page1Text;
    [SerializeField] string Page2Text;
    [SerializeField] string Page3Text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //give page value

        Page1Text = "this is a messege from me wsp man";
        Page2Text = "ibrahim är en smart människa tror jag";
        Page3Text = "wsp man hahahhahah wsp sippi sipss";

        //give page value

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
                        ParentText = Page1Text;
                        ShowText();
                    }
                }
                else
                {
                    InteractionText.text = "";
                }

            }
            

        }

    }

    void ShowText()
    {
        canInteract = false;
        InteractionText.text = "";
        NoteGB.SetActive(true);
        NoteText.text = ParentText;
        Source.PlayOneShot(PaperSound);
        FpsController.enabled = false;

        //cursor

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        //cursor
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


    public void NextPage()
    {


        if(PageNumber == 1)
        {
            PageNumber = 2;

            ParentText = Page2Text;

            ShowText();
        }
        else if(PageNumber == 2) 
        {
            PageNumber = 3;

            ParentText = Page3Text;

            ShowText();

        }
        else if (PageNumber == 3)
        {
            PageNumber = 1;

            ParentText = Page1Text;

            ShowText();

        }



        //audio randomized

        Source.Stop();

        int randomNum = Random.Range(0, NextPageSound.Length);

        Source.PlayOneShot(NextPageSound[randomNum]);

        //audio randomized


    }

    public void PreviousPage()
    {

        if (PageNumber == 1)
        {
            PageNumber = 3;

            ParentText = Page3Text;

            ShowText();
        }
        else if (PageNumber == 2)
        {
            PageNumber = 1;

            ParentText = Page1Text;

            ShowText();

        }
        else if (PageNumber == 3)
        {
            PageNumber = 2;

            ParentText = Page2Text;

            ShowText();

        }


        //audio randomized

        Source.Stop();

        int randomNum = Random.Range(0, NextPageSound.Length);

        Source.PlayOneShot(NextPageSound[randomNum]);

        //audio randomized

    }


}
