#!/bin/bash

# Prompt for issue number
read -p "Enter the GitHub issue number to resolve: " issue_number
git checkout main
git pull

# Check if input is a number
if ! [[ "$issue_number" =~ ^[0-9]+$ ]]; then
  echo "Error: Issue number must be a positive integer."
  exit 1
fi

# Construct and run the codex command
codex exec -s danger-full-access "Use gh to read issue #$issue_number and its comments. Resolve the issue. Add any required tests. Ensure the project builds with no errors and no warnings. Add, commit, push and create a detailed PR which adds the text Closes #$issue_number. Update documentation."
