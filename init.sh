#!/bin/bash

# Prompt user for variables
read -p "Enter project name: " PROJECT_NAME
read -p "Enter author name: " AUTHOR_NAME

# Replace in all files
find . -type f ! -name "init.sh" -exec sed -i '' \
    -e "s/{{PROJECT_NAME}}/${PROJECT_NAME}/g" \
    -e "s/{{AUTHOR_NAME}}/${AUTHOR_NAME}/g" {} +

# Optionally rename files and folders
find . -depth -name '*{{PROJECT_NAME}}*' | while read file; do
    newfile=$(echo "$file" | sed "s/{{PROJECT_NAME}}/${PROJECT_NAME}/g")
    mv "$file" "$newfile"
done

git add *
git commit -m "Template initialisation for ${PROJECT_NAME} by ${AUTHOR_NAME}"
git push


echo "Template initialisation complete."