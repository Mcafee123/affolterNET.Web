#!/bin/bash
set -e

# If custom CA certificate exists, add it to the system trust store
if [ -f "/https/rootCA.pem" ]; then
    echo "Adding custom CA certificate to trust store..."
    cp /https/rootCA.pem /usr/local/share/ca-certificates/mkcert-ca.crt
    update-ca-certificates
fi

# Start the application
exec dotnet ExampleApi.dll
