# 465Prototype

This is the Devlog

I failed to reduce scope. I coded the death plane in under an hour and decided to use the remaining 2 weeks to try and accomplish the p1 goal of vertical movement, but the way my movement code works is that the platforms all are static on the y plane, so they can't move up and down. Changing how the platforms move on a fundamental level in a way that lets the platforms move horizontally and vertically without issues is an incredibly difficult process. I continued trying to make this work until the last minute, and I failed to do so. I reverted the project to a functional state with a death plane that resets the positions of everything.

I will not attempt to include vertical movement in the final build, this project will progress without it.

The death plane code is very simple; when the player touches a collider it checks if the collider is the death plane's collider, and if it is the player reloads the current scene.
