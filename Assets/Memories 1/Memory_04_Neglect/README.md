# Memory 04: The Unanswered Call

**Wrong being corrected:** you ignored your mother's calls the night she fell ill.
**New mechanics:** hold your ground in a circle of light while shadows keep coming. Leaving the light pauses the countdown.
**Time limit:** 150 s. **Island radius:** 15 m.

## Script
| # | Step | Story at the bottom | Objective | Shadows |
|---|---|---|---|---|
| 0 | Narrate | Your mother called you eleven times the night she fell ill. | | |
| 1 | Narrate | You saw every call. You told yourself you would call back tomorrow. | | |
| 2 | Survive 25 s inside a 4 m light | The phone is ringing. Stay in its light. Do not let the dark pull you away. | Stay with the ringing phone 25s (or "Return to the light!") | keeps 4 alive |
| 3 | Reach door | The ringing stops. Go to her house. | Go to her | 2 (porch) |
| 4 | Hold E 3 s | Your hand is shaking. Open it anyway. | Open the door | |
| 5 | Choice, 10 s, default 3 (door opens) | **She looks up when you walk in. What do you say?** | | |
| | 1. I am here now. I am staying. | RIGHT: She smiles. She does not ask where you were. She just holds your hand. | ends: REDEEMED | |
| | 2. I was busy. You know how it is. | WRONG: The excuse sounds smaller out loud. | -10 s, go to 6 | +3 |
| | 3. Check your phone. (default) | WRONG: You look away from her. Again. | -10 s, go to 6 | +3 |
| 6 | Defeat | She waited for you. Now you wait for this to end. | Face what you left behind | from the choice |

**Redeemed:** You stayed. Some calls can still be answered.
**Condemned:** You came, but you were never really there. The memory stays cold.

## Placeholders to replace
- `Actors/YourMother/MODEL_REPLACE_ME`: mother, sitting inside the house. The choice step uses her as the actor; fill **Actor State** for her looking up.
- `ART_REPLACE_ME/PHONE_REPLACE_ME`, `Table`, `LightCircle_REPLACE_ME`, `PhoneLight`: the ringing phone and its pool of light. Keep `Targets/PhoneSpot` at the centre; the radius is 4 m.
- `ART_REPLACE_ME/DOOR_REPLACE_ME`: the front door (hidden when the choice starts), plus the house walls.
- `FIRE_REPLACE_ME`: island fires.
