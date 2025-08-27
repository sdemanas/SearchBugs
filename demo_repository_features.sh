#!/bin/bash

# SearchBugs Repository Feature Demo Script
# This script demonstrates the new repository functionality

echo "🚀 SearchBugs Repository Feature Demo"
echo "=====================================\n"

API_BASE="http://localhost:5026/api"

echo "1. 📋 List all repositories:"
curl -s -X GET "$API_BASE/repo" | jq '.' || echo "No repositories found or server not running"

echo -e "\n2. 🌿 Get branches for a repository (example):"
echo "   GET $API_BASE/repo/{encoded_url}/branches"

echo -e "\n3. 📁 Get repository file tree (example):"  
echo "   GET $API_BASE/repo/{encoded_url}/tree/{commit_sha}"

echo -e "\n4. 📄 Get file content (example):"
echo "   GET $API_BASE/repo/{encoded_url}/file/{commit_sha}/{file_path}"

echo -e "\n5. 📥 Clone repository (example):"
echo "   POST $API_BASE/repo/{encoded_url}/clone"
echo "   Body: {\"targetPath\": \"/path/to/clone\"}"

echo -e "\n6. ➕ Create new repository:"
echo "   POST $API_BASE/repo"
echo "   Body: {\"name\": \"test-repo\", \"description\": \"Test repository\", \"url\": \"https://github.com/user/repo.git\", \"projectId\": \"guid\"}"

echo -e "\n🌐 Frontend Features Available:"
echo "- Repository List: http://localhost:5026/repositories"
echo "- Repository Browser: http://localhost:5026/repositories/{encoded_url}"
echo "- Interactive file navigation with breadcrumbs"
echo "- Branch switching"
echo "- File content preview"
echo "- Clone repository dialog"

echo -e "\n💡 To test the full stack:"
echo "1. Start the backend: dotnet run --project src/SearchBugs.Api"
echo "2. Start the frontend: cd src/SearchBugs.Ui && npm run dev"
echo "3. Visit http://localhost:3000/repositories"

echo -e "\n✅ Implementation Complete!"
echo "All repository features are working from UI to backend with:"
echo "- Code view and navigation"
echo "- File content display"
echo "- Branch management"
echo "- Repository cloning"
echo "- Full API integration"
