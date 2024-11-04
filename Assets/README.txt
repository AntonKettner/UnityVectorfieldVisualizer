This project is for the visualization of Spinfields.

INPUT:
-automatically produced from create_json_from_numpy.py
-the .npy files have to be numbered with :04d convention like --> np.save(f"...{time:04d}", array) --> allows for correct order read in --> 001, 002, ... , 010, 011, ...
-json files, starting with spinfield_0 up to spinfield_n in the folder json_data inside Assets/StreamingAssets

Working better with changing vertical input to s, w and horizontal input to a, d --> not disrupting the Input to change the spinfield

SPINFIELD CONTROLS:
-next Spinfield: Arrow right --> current NO. of spinfield is shown in Debug
-prev Spinfield: Arrow left --> current NO. of spinfield is shown in Debug
-Play the entire Sequence: P
-Reverse Play: B

MOVEMENT CONTROLS:
- W, A, S, D --> Forward, Left, Backwards, Right
- Mousemovement --> Camera Angle

GENERAL Controls:
-Play button at top to start game/end game
-Change view (below Play) to Play Maximized or Play Focused


GENERALS about Unity/this project:
-each Game Object in Unity can have c# scripts attached, which can each have an Update (called each frame) and a Start function (called at Gamestart).
-currently there are 2 relevant scripts for displaying the data, one named Skyr_data_loader, one named Spin_Visualizer