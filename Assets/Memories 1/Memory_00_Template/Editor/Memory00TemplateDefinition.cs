#if UNITY_EDITOR
using UnityEngine;
using BeyondTime.Memories.Building;

namespace BeyondTime.Memories.Content
{
    /// <summary>
    /// TEMPLATE: copy this whole folder to make a new memory.
    ///   1. Duplicate Memory_00_Template, rename it (e.g. Memory_05_Pride).
    ///   2. Rename this file and the class (e.g. Memory05PrideDefinition).
    ///   3. Change Id, Folder and Title below. Id must be unique.
    ///   4. Write your story and steps. Tools > Beyond Time > Memories > 2. Build All Memory Prefabs.
    /// Folders with "Template" in the name are not placed in the test world.
    /// </summary>
    public class Memory00TemplateDefinition : IMemoryDefinition, IMemoryLayout
    {
        public string Id => "memory_00_template";
        public string Folder => "Assets/Memories/Memory_00_Template";
        public string Title => "Template Memory";
        public float Radius => 14f;
        public float TimeLimit => 120f;

        public void Build(MemoryKit k)
        {
            var person = k.Actor("Someone", new Vector3(0f, 0f, 8f), new Color(0.6f, 0.6f, 0.7f), 180f);
            k.Fire(new Vector3(-9f, 0f, -8f));

            k.Narrate("Write what happened in this memory.");
            k.Narrate("Write why it still matters.");
            k.Defeat("Fight the shadows", "Write a line that sets up the fight.", 2);
            k.Reach(person.transform, 3f, "Reach them", "Write a line about reaching them.", 1);
            k.Hold(person.transform, 2.6f, 3f, "Hold E to help", "Help them", "Write a line about the action.");
            k.Choice("What do you do?", 10f, 1,
                MemoryKit.Right("The right thing.", "Write what changes."),
                MemoryKit.Wrong("The same wrong thing.", "Write the consequence.", 10f, 3, 6));
            k.Defeat("Survive the consequence", "Write a line for the wrong path.", 0);   // step 6

            k.Endings("Redeemed ending line.", "Condemned ending line.");
        }
    }
}
#endif
