# Contributing

If you would like to contribute to this project by modifying/adding to the program code or various creative assets, read the [Best Practices for Contributing] below and feel free to follow the standard Github workflow:

1. Fork the project.
2. Clone your fork to your computer.
3. Create a branch for your new feature.
4. Make your changes.
5. Commit your changes and push your branch to your fork.
  * You may want to pull in the lastest project games from the upstream
    master before pushing your own changes, to ensure that you are up
    to date.
6. Make a "Pull Request" (PR) from your branch here on Github.
7. For a video tutorial, please see: https://www.youtube.com/watch?v=-N4Cghw0l2Q

# Unity Version
We are using Unity version 5.4 .
All pull requests must build in 5.4 to be a valid patch.

# General resources
* [Github Tutorial by Quill18](https://www.youtube.com/watch?v=-N4Cghw0l2Q)
* [GitHub Forking Overview](https://gist.github.com/Chaser324/ce0505fbed06b947d962)
* [GitHub Documentation for Desktop Client](https://help.github.com/desktop/guides/contributing/)
* [GitHub Desktop Client](https://desktop.github.com/)
* [GitHub for Windows](https://git-for-windows.github.io/)
* [Quill18's Channel](https://www.youtube.com/channel/UCPXOQq7PWh5OdCwEO60Y8jQ)
* [Project Porcupine Playlist](https://www.youtube.com/playlist?list=PLbghT7MmckI4_VM5q3va043FgAwRim6yX)

## Style Guidelines

We have standardized on Microsoft's [C# Coding Conventions](https://msdn.microsoft.com/en-us/library/ff926074.aspx) and [General Naming Conventions](https://msdn.microsoft.com/en-us/library/ms229045(v=vs.110).aspx), with the following exception:

* Avoid using 'var', even when the type would be clear from context. Verbose typing is best typing.

As a TL;DR on our coding practises, adhere to the following example:

```c#
// Use camelCasing unless stated otherwise.
// Descriptive names for variables/methods should be used.
// Fields, properties and methods should always specify their scope, aka private/protected/internal/public.

// Interfaces start with an I and should use PascalCasing.
interface IInterfaceable { } 

// Class names should use PascalCasing.
// Braces are on a new line. ;)
class Class 
{
    // Fields backing properties start with an underscore and should be private.
    private int _memberField; 
    
    // Properties should use PascalCasing.
    public int MemberField { get { return _memberField; } } 
    
    // Regular fields not backing a property should be camelCased.
    private string someString;
    
    // Methods should use PascalCasing.
    // Method parameters should be camelCased.
    public void SomeMethod( int functionParameter ) 
    {
        // Local variables should also be camelCased.
        int myLocalVariable = 0; 
    } 
    
    // Events should use PascalCasing as well.
    public event SomeEvent; 
}
```

Additionally, for the sake of simplicity, we are standardizing on what should be Microsoft Visual Studio's default code formatting rules. If you are using MonoDevelop (as many of us are), please go to your preferences and set the C# source code formatting rules to the MVS setting:  

![screen shot 2016-08-16 at 8 03 22 pm](https://cloud.githubusercontent.com/assets/777633/17719999/920fb534-63ec-11e6-8903-3725f2cd05b0.png)
![screen shot 2016-08-16 at 8 03 36 pm](https://cloud.githubusercontent.com/assets/777633/17719998/920cff6a-63ec-11e6-8f76-0ac7a5fa0c9d.png)

## Best Practices for Contributing
[Best Practices for Contributing]: #best-practices-for-contributing
* Before you start coding, open an issue so that the community can discuss your change to ensure it is in line with the goals of the project and not being worked on by someone else. This allows for discussion and fine tuning of your feature and results in a more succent and focused additions.
    * If you are fixing a small glitch or bug, you may make a PR without opening an issue.
    * If you are adding a large feaure, create an issue prefixed with "Discusion:" and be sure to take community feedback and get general approval before making your change and submitting a PR.

* Pull Requests represent final code. Please ensure they are:
     * Well tested by the author. It is the author's job to ensure their code works as expected.  
     * Be free of unnecessary log calls. Logging is great for debugging, but when a PR is made, log calls should only be present when there is an actual error or to warn of an unimplemented feature.
   
   If your code is untested, log heavy, or incomplete, prefix your PR with "WIP", so others know it is still being tested and shouldn't be considered for merging yet.

* Small changes are preferable over large ones. The larger a change is the more likely it is to conflict with the project and thus be denied. If your addition is large, be sure to extensively discuss it in an "issue" before you submit a PR, or even start coding.

* Document your changes in your PR. If you add a feature that you expect others to use, explain exactly how future code should interact with your additions. 

* Avoid making changes to more files than necessary for your feature (i.e. refrain from combining your "real" pull request with incidental bug fixes). This will simplify the merging process and make your changes clearer.

* Avoid making changes to the Unity-specific files, like the scene and the project settings unless absolutely necessary. Changes here are very likely to cause difficult merge conflicts. Work in code as much as possible. (We will be trying to change the UI to be more code-driven in the future.) Making changes to prefabs should generally be safe -- but create a copy of the main scene and work there instead (then delete your copy of the scene before committing).

* Include screenshots demonstrating your change if applicable. All UI changes should include screenshots.

That's it! Following these guidelines will ensure that your additions are approved quickly and integrated into the project. Thanks for your contribution!


