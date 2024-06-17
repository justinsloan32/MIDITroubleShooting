using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Minis;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.iOS;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class TestSceneMIDI : MonoBehaviour
{
    public Image greenMeter;
    public Button A0Button;
    public Button B0Button;
    public Button Bb0Button;
    [SerializeField] private List<GameObject> imageList;
    private GameObject currentActiveImage;
    public GameObject A0Img;
    public GameObject B0Img;
    public GameObject Bb0Img;
    public bool A0isActive;
    public bool B0isActive;
    public bool Bb0isActive;
    public List<GameObject> objectsToShake;
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.1f;
    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();
    public int spritesToRemove = 5; // The number of sprites to remove
    public List<GameObject> activeSprites; // The list of active sprites
    public Animator MachineAnimator;
    public Animator GreenBarAnimator;
    public SpriteSpawnScript spriteSpawnScript;
    private int spaceBarInputs = 0;
    private float firstInputTime;
    private float fireActivatedTime;
    public bool fireActivated = false;
    private int inputsAfterFire = 0;
    public float fillRate = .05f; // Adjust this to control the normal fill rate
    public float fireFillRate = .1f; // Adjust this to control the fill rate during fire mode
    public GameObject AngelsSinging;
    public GameObject BlazingModeImage;

    private Vector3 originalPosition;

    private void Awake()
    {
        originalPosition = transform.position;
    }

    public Vector3 GetOriginalPosition()
    {
        return originalPosition;
    }

    void ProcessKeys()
    {
        {
            if (spaceBarInputs > 0 && Time.time - firstInputTime > 4)
            {
                // Reset inputs if more than 4 seconds have passed since the first input
                spaceBarInputs = 0;
            }

            CheckInputAndFillAmount();

            if (!fireActivated && spaceBarInputs >= 3)
            {
                // If fire is not already active, activate it
                fireActivated = true;
                fireActivatedTime = Time.time;
                ActivateFireAnimation();
                fillRate *= 3; // Increase the fill rate

                // Reset inputs for next cycle
                spaceBarInputs = 0;
                firstInputTime = Time.time;
                AngelsSinging.SetActive(true);
                BlazingModeImage.SetActive(true);
            }
        }

        if (fireActivated)
        {
            if (Time.time - fireActivatedTime <= 6 && spaceBarInputs >= 3)
            {
                // Reset the activation time if three inputs have been made within the last six seconds
                fireActivatedTime = Time.time;
                spaceBarInputs = 0;
            }
            else if (Time.time - fireActivatedTime > 6)
            {
                // If more than 6 seconds have passed since fire activation, deactivate fire
                fireActivated = false;
                ResetFireAnimation();
                fillRate /= 3; // Reset fill rate to original
                AngelsSinging.SetActive(false);
                BlazingModeImage.SetActive(false);
                spaceBarInputs = 0;
            }
        }
    }

    void ResetFireAnimation()
    {
        foreach (GameObject sprite in spriteSpawnScript.activeSprites)
        {
            Animator animator = sprite.GetComponent<Animator>();
            if (animator != null)
            {

                animator.Play("MainState");
            }
        }
    }

    void ActivateFireAnimation()
    {
        float remainingFireTime = Mathf.Max(0, 8 - (Time.time - fireActivatedTime));
        fireActivated = true;
        foreach (GameObject sprite in spriteSpawnScript.activeSprites)
        {
            Animator animator = sprite.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("OnFire");
                // Set the speed of the animation based on the remaining time
                animator.speed = animator.GetCurrentAnimatorStateInfo(0).length / remainingFireTime;
            }
        }
    }
    void ShakeObjects()
    {
        foreach (GameObject obj in objectsToShake)
        {
            StartCoroutine(Shake(obj));
        }
    }
    void Start()
    {
        foreach (GameObject image in imageList)
        {
            image.SetActive(false);

        }

        int randomIndex = Random.Range(0, imageList.Count);
        imageList[randomIndex].SetActive(true);
        greenMeter.fillAmount = 0f;
    }
    void Update()
        {
            GameObject A0Img = imageList[0];
            GameObject B0Img = imageList[2];
            GameObject Bb0Img = imageList[1];
            bool note21Pressed = false;
            bool note22Pressed = false;
            bool note23Pressed = false;
            foreach (GameObject obj in objectsToShake)
            {
                originalPositions[obj] = obj.transform.position;
            }

            InputSystem.onDeviceChange += (device, change) =>
            {
                if (change != InputDeviceChange.Added) return;

                var midiDevice = device as MidiDevice;





                midiDevice.onWillNoteOn += (note, velocity) =>
                {


                    
                    if (note.noteNumber == 21)
                    {
                        note21Pressed = true;
                        ExecuteEvents.Execute(A0Button.gameObject, new PointerEventData(EventSystem.current),
                            ExecuteEvents.submitHandler);
                        A0isActive = A0Img.activeSelf;
                        if (A0isActive)
                        {
                            A0Img.SetActive(false);
                            int randomIndex = Random.Range(0, imageList.Count);
                            imageList[randomIndex].SetActive(true);
                            MachineAnimator.SetTrigger("NextMachineAnimation");
                            ProcessKeys();
                            RemoveSprites(10);
                            foreach (GameObject obj in objectsToShake)
                            {
                                StartCoroutine(Shake(obj));
                            }



                        }

                        return;
                    }

                    if (note.noteNumber == 22)
                    {

                        note22Pressed = true;
                        ExecuteEvents.Execute(Bb0Button.gameObject, new PointerEventData(EventSystem.current),
                            ExecuteEvents.submitHandler);
                        Bb0isActive = Bb0Img.activeSelf;
                        if (Bb0isActive)
                        {

                            Bb0Img.SetActive(false);
                            int randomIndex = Random.Range(0, imageList.Count);
                            imageList[randomIndex].SetActive(true);
                            MachineAnimator.SetTrigger("NextMachineAnimation");
                            ProcessKeys();
                            RemoveSprites(10);
                            foreach (GameObject obj in objectsToShake)
                            {
                                StartCoroutine(Shake(obj));
                            }

                        }

                        return;



                    }

                    if (note.noteNumber == 23)
                    {
                        MachineAnimator.SetTrigger("NextMachineAnimation");
                        ProcessKeys();

                        note23Pressed = true;
                        ExecuteEvents.Execute(B0Button.gameObject, new PointerEventData(EventSystem.current),
                            ExecuteEvents.submitHandler);
                        B0isActive = B0Img.activeSelf;

                        if (B0isActive)
                        {

                            B0Img.SetActive(false);
                            int randomIndex = Random.Range(0, imageList.Count);
                            imageList[randomIndex].SetActive(true);
                            MachineAnimator.SetTrigger("NextMachineAnimation");
                            ProcessKeys();
                            RemoveSprites(10);
                            foreach (GameObject obj in objectsToShake)
                            {
                                StartCoroutine(Shake(obj));
                            }

                        }

                        return;


                    }

                };
            };
        }

    void CheckInputAndFillAmount()
    {
        if (spaceBarInputs == 0)
        {
            firstInputTime = Time.time;
        }

        spaceBarInputs++;
        float currentFillRate = fireActivated ? fireFillRate : fillRate;

        float divisor = 2f;
        greenMeter.fillAmount = Mathf.Clamp(greenMeter.fillAmount + (currentFillRate / divisor), 0f, 1f);
    }

    IEnumerator Shake(GameObject obj)
    {
        CMjrGSMIDI_LGP op = obj.GetComponent<CMjrGSMIDI_LGP>();
        Vector3 originalPosition = (op != null) ? op.GetOriginalPosition() : obj.transform.position;

        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            obj.transform.position = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = originalPosition;
    }

    IEnumerator PlayRemovalAnimation(GameObject spriteToRemove)
    {
        Animator animator = spriteToRemove.GetComponent<Animator>();
        animator.gameObject.GetComponent<AudioSource>().Play();
        animator.SetTrigger("Remove");

        // Wait for the animation to finish
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);


        Destroy(spriteToRemove);
        yield return null; // add this line because a coroutine must yield return somewhere
    }

    void RemoveSprites(int numberToRemove)
    {
        // Make sure we don't try to remove more sprites than exist
        numberToRemove = Mathf.Min(numberToRemove, spriteSpawnScript.activeSprites.Count);

        for (int i = 0; i < numberToRemove; i++)
        {
            // Get the first sprite in the list
            GameObject spriteToRemove = spriteSpawnScript.activeSprites[0];

            // Remove the sprite from the list of active sprites
            spriteSpawnScript.activeSprites.RemoveAt(0);

            // Play the removal animation, then destroy the sprite
            StartCoroutine(PlayRemovalAnimation(spriteToRemove));
        }

        


         

        // Update is called once per frame
        
    }
}
