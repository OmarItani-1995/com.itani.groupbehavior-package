# com.itani.groupbehavior-package
A customizable group behavior system

A group formation system, when the enemy group is initialized, a vote for leader starts, when leader is selected, his custom formation is used. 
When leader dies, a new leader is voted and process repeats. 

SETUP: 
Script Creations: 
    Window->Group Behavior->Behavior Group Creator
    Enter Target script name, User script name, Manager script name, then select the folder location, and it'll create all the needed scripts

Initializations: 
    Custom: Call Subscribe/Unsubscribe for Target scripts and User scripts manual in the created Target and User scripts 
    Group on Trigger: Create a monobehavior class overriding PlayerEnemyGroupCreator, setup as a trigger area with references to the users 

Formations: 
    You can create any formation with it's scriptable object creator, override Formation, FormationSO