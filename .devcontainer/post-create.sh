#!/usr/bin/env bash

WS="$(pwd)"
# Create codex directory if it doesn't exist
mkdir -p "$WS/codex"

# Ensure Claude CLI can write its state in the workspace (avoids /root permission issues)
mkdir -p "$WS/.claude"

# Write the auth.json file with OPENAI_API_KEY from environment
echo "{\"OPENAI_API_KEY\":\"${OPENAI_API_KEY}\"}" > "$WS/codex/auth.json"

echo "auth.json configured with OPENAI_API_KEY from environment"
echo "ANTHROPIC_API_KEY is set as environment variable for Claude Code"

# Install Claude Code CLI and OpenAI Codex CLI globally
echo "Installing Claude Code CLI and OpenAI Codex CLI..."
npm install -g @anthropic-ai/claude-code @openai/codex

echo "Claude Code is now available via 'claude' command"
echo "OpenAI Codex is now available via 'codex' command"