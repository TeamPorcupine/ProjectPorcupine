[Contributing](#contributing)  
[Resolving Merge Conflicts](#resolving-merge-conflicts)  
[Unity Version](#unity-version)  
[General resources](#general-resources)  
[Style Guidelines](#style-guidelines)  
[Adding Furniture and Inventory](#adding-new-types-of-furniture-inventory-commands)  
[Best Practices for Contributing](#best-practices-for-contributing)  
[Image & Sound File Formats](#file-formats)  

# Note

Please also read the [Contributor's Portal](../../wiki/Contributors'-Portal#important-links), mainly the [Contributing Guidelines](../../wiki/Contributing-Guidelines).

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
6. Make your changes.
 * Avoid making changes to more files than necessary for your feature (i.e. refrain from combining your "real" pull request with incidental bug fixes). This will simplify the merging process and make your changes clearer.
 * Very much avoid making changes to the Unity-specific files, like the scene and the project settings unless absolutely necessary. Changes here are very likely to cause difficult to merge conflicts. Work in code as much as possible. (We will be trying to change the UI to be more code-driven in the future.) Making changes to prefabs should generally be safe -- but create a copy of the main scene and work there instead (then delete your copy of the scene before committing).
7. Commit your changes. From the command line:
 * `git add Assets/my-changed-file.cs`
 * `git add Assets/my-other-changed-file.cs`
 * `git commit -m "A descriptive commit message"`
8. While you were working some other pull request might have gone in the breaks your stuff or vice versa. This can be a *merge conflict* but also conflicting game logic or code. Before you test, merge with master.
 * `git fetch upstream`
 * `git merge upstream/master`
9. Test. Start the game and do something related to your feature/fix.
10. Push the branch, uploading it to Github.
  * `git push origin my-feature-branch-name`
11. Make a "Pull Request" from your branch here on Github.
  * Include screenshots demonstrating your change if applicable.
12. For a video tutorial, please see: https://www.youtube.com/watch?v=R2fl17eEpwI

# Resolving Merge Conflicts

Depending on the order that Pull Requests get processed, your PR may result in a conflict and become un-mergable.  To correct this, do the following from the command line:

Switch to your branch: `git checkout my-feature-branch-name`
Pull in the latest upstream changes: `git pull upstream master`
Find out what files have a conflict: `git status`

Edit the conflicting file(s) and look for a block that looks like this:
```
<<<<<<< HEAD
my awesome change
=======
some other person's less awesome change
>>>>>>> some-branch
```

Replace all five (or more) lines with the correct version (yours, theirs, or
a combination of the two).  ONLY the correct content should remain (none of
that `<<<<< HEAD` stuff.)

Then re-commit and re-push the file.

```
git add the-changed-file.cs
git commit -m "Resolved conflict between this and PR #123"
git push origin my-feature-branch-name
```

The pull request should automatically update to reflect your changes.

# Unity Version
We are using Unity version 5.4.2.
All pull requests must build in 5.4.2 to be a valid patch.  Though you can use `#IF UNITY_X_Y_Z` with x, y, z referring to the version details such as 5.4.2 or 5.6.1, More details [here](https://docs.unity3d.com/Manual/PlatformDependentCompilation.html)

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

As a TL;DR on our coding practices, adhere to the following example:

```c#
// All files begin with the following license header (remove this line):
#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System; // System usings go first
using UnityEngine; // followed by any other using directives

// Use camelCasing unless stated otherwise.
// Descriptive names for variables/methods should be used.
// Fields, properties and methods should always specify their scope, aka private/protected/internal/public.

// Interfaces start with an I and should use PascalCasing.
public interface IInterfaceable
{
}

// Class names should use PascalCasing.
// Braces are on a new line. ;)

/// <summary>
/// Xml documentation comments are encouraged. Describe public APIs and the intent of code, not implementation details.
/// </summary>
public class Class
{
    // Private fields should be camelCased.
    // Use properties for any field that needs access levels other than private
    private string someField;

    // Events should use PascalCasing as well.
    // ✓ DO name events with a verb or a verb phrase.
    // Examples include Clicked, Painting, DroppedDown, and so on.
    // ✓ DO give events names with a concept of before and after, using the present and past tenses.
    // For example, a close event that is raised before a window is closed would be called Closing,
    // and one that is raised after the window is closed would be called Closed.
    public event EventHandler<EventArgs> SomeEvent;

    // Properties should use PascalCasing.
    public int MemberProperty { get; set; }

    // Methods should use PascalCasing.
    // Method parameters should be camelCased.
    public void SomeMethod(int functionParameter)
    {
        // Local variables should also be camelCased.
        int myLocalVariable = 0;
    }
}
```

Additionally, for the sake of simplicity, we are standardizing on what should be Microsoft Visual Studio's default code formatting rules. If you are using MonoDevelop (as many of us are), please go to your preferences and set the C# source code formatting rules to the MVS setting:  

![screen shot 2016-08-16 at 8 03 22 pm](https://cloud.githubusercontent.com/assets/777633/17719999/920fb534-63ec-11e6-8903-3725f2cd05b0.png)
![screen shot 2016-08-16 at 8 03 36 pm](https://cloud.githubusercontent.com/assets/777633/17719998/920cff6a-63ec-11e6-8f76-0ac7a5fa0c9d.png)

It is also highly recommended that you install [StyleCop](https://github.com/TeamPorcupine/ProjectPorcupine/wiki/StyleCop), which will automatically point out any deviations from the project's style guidelines. Any deviations in your code which can be tracked by StyleCop will result in the rejection of your Pull Request.

## Adding New Types of Furniture, Inventory, Commands...
There are multiple examples and it should be reasonably easy to add new types, when building a PR of just these you don't need to open an issue though it is encouraged for balancing purposes.  Any new types should be fully implemented in PR and shouldn't just be placeholders.

### Furniture/Inventory
We have standardized the Types of Furniture and Inventory to match `type_material`, such as `wall_steel` and `generator_oxygen` or `generator_power`, for localization a matching prefix is added automatically to Type such as `inv` and `furn`. This means a few things:

* When adding a new Furniture or inventory the files should have the Type "type_material", you could give it the more english sounding name, as of now name is not used for anything.

* For machines the convention will be `whatItDoes_whatItMakes`.

* For multiword parts it will be `myType_myMaterial`.

* In the Localization a line could be will be `inv_type_material=Material Type` and `furn_type_material=Material Type`.

* In Localization the description can be set with `inv_type_material_desc=Some cool description.` and `furn_type_material_desc=Some awesome description`.

* For image files and their xml files use the Type as the name.

## Best Practices for Contributing
[Best Practices for Contributing]: #best-practices-for-contributing
* Before you start coding, open an issue so that the community can discuss your change to ensure it is in line with the goals of the project and not being worked on by someone else. This allows for discussion and fine tuning of your feature and results in a more succent and focused additions.
    * If you are fixing a small glitch or bug, you may make a PR without opening an issue.
    * If you are adding a large feature, create an issue prefixed with "[Discussion]" and be sure to take community feedback and get general approval before making your change and submitting a PR.

* Pull Requests represent final code. Please ensure they are:
     * Well tested by the author. It is the author's job to ensure their code works as expected.
     * Be free of unnecessary log calls. Logging is great for debugging, but when a PR is made, log calls should only be present when there is an actual error or to warn of an unimplemented feature. Please use `UnityDebugger.Debugger` instead of Unity's `Debug.Log()`.

   If your code is untested, log heavy, or incomplete, prefix your PR with "[WIP]", so others know it is still being tested and shouldn't be considered for merging yet.

* Small changes are preferable over large ones. The larger a change is the more likely it is to conflict with the project and thus be denied. If your addition is large, be sure to extensively discuss it in an "issue" before you submit a PR, or even start coding.

* Changes to code that your PR isn't specifically fixing, try to keep your PR focused cause people may have problems with a small section of your PR but the rest is ready to be merged which complicates the merging procedure.

* Limit any change to code definitions (variables, functions, properties, and classes/structs/interfaces)
    * A restructure/overhaul is defined as the focus being on the entire system rather than a small module so almost every one of these 'cautions' are void in that case.
    * Any changes to functions should be limited, you should try to work 'within' the current project.  Any system 'restructures' or overhauls are obviously a different matter.  This is because if you have to manipulate and twist functions to achieve certain functionality you are most likely applying a hack, or the system is very limited in scope and in that case it is most likely a restructure or overhaul that you are doing.  
    * Adding new public variables should always be a cautionary procedure, so unless you are specifically exposing private variables for modding purposes or removing public variables since they aren't needed, you should take care with adding a bunch of public variables.  Exceptions being new systems entirely or system restructures.
    * Shouldn't remove any classes/interfaces/structs unless your PR is a restructure/overhaul.  

* Document your changes in your PR. If you add a feature that you expect others to use, explain exactly how future code should interact with your additions.

* Avoid making changes to more files than necessary for your feature (i.e. refrain from combining your "real" pull request with incidental bug fixes). This will simplify the merging process and make your changes clearer.

* Avoid making changes to the Unity-specific files, like the scene and the project settings unless absolutely necessary. Changes here are very likely to cause difficult merge conflicts. Work in code as much as possible. (We will be trying to change the UI to be more code-driven in the future.) Making changes to prefabs should generally be safe -- but create a copy of the main scene and work there instead (then delete your copy of the scene before committing).

* Include screenshots demonstrating your change if applicable. All UI changes should include screenshots.

* If you want to help with localization of the project you can submit your work over at [Localization Repo](https://github.com/QuiZr/ProjectPorcupineLocalization/tree/Someone_will_come_up_with_a_proper_naming_scheme_later)

That's it! Following these guidelines will ensure that your additions are approved quickly and integrated into the project. Thanks for your contribution!

## File Formats

In the primary GIT repo, we will only accept files in these formats:

### Images

Images should be in a compressed, ready-to share format like PNG or JPEG.  The original Photoshop/GIMP/etc... files are not to be commited to the repository.  However, hosting the originals somewhere (like a public Google Drive folder) as a reference would be appreciated (but not required).

### Sound Files & Music

As per Issue #795, we have standardized on the open-source friendly OGG audio format, as opposed to MP3 or uncompressed WAV files.  Small sound effects that are played frequently should likely be set to "Decompress on Load" in Unity's inspector, whereas longer sounds (such as songs) should not be.  Hosting the original, uncompressed WAV files somewhere (like a public Google Drive folder) as a reference would be appreciated (but not required).
