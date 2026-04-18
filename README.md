# ClimbGame - Character Controller Prototype

A technical deep-dive into building a custom character controller without Unity's AnimationController. Demonstrates animation placement on interactive objects, climbing mechanics, and handling variable gravity directions.

---

## Key Innovation: No AnimationController

This project abandons Unity's traditional AnimationController in favor of:

- **Direct animation control via Playables API**
- **FSM-driven state transitions** with frame-perfect timing
- **Reusable animation system** that works for player AND interactive objects
- **Zero animation graph bloat** - clean, scriptable behavior

The controller is designed to be placed on any character or interactive object, making it truly universal.

---

## Technical Highlights

### Character Controller
- **Kinematic movement** based on KinematicCharacterController physics
- **Directional locomotion** - smoothly blends walk/run/strafe animations
- **Input-responsive rotation** with Slerp-based smooth turning
- **Custom gravity support** - works in any direction, not just down
- **Ground detection** with raycasting and collision checks

### Animation System
- **Playables API integration** for layer mixing and blending
- **Frame-accurate callbacks** via AnimationFrameEventsBehavior
- **LookAt IK** using AnimationScriptPlayable for head/body tracking
- **Climbing animation states** with smooth transitions
- **Per-object animation placement** - animate interactive objects with same system

### Interactive Objects
- **Animatable interactables** - doors, levers, platforms can use same animation system
- **Physics response** - damageable/pushable objects with force application
- **State-driven behavior** - FSM controls object animations just like characters

### Polish & Optimization
- **World-space UI** with billboard rotation corrected for variable gravity
- **Character nameplate system** with proper facing
- **Efficient animation updates** - only play what's visible
- **Performance-conscious design** - avoid unnecessary allocations

---

## Architecture

```
Scripts/
  Character/
    - KinematicCharacterController (physics movement)
    - BaseCharacterController (FSM + Playables base)
    - PlayerCharacterController (input handling)
    - AICharacterController (behavior tree)
  Animation/
    - PlayablesAnimatorController (layer mixing, blending)
    - AnimationFrameEventsBehavior (frame callbacks)
    - LookAtSystem (head tracking with IK)
    - LocomotionDirection (movement state blending)
  Interaction/
    - Interactable (base for interactive objects)
    - InteractionState (per-character state tracking)
    - SkeletonProfile (bone correction system)
  Climbing/
    - ClimbingState (ledge/wall climbing logic)
    - LedgeDetector (raycast-based edge detection)
    - ClimbingTransition (smooth climbing entry/exit)
  UI/
    - CharacterNameplateManager (world-space labels)
    - BillboardLabel (rotation tracking camera)
```

---

## Key Features

### Universal Controller
- Same animation controller works for:
  - Player character
  - NPCs / enemies
  - Mechanical objects (doors, gates, etc.)
  - Environmental interactables

### Climbing Mechanics
- Ledge detection via raycasting
- Smooth climbing transition animations
- Support for climbing in any gravity direction
- Pushable ledges and handholds

### Variable Gravity
- Character controller respects any gravity direction
- Animations play correctly regardless of world orientation
- IK system adapts to gravity changes
- UI elements remain readable in any orientation

### State-Driven Behavior
Each animation state has:
- Entry animation
- Exit animation
- Frame-accurate event timing
- Transition constraints (can't jump during attack, etc.)

---

## Design Decisions

### Why No AnimationController?
- **Flexibility**: Add new states without editing complex graphs
- **Reusability**: Same system for characters, enemies, interactive objects
- **Performance**: No overuse of layers or blend trees
- **Clarity**: Logic is in code, not scattered across state machines

### Playables API Benefits
- Direct animation blending at runtime
- Layer-based animation composition
- Frame-accurate event callbacks
- Lower memory footprint than AnimationController graphs

### Physics-Based Movement
- Kinematic controller gives precise control
- Gravity is applied as force, not baked into animation
- Climbing feels responsive and skill-based
- Environmental physics feel grounded

---

## How to Use

### Open & Play
```bash
git clone https://github.com/vadim-berceac/ClimbGame.git
cd ClimbGame
# Open in Unity 2022.3+
```

In Unity:
1. Open `Assets/Scenes/ClimbingScene.unity`
2. Press Play
3. Use `WASD` to move, `Space` to climb/jump, `Mouse` to look around

### Add Your Own Character
1. Create a new GameObject
2. Add `PlayerCharacterController` component
3. Assign animations in Inspector
4. Add `KinematicCharacterController` for physics
5. Done - it inherits the entire system

### Create Interactive Objects
1. Add `Interactable` component to object
2. Create animation states (open, close, etc.)
3. FSM handles the rest
4. Player can interact via the interaction system

---

## Notable Implementation Details

### AnimationFrameEventsBehavior
Enables frame-accurate events without AnimationEvents:
```csharp
public class AnimationFrameEventsBehavior : AnimationScriptPlayable
{
    public event Action<int> OnFrame;
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        int currentFrame = GetCurrentFrame();
        OnFrame?.Invoke(currentFrame);
    }
}
```

### Climbing State Transition
Seamless climbing entry with proper animation timing:
```csharp
public void StartClimbing(LedgeInfo ledge)
{
    // Animate to ledge position
    // Smooth animation blend during transition
    // Lock rotation until climbing state fully active
    // Fire frame events for grip timing
}
```

### LookAt IK System
Head tracking that respects gravity direction:
```csharp
public class LookAtSystem
{
    public void UpdateLookTarget(Vector3 targetPos)
    {
        // Corrects IK for current gravity direction
        // Applies bone corrections from SkeletonProfile
        // Clamps head rotation for realism
    }
}
```

---

## Performance Notes

- **Target**: 60 FPS on mid-range hardware
- **Memory**: Minimal allocations during gameplay
- **Animations**: Streamed from disk, not all loaded at once
- **Physics**: Kinematic movement avoids rigidbody overhead

Profiling tips:
- Use Unity Profiler to check animation update time
- Monitor Playables API allocation patterns
- Watch physics raycast count (climbing detection)

---

## Potential Extensions

- [ ] Networked climbing synchronization
- [ ] Procedural ledge generation
- [ ] Advanced parkour moves (wall-run, vaulting)
- [ ] Rope and vine climbing
- [ ] Multiplayer climbing puzzles
- [ ] Dynamic gravity zones with smooth transitions

---

## Code Quality

- **Clean Architecture**: Separation of concerns between physics, animation, and behavior
- **Extensible**: Add new animation states without touching core systems
- **Well-documented**: Comments explain WHY, not just WHAT
- **Performance-conscious**: Minimal garbage allocation during gameplay
- **Tested**: Handles edge cases like climbing in variable gravity

---

## License

Personal portfolio project. Free to reference and learn from.

---

## Contact

Vadim Berchak - Gameplay Programmer  
Telegram: https://t.me/Jabberwockyvb  
Email: grondus@gmail.com
