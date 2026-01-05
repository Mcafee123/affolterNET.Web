#!/bin/bash
# Post-import script to fix scope-based permissions in Keycloak
# Run this after Keycloak starts with realm import
#
# Can run locally (https://localhost:8443) or in Docker (https://keycloak:8443)
set -e

KEYCLOAK_URL="${KEYCLOAK_URL:-https://localhost:8443}"
ADMIN_USER="${KEYCLOAK_ADMIN:-admin}"
ADMIN_PASS="${KEYCLOAK_ADMIN_PASSWORD:-admin}"
REALM="demo"
CLIENT_ID_NAME="bff-client"

# Use -k for self-signed certs, or set CA_CERT for custom CA
CURL_OPTS="-s"
if [ -n "$SSL_CERT_FILE" ] && [ -f "$SSL_CERT_FILE" ]; then
  CURL_OPTS="$CURL_OPTS --cacert $SSL_CERT_FILE"
else
  CURL_OPTS="$CURL_OPTS -k"
fi

echo "=== Keycloak Permission Fix Script ==="
echo "Keycloak URL: $KEYCLOAK_URL"

# Wait for Keycloak to be ready
echo "Waiting for Keycloak to be ready..."
for i in $(seq 1 30); do
  if curl $CURL_OPTS "$KEYCLOAK_URL/health/ready" 2>/dev/null | grep -q "UP"; then
    echo "Keycloak is ready!"
    break
  fi
  if [ "$i" -eq 30 ]; then
    echo "ERROR: Keycloak did not become ready in time"
    exit 1
  fi
  echo "  Attempt $i/30..."
  sleep 2
done

# Get admin token
echo "Getting admin token..."
TOKEN=$(curl $CURL_OPTS -X POST "$KEYCLOAK_URL/realms/master/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=$ADMIN_USER" \
  -d "password=$ADMIN_PASS" \
  -d "grant_type=password" \
  -d "client_id=admin-cli" | python3 -c "import sys,json; print(json.load(sys.stdin)['access_token'])")

if [ -z "$TOKEN" ]; then
  echo "ERROR: Failed to get admin token"
  exit 1
fi
echo "Token obtained."

# Get bff-client UUID
echo "Getting bff-client ID..."
CLIENT_UUID=$(curl $CURL_OPTS "$KEYCLOAK_URL/admin/realms/$REALM/clients?clientId=$CLIENT_ID_NAME" \
  -H "Authorization: Bearer $TOKEN" | python3 -c "import sys,json; print(json.load(sys.stdin)[0]['id'])")
echo "Client UUID: $CLIENT_UUID"

# Helper function to get resource ID by name
get_resource_id() {
  curl $CURL_OPTS "$KEYCLOAK_URL/admin/realms/$REALM/clients/$CLIENT_UUID/authz/resource-server/resource?name=$1" \
    -H "Authorization: Bearer $TOKEN" | python3 -c "import sys,json; print(json.load(sys.stdin)[0]['_id'])"
}

# Helper function to get scope ID by name
get_scope_id() {
  curl $CURL_OPTS "$KEYCLOAK_URL/admin/realms/$REALM/clients/$CLIENT_UUID/authz/resource-server/scope?name=$1" \
    -H "Authorization: Bearer $TOKEN" | python3 -c "import sys,json; print(json.load(sys.stdin)[0]['id'])"
}

# Helper function to get policy ID by name
get_policy_id() {
  curl $CURL_OPTS "$KEYCLOAK_URL/admin/realms/$REALM/clients/$CLIENT_UUID/authz/resource-server/policy?name=$(echo $1 | sed 's/ /%20/g')&type=role" \
    -H "Authorization: Bearer $TOKEN" | python3 -c "import sys,json; print(json.load(sys.stdin)[0]['id'])"
}

# Helper function to delete permission by name
delete_permission() {
  local name="$1"
  local encoded_name=$(echo "$name" | sed 's/ /%20/g')
  local perm_id=$(curl $CURL_OPTS "$KEYCLOAK_URL/admin/realms/$REALM/clients/$CLIENT_UUID/authz/resource-server/permission?name=$encoded_name" \
    -H "Authorization: Bearer $TOKEN" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d[0]['id'] if d else '')" 2>/dev/null || echo "")

  if [ -n "$perm_id" ]; then
    curl $CURL_OPTS -X DELETE "$KEYCLOAK_URL/admin/realms/$REALM/clients/$CLIENT_UUID/authz/resource-server/permission/scope/$perm_id" \
      -H "Authorization: Bearer $TOKEN"
    echo "  Deleted: $name"
  fi
}

# Helper function to create scope permission
create_scope_permission() {
  local name="$1"
  local resource_id="$2"
  local scope_ids="$3"
  local policy_ids="$4"
  local decision_strategy="${5:-UNANIMOUS}"

  curl $CURL_OPTS -X POST "$KEYCLOAK_URL/admin/realms/$REALM/clients/$CLIENT_UUID/authz/resource-server/permission/scope" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "{
      \"name\": \"$name\",
      \"type\": \"scope\",
      \"logic\": \"POSITIVE\",
      \"decisionStrategy\": \"$decision_strategy\",
      \"resources\": [$resource_id],
      \"scopes\": [$scope_ids],
      \"policies\": [$policy_ids]
    }" | python3 -c "import sys,json; d=json.load(sys.stdin); print(f'  Created: {d.get(\"name\", \"ERROR\")}' if 'id' in d else f'  Error: {d}')"
}

echo ""
echo "Getting resource IDs..."
ADMIN_RES=$(get_resource_id "admin-resource")
USER_RES=$(get_resource_id "user-resource")
echo "  admin-resource: $ADMIN_RES"
echo "  user-resource: $USER_RES"

echo ""
echo "Getting scope IDs..."
VIEW_SCOPE=$(get_scope_id "view")
MANAGE_SCOPE=$(get_scope_id "manage")
READ_SCOPE=$(get_scope_id "read")
CREATE_SCOPE=$(get_scope_id "create")
UPDATE_SCOPE=$(get_scope_id "update")
DELETE_SCOPE=$(get_scope_id "delete")
echo "  view: $VIEW_SCOPE"
echo "  manage: $MANAGE_SCOPE"
echo "  read: $READ_SCOPE"
echo "  create: $CREATE_SCOPE"
echo "  update: $UPDATE_SCOPE"
echo "  delete: $DELETE_SCOPE"

echo ""
echo "Getting policy IDs..."
ADMIN_POL=$(get_policy_id "Admin Role Policy")
USER_POL=$(get_policy_id "User Role Policy")
VIEWER_POL=$(get_policy_id "Viewer Role Policy")
echo "  Admin Role Policy: $ADMIN_POL"
echo "  User Role Policy: $USER_POL"
echo "  Viewer Role Policy: $VIEWER_POL"

echo ""
echo "Deleting broken scope permissions..."
delete_permission "Admin View Permission"
delete_permission "Admin Manage Permission"
delete_permission "User Read Permission"
delete_permission "User Write Permission"

echo ""
echo "Creating scope permissions with proper links..."

# Admin View Permission
create_scope_permission "Admin View Permission" \
  "\"$ADMIN_RES\"" \
  "\"$VIEW_SCOPE\"" \
  "\"$ADMIN_POL\"" \
  "UNANIMOUS"

# Admin Manage Permission
create_scope_permission "Admin Manage Permission" \
  "\"$ADMIN_RES\"" \
  "\"$MANAGE_SCOPE\"" \
  "\"$ADMIN_POL\"" \
  "UNANIMOUS"

# User Read Permission (AFFIRMATIVE - either User OR Viewer role)
create_scope_permission "User Read Permission" \
  "\"$USER_RES\"" \
  "\"$READ_SCOPE\"" \
  "\"$USER_POL\",\"$VIEWER_POL\"" \
  "AFFIRMATIVE"

# User Write Permission
create_scope_permission "User Write Permission" \
  "\"$USER_RES\"" \
  "\"$CREATE_SCOPE\",\"$UPDATE_SCOPE\",\"$DELETE_SCOPE\"" \
  "\"$USER_POL\"" \
  "UNANIMOUS"

echo ""
echo "=== Verifying RPT token ==="
ADMIN_TOKEN=$(curl $CURL_OPTS -X POST "$KEYCLOAK_URL/realms/$REALM/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=admin@example.com" \
  -d "password=admin123" \
  -d "grant_type=password" \
  -d "client_id=bff-client" \
  -d "client_secret=bff-client-secret" | python3 -c "import sys,json; print(json.load(sys.stdin).get('access_token', ''))")

RPT_RESULT=$(curl $CURL_OPTS -X POST "$KEYCLOAK_URL/realms/$REALM/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d "grant_type=urn:ietf:params:oauth:grant-type:uma-ticket" \
  -d "audience=bff-client")

echo "$RPT_RESULT" | python3 -c "
import sys,json,base64
d = json.load(sys.stdin)
if 'access_token' in d:
    payload = d['access_token'].split('.')[1]
    payload += '=' * (4 - len(payload) % 4)
    decoded = json.loads(base64.urlsafe_b64decode(payload))
    perms = decoded.get('authorization', {}).get('permissions', [])
    print('SUCCESS! RPT obtained with', len(perms), 'permissions:')
    for p in perms:
        scopes = ', '.join(p.get('scopes', [])) or 'all'
        print(f'  - {p[\"rsname\"]}: {scopes}')
else:
    print('ERROR:', d.get('error_description', d))
"

echo ""
echo "=== Done ==="
