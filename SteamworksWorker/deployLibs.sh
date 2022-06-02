#!/bin/bash

#Try to remove previously deployed file..
rm -f libsteam_api.so
rm -f steam_api64.dll
rm -f Steamworks.NET

path="SteamworksLibs/"
if [[ $OSTYPE == "linux-gnu" ]]; then
    path+="linux64"
#In the future?
#elif [[ $OSTYPE == "darwin" ]]; then 
#    path+="osx"
fi

cp $path/* . 