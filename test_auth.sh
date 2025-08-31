#!/bin/bash

echo "Testing login..."
TOKEN_RESPONSE=$(curl -s -X POST "http://localhost:5026/api/v1/users/login" \
-H "Content-Type: application/json" \
-d '{"email": "nathvichea1@gmail.com", "password": "@Vichea123"}')

echo "Login response: $TOKEN_RESPONSE"

TOKEN=$(echo $TOKEN_RESPONSE | jq -r '.token')
echo "Extracted token: $TOKEN"

if [ "$TOKEN" != "null" ] && [ "$TOKEN" != "" ]; then
    echo "Testing protected endpoint with token..."
    curl -v -X GET "http://localhost:5026/api/v1/users" \
    -H "Authorization: Bearer $TOKEN"
else
    echo "Failed to get token"
fi
