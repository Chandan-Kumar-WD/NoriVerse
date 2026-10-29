#!/bin/zsh
# Launches the ElevenLabs MCP server with the key from ~/.zshrc, since GUI-launched apps do not load shell config.
# The key is read into the environment only; it is never printed or stored here.
if [[ -z "$ELEVENLABS_API_KEY" || "$ELEVENLABS_API_KEY" == *'${'* ]]; then
  line=$(grep -E '^export ELEVENLABS_API_KEY=' "$HOME/.zshrc" | tail -1)
  eval "$line"
fi
export ELEVENLABS_API_KEY
export ELEVENLABS_MCP_BASE_PATH="$HOME/Documents/NoriVerse/Tools/AudioPipeline/raw"
exec "$HOME/.local/bin/uvx" elevenlabs-mcp
