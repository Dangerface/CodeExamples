Silly Frog Physics Mechanics
A collection of physics, procedural animation, and input management scripts powering a ragdoll-style "silly frog physics" game. The gameplay revolves around independently controlling the frog's legs, managing surface grips, predicting slopes, and executing physics-driven jumps.

Script Overview
BodyAlignment.cs: Aligns the frog's body to ground slopes, enforces air rotation limits, and predicts landing angles using raycast simulations.   

FootController.cs: Manages individual foot joints, extension limits, surface contact states, and dynamic mass changes between air and ground.   

FrogGrip.cs: Pulls the frog's body toward planted feet to simulate climbing or clinging physics.   

FrogLegController.cs & UpperLegController.cs: Procedurally solves two-bone Inverse Kinematics (IK) for the legs, scaling segments and adjusting knee angles dynamically.   

GamePadInputManager.cs: Captures dual-stick gamepad input, applies deadzones and axis inversions, and feeds movement data to the feet.   

JumpBoost.cs: Detects synchronized foot extensions and liftoff windows to trigger explosive jump forces and predictive body alignment.   

PlayerSetup.cs: Automatically sets up collision ignore rules between the body and foot colliders on startup. 