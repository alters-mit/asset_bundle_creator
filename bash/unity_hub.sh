#!/bin/bash

export DISPLAY=$1
mkdir ~/unity_hub -p
cd unity_hub
wget https://public-cdn.cloud.unity3d.com/hub/prod/UnityHub.AppImage
chmod +x UnityHub.AppImage
./UnityHub.AppImage --headless install --version 2020.3.24f1 --changeset 79c78de19888
~/Unity/Hub/Editor/2020.3.24f1/Editor/Unity -quit -batchmode -serial $4 -username $2 -password $3
./UnityHub.AppImage --headless install-modules --version 2020.3.24f1 --changeset 79c78de19888 -m windows -m mac -linux
