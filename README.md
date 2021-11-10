# Dungeon Life

An artificial life terrarium that takes place in a roguelike-inspired dungeon.

## Controls

Click-and-drag with the middle mouse button on the world display to move it around.  Or you can use the scrollbars.

Click on a cell to load the cell and entity details into the side panel.

The toolbar at the top of the screen can be used to run the simulation, pause it, and then step it one tick at a time.  It also displays the current world time.  One minute passes for each tick.

## Environment

A work in progress.  The "dungeon" is a 256x256 tile room.  The border wall will emit either heat or cold throughout the day.  The heat dissipates into the room.
When water begins to heat, it lets off steam, which will raise the humidity that dissipates throughout the room.  There are stone pillars scattered around the room
that will slow down the passage of heat.  Water will also slow the spread of heat.

### TODO
* I have a cellular-automata based cavern generator that will be used here soon-ish.

## Lifeforms

### Algae

The algae cells are at the bottom of the food chain.  They roughly follow the rules of Conway's Game of Life, modified to use floating point values.
The cells are affected by temperature and humidity.  Too hot and they burn, too cold and they stop moving.

#### TODO
* Increased growth by feeding on oink corpses.

### Oinks

Oinks feed on algae.  They get hungry fast, and thirsty a bit slower.  They will move in flocks following the Boids algorithm.

#### TODO
* Reproductive urge, temperature sensitivity, wetness sensitivity, mortality, corpses.

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
