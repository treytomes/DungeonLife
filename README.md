# Dungeon Life

An artificial life terrarium that takes place in a roguelike-inspired dungeon.

## Controls

Click-and-drag with the middle mouse button on the world display to move it around.  Or you can use the scrollbars.

Click on a cell to load the cell and entity details into the side panel.

The toolbar at the top of the screen can be used to run the simulation, pause it, and then step it one tick at a time.  It also displays the current world time.  One minute passes for each tick.

## Environment

The "dungeon" is a 256x256 tile room filled using a cellular-automata based cavern generator, which I have modified to generate pools of water.
The border wall will emit either heat or cold throughout the day.  The heat dissipates into the room. When water begins to heat, it lets off steam,
which will raise the humidity that dissipates throughout the room.  There are stone pillars scattered around the room that will slow down the passage of heat.
Water will also slow the spread of heat.

### TODO
* It would be neat if the entire world wrapped around on itself, and the UI allowed the user to keep scrolling in one direction forever.
  I would need to reconfigure the heat sources to be in the middle of the space instead of the perimeter, since there would be no perimeter.
  Maybe convert a couple of the stone regions to border blocks?

## Lifeforms

### Algae

The algae cells are at the bottom of the food chain.  They roughly follow the rules of Conway's Game of Life, modified to use floating point values.
The cells are affected by temperature and humidity.  Too hot and they burn, too cold and they stop moving.

#### TODO
* Increased growth by feeding on oink corpses.

### Oinks

* Oinks feed on algae.
* They get hungry fast, and thirsty a bit slower.
* They will move in flocks following the Boids algorithm.
* They die.  Lifespan is 12 days, then each day the dice is rolled to see if that will be the day they die.  The dice is more weighted with each passing day.
* They drop useless corpses when they die.
* They grow up, at 1/4 of their life span.
* There are boy oinks and girl oinks.  We'll get back to that later.

#### TODO
* Reproductive urge, temperature sensitivity, wetness sensitivity, sense of smell (partially implemented).

#### Sense of smell?

We can see far into the distance, but not around corners.  We can smell around corners, but not long-distance.  How best to represent this fact?

## Toolchain

This project will be written in C#, currently in .NET 5.0, using the SadConsole library for graphics.
NAudio is referenced in hopes of eventually adding some ambient noise to the environment.
ComputeSharp is also referenced.  When I add z-levels to the dungeon I'm probably going to need to offload some of the processing onto the GPU.

## References

* [SadConsole Docs](https://sadconsole.com/)
* [SadConsole Git](https://github.com/Thraka/SadConsole)
* [RogueSharp Git](https://github.com/FaronBracy/RogueSharp)
* [GoRogue Git](https://github.com/Chris3606/GoRogue#doryen-library-libtcod)
* [Boids](https://en.wikipedia.org/wiki/Boids)
