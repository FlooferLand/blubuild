#!/bin/bash

godot=$(cat "./scripts/bash/godot_path.txt")

function game() {
	$godot --path "./" ---- "$@" &
}

# Building
dotnet build Blubuild.csproj --verbosity quiet > /dev/null

# Server
game "--server"
sleep 0.01

# Clients
game "--client=1"
game "--client=2"

# Waiting for a process to end
wait -n
