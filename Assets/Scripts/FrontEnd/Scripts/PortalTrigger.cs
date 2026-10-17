using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeyondTime.FrontEnd
{
    /// <summary>Loads the next scene when the player walks into the portal.</summary>
    [RequireComponent(typeof(Collider))]
    public class PortalTrigger : MonoBehaviour
    {
        public string sceneToLoad = FrontEndScenes.Briefing1;
        public FlowLoader loader;           // uses its fade if set
        public AudioSource enterSound;

        bool used;

        void Start() { GetComponent<Collider>().isTrigger = true; }

        void OnTriggerEnter(Collider other)
        {
            if (used || !other.CompareTag("Player")) return;
            used = true;
            if (enterSound) enterSound.Play();

            if (loader) loader.LoadScene(sceneToLoad);
            else { Time.timeScale = 1f; SceneManager.LoadScene(sceneToLoad); }
        }
    }
}
