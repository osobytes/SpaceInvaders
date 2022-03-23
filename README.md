# Part 1
## Scope
Create a character that uses the main sprite on Space Invaders, and respond to user's input.
Move: Character will move left/right
Fire: Character will fire a projectile.


## Topics:
* Creating a New Unity Project
* Brief Overview of Folder Structure
* Package Manager
* Importing Sprites
* MonoBehaviour - Hero
* Sprite Renderer
* Handling Input
* Movement (Coordinates)
* Prefabs
* Fire (Spawning Prefabs)


### Creating a new Unity Project:
First we will create a new Unity Project, and select the 2D template.

### Folder Structure
In Assets folder we have to create the following folder structure:

```
Assets
|  Scenes
|  Scripts
|  Sprites
|  Prefabs
```

### Package Manager
Package Manager in Unity is used to fetch and get specific Unity Modules, that can be used to create behaviours and add plugins and extensions to Unity.
For the first part we will require a Unity Package called `Input System`

* go to Window -> Package Manager
* on the Top left of the new window select `Package Manager` as source.
* Go to the search tab to the right and search for `Input System`
* Install Input system by selecting it and clicking Install button.

### Importing Sprites
For this part we will require two sprites: `Hero.png` and `Bullet.png`
* Copy the two sprites to your **Sprites** folder.
* From the Unity UI find both sprites in the folder, and select them to set these options using the inspector:
* Apply Changes

## MonoBehaviour - Hero
In order to create the main hero of our story, we are going to rely on something in Unity called `GameObject`.
And we will override the behaviour of that `GameObject` by using a `MonoBehaviour`.

The GameObject will allow us to define an object with specific properties, which in this case will be the character the player is going to be using, which we'll be naming `Hero` from now on.

The `Hero` GameObject will have an homonimous MonoBehaviour which we will use to add the specific logic that will allow us to add the required logic for
this first part of the excercise.

## Sprite Renderer
In order for our `Hero` to have a visual presense in our game. We will be using one of the sprites we just recently imported: `Hero.png`
This sprite can be added to this Hero component by selecting the GameObject and choosing 'Add new component' on the inspector.
When the filter prompts, search for `Sprite Renderer` and add that component. You can then assign the `Hero.png` sprite to the component properties.

## Handling Input
In order for our Hero to react to the player's input, we need to add an `Player Input` component.
We can then modify this Player Input to our needs, and we can save the asset for later usage.
Once we have setup the Player Input in our Hero GameObject, we can then proceed to modify our Hero `MonoBehaviour` to react
to the user's input.