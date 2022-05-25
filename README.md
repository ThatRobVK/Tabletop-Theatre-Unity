# Tabletop Theatre

## About

Tabletop Theatre (TT) is an immersive 3D Virtual TableTop, allowing you to play tabletop roleplaying games online with the full videogame experience.

### Releases

TT has not been released yet. Releases for users will be hosted on the website.

## Contributing

TT is open source to encourage the creative and technical community around tabletop RPGs to contribute fixes, features and additional assets. Before contributing, please see [the contributing guidelines](https://github.com/ThatRobVK/Tabletop-Theatre/blob/main/CONTRIBUTING.md) which goes into detail on the design ethos, feature roadmap, feature and bug bounties, and licensing.

## Getting started

### Unity requirements

TT is built on Unity 2021.3.latest (LTS) on the built-in render pipeline. This is likely to be upgraded to 2022 LTS with the universal render pipeline in the near future. 

### Setting up the project

1. Clone the repository.
1. Unzip the file `Library.zip` into the root of the cloned folder, this gives you an asset database that allows the assets below to be loaded correctly.
1. Copy the contents of the `LicensedAssets` folder into the `Assets` folder, so that you get `Assets\Licensed` and `Assets\Plugins\Licensed`. This gives you mock versions of licensed assets, allowing your copy to load, build and run without requiring licenses to these assets.
1. Fire up Unity and load the project.
1. The project should now load and run.

#### Note:
The Library folder is meant to be project specific and wouldn't normally be stored in source control. However, without the Asset Database in this folder, all assets will be re-imported and you will get errors on some scripts. By using the same asset database, you'll avoid these errors.

### But... it looks awful!

Heh, yeah, about that... As I am a development studio of 1, and not an artist in any way, TT relies on a number of third-party assets for a lot of the visual elements. As these assets are licensed to the individual developer (whether paid for or not), these cannot be distributed as part of an open source project.

For any assets that can't be included, mock versions are included to allow TT to build and run on contributors' machines. This includes images, in-game assets as well as third-party code.

Any assets created specifically for TT are included in the `Assets\Content` folder and are covered under [the open source license of this project](https://github.com/ThatRobVK/Tabletop-Theatre/blob/main/LICENSE).

All core functionality is custom-built for TT and will work when you clone the repo, but some non-core features such as highlighting and UI effects are mocked out and will not work.

## Credits

Maintainer: [RVK](https://github.com/ThatRobVK)

