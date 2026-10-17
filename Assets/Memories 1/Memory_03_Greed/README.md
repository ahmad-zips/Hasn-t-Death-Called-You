# Memory 03: The Wallet

**Wrong being corrected:** you kept an old man's wallet that held the money for his medicine.
**New mechanics:** a real branch. Returning the wallet is a walk across the island; keeping it is a hard fight. Both end in a timed escape through fire.
**Time limit:** 140 s (+10 s at the escape). **Island radius:** 15 m.

## Script
| # | Step | Story at the bottom | Objective | Shadows |
|---|---|---|---|---|
| 0 | Narrate | You found an old man's wallet on the bus. Inside was the money for his medicine. | | |
| 1 | Narrate | His name and address were inside it. You kept it anyway. | | |
| 2 | Reach wallet | It is still where you hid it, guarded by what you became. | Find the wallet | 3 (guards) |
| 3 | Hold E 2 s | It feels heavier than it should. | Pick up the wallet | |
| 4 | Choice, 8 s, default 2 | **The old man is still waiting at the bus stop. What do you do with it?** (the wallet disappears from the stand) | | |
| | 1. Return it to him. | RIGHT: You turn toward the bus stop. | go to 5 | |
| | 2. Keep it. Nobody will know. (default) | WRONG: Your hand closes around it. Greed has teeth. | go to 7 | +4 |
| 5 | Reach old man | He is still at the stop, checking his pockets again and again. | Return the wallet to the old man | 2 |
| 6 | Hold E 3 s, then jump to 8 | Put it in his hands. Look him in the eye. | Give it back | |
| 7 | Defeat (wrong path only) | It is heavier than it looks. It always was. | Greed fights to keep you | from the choice |
| 8 | Reach exit, fire wall on, +10 s | The memory is burning. Run. | Escape before the fire takes the memory | 2 |

**Redeemed:** He did not recognise you. He only said thank you. That was enough.
**Condemned:** You escaped with the wallet. It weighs more every step.

## Placeholders to replace
- `Actors/TheOldMan/MODEL_REPLACE_ME`: old man, standing at the bus stop. Step 6 has him as the actor; fill **Actor State** for a thank-you animation.
- `StepObjects/WALLET_REPLACE_ME`: the wallet on the hiding place.
- `StepObjects/EscapeFire/Fire_REPLACE_ME` ×6: fire on the escape route (hidden until step 8).
- `ART_REPLACE_ME`: bus shelter, bench, hiding place, rubble. `FIRE_REPLACE_ME`: island fires.
