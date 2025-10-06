using NUnit.Framework.Constraints;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PeeingSystem : MonoBehaviour
{

    private bool CanInteract = true;

    [SerializeField] private GameObject PeeingParticle;
    [SerializeField] private ParticleSystem PeeParticleSystem;

    [SerializeField] private AudioSource PeeingSound;
    [SerializeField] private AudioSource ZipSource;
    [SerializeField] private AudioClip[] ZipSounds; // 0 zip down, 1 zip up

    [SerializeField] private FirstPersonController  FPSController;



    // Update is called once per frame
    void Update()
    {
        if (CanInteract == true)
        {
            if(Input.GetMouseButtonDown(0))
            {
                Ray ray = new Ray(transform.position, transform.forward);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 10f))
                {

                    if(hit.collider.CompareTag("Toilet"))
                    {


                        //do your thing

                        StartCoroutine(Pee());

                    }
                }
            }
        }
    }

    IEnumerator Pee()
    {
        CanInteract = false;
        ZipSource.PlayOneShot(ZipSounds[0]);
        FPSController.enabled = false;

        yield return new WaitForSeconds(0.5f);

        PeeingParticle.SetActive(true);
        PeeingSound.Play();

        yield return new WaitForSeconds(5f);

        PeeingSound.Stop();
        ZipSource.PlayOneShot(ZipSounds[1]);
        PeeParticleSystem.Stop();


        yield return new WaitForSeconds(1f);

        PeeingParticle.SetActive(false);
        FPSController.enabled = true;
        CanInteract = true;

    }
}
