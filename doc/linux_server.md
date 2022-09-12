# How to install Unity Editor on a Linux server

### 1. On a personal computer:

1. [Install Unity Hub.](https://unity3d.com/get-unity/download)
2. [Manually activate a license.](https://docs.unity3d.com/Manual/ManualActivationGuide.html) Save the license serial number.

### 2. On a Linux server:

1. `cd ~`
2. `git clone https://github.com/alters-mit/asset_bundle_creator.git`
3. `cd asset_bundle_creator/bash`
4. `chmod +x unity_hub.sh`
5. `./unity_hub.sh DISPLAY USERNAME PASSWORD LICENSE`

**`unity_hub.sh` requirements:**

- An Internet connection (it will need to download Unity Hub, Unity Editor, etc.)
- A valid X virtual display

**`unity_hub.sh` arguments:**

- `DISPLAY` is the X11 display index, for example `:0`
- `USERNAME` and `PASSWORD` are your Unity account credentials
- `LICENSE` is the license serial number
