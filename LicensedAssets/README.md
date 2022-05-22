# Licensed Assets

## Unlicensed assets

Tabletop Theatre uses a number of third-party and proprietary assets. As these assets are licensed, they cannot be distributed as part of an open source project.

This folder contains mock versions of these assets that allow the open source version of TT to build and run, without including any licensed content.

In the Plugins folder are fake generated DLLs that have the same classes and public method signatures as the original files, but no implementation. This allows the project to build, but no licensed functionality is included. Be aware that when you call these methods, nothing will happen. In the Licensed folder you will find placeholder files to allow this open source project to build without missing images and artefacts. All images have been replaced with ones that serve the same function but do not have the same fidelity as those created by the original artists. All shaders have been gutted and defer to a built-in Unity shader.

## Proprietary TT code

TT contains some proprietary code related to server communications, cloud-hosted asset loading, etc. This folder contains DLL without these features but with functionality suitable for local development, e.g. loading assets from a local folder, faking any server communications and returning appropriate responses, etc.

# How to use

This folder contains all the files required to build and run TT without using any licensed assets. Simply copy the contents of this folder into the Assets folder, so that you get `Assets\Licensed\*` and `Assets\Plugins\Licensed\*`. Now open the project in Unity and it should load and run without issue.

# Bring your own license

If you have a personal license to any of these assets, you may use them to contribute to Tabletop Theatre or build your own versions of TT, to the extent permitted by their individual licenses.

For code:
1. Create a new Unity project
2. Import the asset from the Asset Store
3. Add an assembly definition to the scripts folder and set its name to match the name of the DLL you wish to replace.
4. Build the project in Unity
5. Grab the DLL file from the `Library\ScriptAssemblies` folder in this new project
6. Copy this file and replace the DLL in `Assets\Plugins\Licensed` in TT, leaving the `.meta` file as-is

For other assets (images, shaders, etc):
1. Create a new Unity project (or use the one created above)
2. Import the asset from the Asset Store
3. Take the files you wish to use from the new project
4. Copy these files into `Assets\Licensed` in TT, replacing the existing files

# Contributing

If you make original contributions, do not place these in the `LicensedAssets`, `Assets\Licensed` or `Assets\Plugins\Licensed` folders as these are removed and/or replaced for official builds. Instead, put your contributions in other appropriate folders under `Assets`.

Do not introduce new licensed assets into TT. The [Unity Asset Store Terms of Service and EULA](https://unity3d.com/legal/as_terms) do not allow for the transfer of licenses and therefore any licensed assets that you have purchased cannot be used in official releases of TT. 
