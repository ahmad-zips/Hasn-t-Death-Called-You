using UnityEngine;
using UnityEngine.Video;
using TMPro;

namespace BeyondTime.Game
{
    /// <summary>Every kind of thing in the game that you can swap for your own asset.</summary>
    public enum AssetSlot
    {
        None,
        // Environment
        Lava, Ground, Wall, Fire, FireBurst, FireEmber, Bridge, Portal,
        // Props
        Knife, Gate, Lever, Rubble, EscapeDoor,
        // Characters
        HumanVictim, HumanAttacker, HumanLookout, HumanFriend,
        ShadowPursuer, ShadowGatekeeper, ShadowHarrier,
        // Paradise
        ParadiseGround, ParadiseTree, ParadiseFlower, ParadiseRock, ParadiseWater, ParadiseShrine
    }

    public enum UISlot { None, MenuBackground, Logo, Panel, Button, PauseBackground, GameOverBackground, VictoryBackground, ResultCard, VideoPlaceholder, HudFrame }
    public enum MusicSlot { None, Menu, World, Paradise }

    /// <summary>
    /// THE ONE PLACE TO DRAG YOUR ASSETS.
    /// Select Assets/Memories/Resources/BeyondTimeAssets.asset and drag models, materials, effects, sounds,
    /// sprites and the intro video into the fields. Empty fields keep the built-in placeholders.
    /// Changes show up the next time you press Play: no rebuild needed.
    /// To replace just ONE object, use the Override fields on that object's Replaceable Asset component instead.
    /// </summary>
    [CreateAssetMenu(menuName = "Beyond Time/Asset Library", fileName = "BeyondTimeAssets")]
    public class BeyondTimeAssets : ScriptableObject
    {
        [Header("ENVIRONMENT  (prefab = replace the object, material = repaint the placeholder)")]
        public Material lavaMaterial;
        public GameObject lavaPrefab;
        public Material groundMaterial;
        public Material wallMaterial;
        [Tooltip("Ambient fires, braziers, spreading hellfire.")] public GameObject firePrefab;
        [Tooltip("The burst of a timed fire hazard (Siege walkway).")] public GameObject fireBurstPrefab;
        [Tooltip("Embers that warn before a fire hazard ignites.")] public GameObject fireEmberPrefab;
        [Tooltip("Stretched to fit each bridge between islands.")] public GameObject bridgePrefab;
        [Tooltip("The final gate's portal.")] public GameObject portalPrefab;
        public Material hellSkybox;

        [Header("PROPS")]
        public GameObject knifePrefab;
        public GameObject gatePrefab;
        public GameObject leverPrefab;
        public GameObject rubblePrefab;
        public GameObject escapeDoorPrefab;
        [Tooltip("The Harrier's projectile. Face +Z.")] public GameObject projectilePrefab;

        [Header("HUMANS  (humanoid models; Rig = Humanoid)")]
        [Tooltip("Used for every human whose own slot below is empty.")] public GameObject humanModel;
        [Tooltip("Animator Controller for humans. Needs a Speed float (0 idle, 2 walk, 6 run). Empty = Starter Assets controller.")]
        public RuntimeAnimatorController humanAnimator;
        public GameObject victimModel;
        public GameObject attackerModel;
        public GameObject lookoutModel;
        public GameObject friendModel;

        [Header("SHADOW ENEMIES")]
        [Tooltip("Used for every shadow whose own slot below is empty.")] public GameObject shadowModel;
        [Tooltip("Repaints the placeholder shadows if you have no shadow model yet.")] public Material shadowMaterial;
        public RuntimeAnimatorController shadowAnimator;
        public GameObject pursuerModel;
        public GameObject gatekeeperModel;
        public GameObject harrierModel;

        [Header("PARADISE")]
        public Material paradiseGroundMaterial;
        public GameObject treePrefab;
        public GameObject flowerPrefab;
        public GameObject paradiseRockPrefab;
        public Material waterMaterial;
        public GameObject shrinePrefab;
        public Material paradiseSkybox;

        [Header("VIDEO")]
        [Tooltip("The grave intro video. Empty = a placeholder title card plays instead.")]
        public VideoClip introVideo;

        [Header("MUSIC AND STINGS")]
        public AudioClip menuMusic;
        public AudioClip worldMusic;
        public AudioClip paradiseMusic;
        public AudioClip memoryRedeemedSting;
        public AudioClip memoryCollapsedSting;
        public AudioClip soulLostSting;
        public AudioClip gameOverSting;
        public AudioClip buttonClick;

        [Header("COMBAT SOUNDS (used by every fighter that has none of its own)")]
        public AudioClip[] punchHits;
        public AudioClip blockSound;
        public AudioClip whooshSound;
        public AudioClip[] footsteps;

        [Header("UI  (sprites: Texture Type = Sprite (2D and UI))")]
        public TMP_FontAsset font;
        public Sprite menuBackground;
        public Sprite logo;
        [Tooltip("9-sliced panel frame.")] public Sprite panelSprite;
        [Tooltip("9-sliced button.")] public Sprite buttonSprite;
        public Sprite pauseBackground;
        public Sprite gameOverBackground;
        public Sprite victoryBackground;
        public Sprite resultCardBackground;
        public Sprite videoPlaceholderImage;
        public Sprite hudFrame;

        // ------------------------------------------------------------ lookup

        static BeyondTimeAssets cached;
        public static BeyondTimeAssets Load()
        {
            if (!cached) cached = Resources.Load<BeyondTimeAssets>("BeyondTimeAssets");
            return cached;
        }

        static GameObject Or(GameObject a, GameObject b) => a ? a : b;

        public GameObject GetPrefab(AssetSlot s)
        {
            switch (s)
            {
                case AssetSlot.Lava: return lavaPrefab;
                case AssetSlot.Fire: return firePrefab;
                case AssetSlot.FireBurst: return fireBurstPrefab;
                case AssetSlot.FireEmber: return fireEmberPrefab;
                case AssetSlot.Bridge: return bridgePrefab;
                case AssetSlot.Portal: return portalPrefab;
                case AssetSlot.Knife: return knifePrefab;
                case AssetSlot.Gate: return gatePrefab;
                case AssetSlot.Lever: return leverPrefab;
                case AssetSlot.Rubble: return rubblePrefab;
                case AssetSlot.EscapeDoor: return escapeDoorPrefab;
                case AssetSlot.HumanVictim: return Or(victimModel, humanModel);
                case AssetSlot.HumanAttacker: return Or(attackerModel, humanModel);
                case AssetSlot.HumanLookout: return Or(lookoutModel, humanModel);
                case AssetSlot.HumanFriend: return Or(friendModel, humanModel);
                case AssetSlot.ShadowPursuer: return Or(pursuerModel, shadowModel);
                case AssetSlot.ShadowGatekeeper: return Or(gatekeeperModel, shadowModel);
                case AssetSlot.ShadowHarrier: return Or(harrierModel, shadowModel);
                case AssetSlot.ParadiseTree: return treePrefab;
                case AssetSlot.ParadiseFlower: return flowerPrefab;
                case AssetSlot.ParadiseRock: return paradiseRockPrefab;
                case AssetSlot.ParadiseShrine: return shrinePrefab;
                default: return null;
            }
        }

        public Material GetMaterial(AssetSlot s)
        {
            switch (s)
            {
                case AssetSlot.Lava: return lavaMaterial;
                case AssetSlot.Ground: return groundMaterial;
                case AssetSlot.Wall: return wallMaterial;
                case AssetSlot.ShadowPursuer:
                case AssetSlot.ShadowGatekeeper:
                case AssetSlot.ShadowHarrier: return shadowMaterial;
                case AssetSlot.ParadiseGround: return paradiseGroundMaterial;
                case AssetSlot.ParadiseWater: return waterMaterial;
                default: return null;
            }
        }

        public RuntimeAnimatorController GetController(AssetSlot s)
        {
            if (IsShadow(s)) return shadowAnimator ? shadowAnimator : humanAnimator;
            if (IsHuman(s)) return humanAnimator;
            return null;
        }

        public Sprite GetSprite(UISlot s)
        {
            switch (s)
            {
                case UISlot.MenuBackground: return menuBackground;
                case UISlot.Logo: return logo;
                case UISlot.Panel: return panelSprite;
                case UISlot.Button: return buttonSprite;
                case UISlot.PauseBackground: return pauseBackground;
                case UISlot.GameOverBackground: return gameOverBackground;
                case UISlot.VictoryBackground: return victoryBackground;
                case UISlot.ResultCard: return resultCardBackground;
                case UISlot.VideoPlaceholder: return videoPlaceholderImage;
                case UISlot.HudFrame: return hudFrame;
                default: return null;
            }
        }

        public AudioClip GetMusic(MusicSlot s)
        {
            switch (s)
            {
                case MusicSlot.Menu: return menuMusic;
                case MusicSlot.World: return worldMusic;
                case MusicSlot.Paradise: return paradiseMusic;
                default: return null;
            }
        }

        public static bool IsHuman(AssetSlot s) => s == AssetSlot.HumanVictim || s == AssetSlot.HumanAttacker || s == AssetSlot.HumanLookout || s == AssetSlot.HumanFriend;
        public static bool IsShadow(AssetSlot s) => s == AssetSlot.ShadowPursuer || s == AssetSlot.ShadowGatekeeper || s == AssetSlot.ShadowHarrier;
        public static bool IsCharacter(AssetSlot s) => IsHuman(s) || IsShadow(s);
    }
}
