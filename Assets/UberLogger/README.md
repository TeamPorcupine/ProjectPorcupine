# UberLogger
UberLogger is a free, modular, opensource replacement for Unity's
Debug.Log system. It also includes UberConsole, a replacement for
Unity's editor console, an in game console version of UberConsole and
a file logger.

UberConsole looks like this:

![](Pics/UberConsoleEditor.png)

And the in-game version looks like this:

![](Pics/UberConsoleGame.png)

## Core Features
* Drop in replacement for Unity's Debug.Log() methods. No code changes
  needed.
* A threadsafe, modular backend that supports any number of loggers,
  so Debug.Log can be routed to multiple locations. The supplied file
  logger should work on mobile devices.
* Included are a replacement editor console, an in-game console and a
  file logger.
* Support for named debug channels - Debug.LogChannel("Boot", "Some
  message") can be filtered based on the channel name.
* Methods may be marked as excluded from callstacks by tagging them
  with '[StackTraceIgnore]', to keep your logs tidy.

## UberConsole Features
* More compact view shows more errors in the same space.
* Supports debug log channels
* Messages can be filtered by regular expressions
* Timestamps
* Source code can be shown inline with the callstack.
* An in-game version of UberConsole is also provided.

## Installation
* Stick the UberLogger folder somewhere under your Unity project
  Assets folder.
* To view the editor UberConsole, go to Window->Show Uber Console.
* To use UberConsole in your game, drag the UberAppConsole prefab into
  your scene.
* Usage examples can be seen in the Examples folder.

## Notes
* Currently only tested in Unity 4 and 5 Free, but should work in Pro.
* Due to file incompatibilities, the in-game console skin doesn't work
  in Unity 4 and would need to be set up again. Same with the
  prefab. That said, the code works, and so does the editor UberConsole.
* Pull requests welcome!

 * * * *

[UberLogger]: https://github.com/bbbscarter/UberLogger
