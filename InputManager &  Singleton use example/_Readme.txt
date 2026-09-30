Input Manager
A Singleton touch management system built for Unity's Input System that converts raw input into taps, holds, double-taps, and 8-way directional swipes.

Global Access: Easily referenced anywhere via InputManager.Instance.

State Machine: Manages dynamic transitions between touch, hold, and swipe states.

Event-Driven: Exposes clean C# events for actions and directional inputs without tight reference dependencies.

Singletons & SOLID Principles
Pros (Single Responsibility): Enforces a single global authority for managing a unique resource (like player input).

Cons (Coupling & Dependency Inversion): Violates loose coupling because other scripts depend directly on a global instance, making unit testing and code reuse harder.