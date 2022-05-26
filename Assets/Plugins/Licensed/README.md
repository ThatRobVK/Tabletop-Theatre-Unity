# Licensed DLLs

## Licensed assets

Tabletop Theatre uses a number of third-party and proprietary assets. As these assets are licensed, they cannot be distributed as part of an open source project.

In this folder are fake generated DLLs that have the same classes and public method signatures as the original files, but no implementation. This allows the project to build, but no licensed functionality is included. Be aware that when you call these methods, nothing will happen.

## Bring your own license

If you have a personal license to any of these assets, you may use them to contribute to Tabletop Theatre or build your own versions of TT, to the extent permitted by their individual licenses. To do this, import the asset into a new Unity project, add an Assembly Definition to it and build it, then replace the DLL in this folder with the DLL from that project.

# Proprietary TT code

TT contains some proprietary code related to server communications, cloud-hosted asset loading, etc. This folder contains DLL without these features but with functionality suitable for local development, e.g. loading assets from a local folder, faking any server communications and returning appropriate responses, etc.
