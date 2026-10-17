# Memory 01: The Moment of Anger (tutorial)

**Wrong being corrected:** you hit your younger brother in a fit of anger.
**New mechanics:** fighting, reaching a person, hold E while shadows try to break your focus, a timed choice.
**Time limit:** 120 s. **Island radius:** 14 m (entrance at -Z).

## Script
| # | Step | Story at the bottom | Objective | Shadows |
|---|---|---|---|---|
| 0 | Narrate | This is the night you hit your brother. He only asked you to stop shouting. | | |
| 1 | Narrate | The shadows are your anger. They want this night to end the same way it did. | | |
| 2 | Defeat | Left mouse to strike. Space to dodge. Q to repel. Every hit you take costs you time. | Fight back the shadows of your anger | 2 (front) |
| 3 | Reach brother (3 m) | He is still on the ground. Reach him before the moment repeats. | Reach your brother | 2 (mid) |
| 4 | Hold E 3 s | Kneel. If a shadow strikes you, you lose your focus. | Kneel beside him | |
| 5 | Choice, 10 s, default 3 | **He looks up at you. What do you say?** | | |
| | 1. I was wrong. I am so sorry. | RIGHT: His shoulders loosen. For the first time in years, the street is quiet. | ends: REDEEMED | |
| | 2. You made me do it. | WRONG: The same words as that night. The shadows grow stronger. | -10 s, go to 6 | +3 (back) |
| | 3. Say nothing. (default) | WRONG: Silence fills the space where your apology should have been. | -5 s, go to 6 | +2 (back) |
| 6 | Defeat | The wrong repeats. Fight your way out of it. | Survive what your anger became | from the choice |

**Redeemed:** You knelt, and you said it. The anger has nothing left to hold.
**Condemned:** You survived the memory, but you said it again. The anger remembers.

## Placeholders to replace
- `Actors/YourBrother/MODEL_REPLACE_ME`: brother model, ideally sitting or kneeling on the ground. Fill an **Actor State** on step 4 for a reaction animation.
- `ART_REPLACE_ME`: street (HouseLeft, HouseRight, crates, lamp post), ground and molten rim.
- `FIRE_REPLACE_ME`: three island fires.
