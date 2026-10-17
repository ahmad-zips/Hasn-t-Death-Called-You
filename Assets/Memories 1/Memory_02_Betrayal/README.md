# Memory 02: The Secret You Sold

**Wrong being corrected:** you sold your best friend's secret for a promotion.
**New mechanics:** collect items guarded by shadows, escort a person who loses health while shadows are near her.
**Time limit:** 150 s. **Island radius:** 15 m.

## Script
| # | Step | Story at the bottom | Objective | Shadows |
|---|---|---|---|---|
| 0 | Narrate | You promised your best friend you would keep her secret. | | |
| 1 | Narrate | Two days later, you traded it for a promotion. She found out from a stranger. | | |
| 2 | Collect 3 pieces | Her letter to you was torn apart that night. The shadows guard every piece. | Gather the pieces of her letter 0 / 3 | 3 (guards) |
| 3 | Reach friend | She is waiting at the far end. She does not know if she can trust you. | Bring the letter back to her | 2 |
| 4 | Escort to the light | She will follow you. Keep the shadows away from her. | Lead her to the light (Friend 100%) | 3 (flank, half hunt her) |
| 5 | Choice, 10 s, default 2 | **Your manager offers you a way to stay quiet. What do you do?** | | |
| | 1. Tell everyone the truth. It was me. | RIGHT: Her eyes fill, but she nods. Trust is slow, but it starts here. | ends: REDEEMED | |
| | 2. Keep the promotion. She will never know. (default) | WRONG: You chose the lie again. Lies need defending. | -10 s, go to 6 | +4 |
| 6 | Survive 15 s | Every lie needs someone to defend it. Survive. | Defend the lie | keeps 4 alive |

If the friend's health reaches 0 during the escort, the memory collapses: "Your Friend was taken by the shadows."

**Redeemed:** You told the truth. It cost you something real, and it gave you something back.
**Condemned:** You kept the promotion. The memory holds, but it is hollow.

## Placeholders to replace
- `Actors/YourFriend/MODEL_REPLACE_ME`: friend model. Fill Memory NPC **Walk State**, **Idle State** and **Hurt State**. Add a NavMeshAgent once the world has a NavMesh for smarter following.
- `Collectibles/LetterPieces/LetterPieces_1..3/MODEL_REPLACE_ME`: torn paper pieces.
- `ART_REPLACE_ME/LanternBase_REPLACE_ME` and `Targets/TheLight`: the lantern she must reach.
- `ART_REPLACE_ME`: desks, pillars. `FIRE_REPLACE_ME`: island fires.
