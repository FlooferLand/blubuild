#!/bin/bash

godot=$(cat "./scripts/bash/godot_path.txt")

dotnet build Blubuild.csproj --verbosity quiet > /dev/null
$godot --path "./"
