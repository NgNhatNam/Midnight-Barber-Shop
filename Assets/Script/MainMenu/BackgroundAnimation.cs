using System.Collections;
using UnityEngine;
using static Unity.Cinemachine.CinemachineSplineRoll;

public class BackgroundAnimation : MonoBehaviour
{
    private Animator animator;

    private string currentAnimation;

    const string BACKGROUND = "Background";

    private AudioController audioController;

    private void Awake()
    {
        audioController = FindAnyObjectByType<AudioController>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        //StartCoroutine(PlayMusicPlaylist());
        audioController.PlayMusic(audioController.mainMenuMusic1, true);
        ChangeAnimationState(BACKGROUND);

    }

    IEnumerator PlayMusicPlaylist()
    {
        AudioClip[] playlist = new AudioClip[]
        {
        audioController.mainMenuMusic,
        audioController.mainMenuMusic1,
        audioController.mainMenuMusic2,
        audioController.mainMenuMusic3
        };

        int lastIndex = -1;

        while (true)
        {
            int index;

            do
            {
                index = Random.Range(0, playlist.Length);
            }
            while (index == lastIndex);

            lastIndex = index;

            audioController.PlayMusic(playlist[index], true);

            yield return new WaitForSeconds(playlist[index].length);
        }
    }


    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimation == newAnimation) return;
        animator.Play(newAnimation);
        currentAnimation = newAnimation;
    }
}
