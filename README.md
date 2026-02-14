HW2 – The Scope and Grabbing
Student: Prakash Karkee
Unity Version: Unity 6.2.x (6000.2.9f1)
XR: OpenXR + XR Interaction Toolkit

GitHub Repository:
https://github.com/prakashkarkee/XR_Project_HW2_scope

What to test :
1) Magnifying lens (scope):
   - The magnifying lens can be grabbed.
   - Looking through the lens shows magnified view.
2) Hidden object: Green cube
   - A hidden object is invisible normally but visible through the lens.
3) Combined 2-hand grab (CapsulePlay):
   - Combined translation from both controllers.
   - Combined rotation from both controllers.
   - Rotation happens around the manipulating controller / midpoint.
   - Double rotation toggle (Extra): press "R" to toggle.

How to run:
1) Unzip the submission folder named "Build".
2) Run: XR_HW2.exe
3) Put on headset / make sure OpenXR runtime is active.

Build contents included:
- XR_HW2.exe
- XR_HW2_Data (folder)
- MonoBleedingEdge (folder)
- UnityPlayer.dll
- Demo video file: XR_HW2 - prakscene - Windows, Mac, Linux - Unity 6.2 (6000.2.9f1) _DX11_ 2026-02-14 23-39-39.mp4

Notes / Special Instructions:
- The 2-hand grabbing is implemented on the object named "CapsulePlay" (sky material) in the scene.
- The magnifying lens uses a RenderTexture (LensRT) rendered by LensCamera.
- If the lens appears black from one side, a back-facing lens plane is included to ensure visibility.

Controls:
- Grab: Controller grip (default XRI binding)
- Toggle double rotation (EC): Keyboard "R" (while app is running)
- Lighting Environment (Environment pleasing) : Keyboard "spacebar"   
- Exit button : "A" on controller
