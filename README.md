# 465Prototype

This is the Devlog

My initial prototype used a script called ShiftMover, which I have deprecated. This script was inside the platform and connected to the player, and called on my player script to move that platform. 
Because the script acted on the end of the platform it prevented the player from interacting with multiple platforms, it stayed locked to the first one the player interacted with.
I have brought in a first person controller script I have used on several projects in the past and modified it to interact with the platform in a way that perfectly conforms to all my initially stated goals and the requirements of the post mortem.

I was initially going to enable the player to hold Space and Control to move up and down respectively while connected to a platform, and make a deathplane that would end the game when it interacted with the player, but after seeing how the platforms perform without their Y position constrained I was forced to scrap this aspect of verticality. Every time I tried a new implementation of the player's connection or movement scheme for the platform I tried again to implement vertical movement and it either made the player and platform sink, 

I repeatedly failed to correctly make the player platform move- I attempted to make it a parent to the player so that the player would inherit its movement but the player's rigidbody would have to be disabled while connected to the platform and this would prevent collision for the player body.
Eventually, I passed the platform's rigidbody into the movement code instead of the player's, both rigidbodies still exist but the player is moving the platform's instead of its own, allowing the player to control one platform, disconnect, move to another, and control it without issue.
