[Contributing](#contributing)  
[Resolving Merge Conflicts](#resolving-merge-conflicts)  
[Unity Version](#unity-version)  
[General resources](#general-resources)  
[Style Guidelines](#style-guidelines)  
[Best Practices for Contributing](#best-practices-for-contributing)



# Contributing

If you would like to contribute to this project by modifying/adding to the program code or various creative assets, read the [Best Practices for Contributing] below and feel free to follow the standard Github workflow:

1. Fork the project.
2. Clone your fork to your computer.
 * From the command line: `git clone https://github.com/<USERNAME>/ProjectPorcupine.git`
3. Change into your new project folder.
 * From the command line: `cd ProjectPorcupine`
4. [optional]  Add the upstream repository to your list of remotes.
 * From the command line: `git remote add upstream https://github.com/TeamPorcupine/ProjectPorcupine.git`
5. Create a branch for your new feature.
 * From the command line: `git checkout -b my-feature-branch-name`
6. [optional]  It is recommended that you rebase your branch on top of the latest master, to minimize excess commit messages.
 * From the command line:  
     `git fetch upstream master`  
     `git rebase upstream/master`  
6. Make your changes.
 * Avoid making changes to more files than necessary for your feature (i.e. refrain from combining your "real" pull request with incidental bug fixes). This will simplify the merging process and make your changes clearer.
 * Very much avoid making changes to the Unity-specific files, like the scene and the project settings unless absolutely necessary. Changes here are very likely to cause difficult to merge conflicts. Work in code as much as possible. (We will be trying to change the UI to be more code-driven in the future.) Making changes to prefabs should generally be safe -- but create a copy of the main scene and work there instead (then delete your copy of the scene before committing).
7. Commit your changes and push your branch to your fork.
  * From the command line:  
    `git add Assets/my-changed-file.cs`  
    `git add Assets/my-other-changed-file.cs`  
    `git commit -m "A descriptive commit message"`  
    `git push origin my-feature-branch-name`  
8. Make a "Pull Request" from your branch here on Github.
  * Include screenshots demonstrating your change if applicable.
9. For a video tutorial, please see: https://www.youtube.com/watch?v=R2fl17eEpwI

# Resolving Merge Conflicts

Depending on the order that Pull Requests get processed, your PR may result in a conflict and become un-mergable.  To correct this, do the following from the command line:  
  
Switch to your branch: `git checkout my-feature-branch-name`  
Pull in the lastest upstream changes: `git pull upstream master`  
Find out what files have a conflict: `git status`  

Edit the conflicting file(s) and look for a block that looks like this:  
    `<<<<<<< HEAD`  
    `my awesome change`  
    `=======`  
    `some other person's less awesome change`  
    `>>>>>>> some-branch`  

Replace all five (or more) lines with the correct version (yours, theirs, or
a combination of the two).  ONLY the correct content should remain (none of 
that "<<<<< HEAD" stuff.)

Then re-commit and re-push the file.  
  
  `git add the-changed-file.cs`  
  `git commit -m "Resolved conflict between this and PR #123"`  
  `git push origin my-feature-branch-name`  

The pull request should automatically update to reflect your changes.

# Unity Version
We are using Unity version 5.4 .  
All pull requests must build in 5.4 to be a valid patch.  

# General resources
* [Github Tutorial by Quill18](https://www.youtube.com/watch?v=R2fl17eEpwI)
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
    * If you are adding a large feaure, create an issue prefixed with "Discussion:" and be sure to take community feedback and get general approval before making your change and submitting a PR.

* Pull Requests represent final code. Please ensure they are:
     * Well tested by the author. It is the author's job to ensure their code works as expected.  
     * Be free of unnecessary log calls. Logging is great for debugging, but when a PR is made, log calls should only be present when there is an actual error or to warn of an unimplemented feature.
   
   If your code is untested, log heavy, or incomplete, prefix your PR with "WIP", so others know it is still being tested and shouldn't be considered for merging yet.

* Small changes are preferable over large ones. The larger a change is the more likely it is to conflict with the project and thus be denied. If your addition is large, be sure to extensively discuss it in an "issue" before you submit a PR, or even start coding.

* Document your changes in your PR. If you add a feature that you expect others to use, explain exactly how future code should interact with your additions. 

* Avoid making changes to more files than necessary for your feature (i.e. refrain from combining your "real" pull request with incidental bug fixes). This will simplify the merging process and make your changes clearer.

* Avoid making changes to the Unity-specific files, like the scene and the project settings unless absolutely necessary. Changes here are very likely to cause difficult merge conflicts. Work in code as much as possible. (We will be trying to change the UI to be more code-driven in the future.) Making changes to prefabs should generally be safe -- but create a copy of the main scene and work there instead (then delete your copy of the scene before committing).

* Include screenshots demonstrating your change if applicable. All UI changes should include screenshots.

* If you want to help with localization of the project you can submit your work over at [Localization Repo](https://github.com/QuiZr/ProjectPorcupineLocalization)

That's it! Following these guidelines will ensure that your additions are approved quickly and integrated into the project. Thanks for your contribution!


