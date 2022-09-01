# RobotCreator vs. CompositeObjectCreator

[RobotCreator](robot_creator.md) and [CompositeObjectCreator](composite_object_creator.md) are very similar and they have certain advantages and disadvantages over each other.

**For robotics simulations, you should *always* use RobotCreator.** RobotCreator creates prefabs that have ArticulationBody components. Unity's legacy Joint+Rigidbody articulation system has a lot of bugs; joints tend to stretch or move in unexpected ways, especially in complex joint chains. ArticulationBody components don't have these problems and allow for a far more stable simulation.

**For all other cases, you should use CompositeObjectCreator.** CompositeObjectCreator uses the legacy Joint+Rigidbody which, though prone to physics glitches, is far more flexible than ArticulationBodies.

You can think of ArticulationBody joint chains as "stable yet brittle" in three key ways:

1. If you parent one ArticulationBody to another at runtime, the program will crash. This is not true of Joint components, which can be dynamically adjusted.
2. There are many third-person modules for Unity that can interact with Joint components but not ArticulationBody components.
3. ArticulationBody components are always motorized, meaning they won't swing freely. Joint components can swing freely, meaning that they are better-suited for articulated objects such as a bucket with a handle.
