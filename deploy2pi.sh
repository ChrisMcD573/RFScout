#!/bin/bash
# deploy.sh - Cross-compiles to arm64 and pushes RFScout to the Raspberry Pi

# --- Configuration (Update these for your Pi's network) ---
PI_USER="student" #user (maybe needs permissions)
PI_HOST="cit250" #host alias
TARGET_DIR="/home/$PI_USER/Projects/RFScout"
# ----------------------------------------------------------

echo " -----------------------------------------------------"
echo " ----------------- Compiling RFScout -----------------"
echo " ----------------- for ARM64 (RPI 4) -----------------"
echo " -----------------------------------------------------"

dotnet publish src/RFScout.Console/RFScout.Console.csproj \
    -c Release \
    -r linux-arm \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:EnableCompressionInSingleFile=true \
    -o ./deploy_dist

echo " -----------------------------------------------------"
echo " ----------------- Pushing to RPI 4  -----------------"
echo " -----------------------------------------------------"
# Double check the target directory is valid
ssh $PI_USER@$PI_HOST "mkdir -p $TARGET_DIR/logs"

# Kill the app if it's running (hide the error if it isn't)
ssh $PI_USER@$PI_HOST "killall RFScout.Console 2>/dev/null || true"

# Delete the old binary to completely prevent file lock errors
ssh $PI_USER@$PI_HOST "rm -f $TARGET_DIR/RFScout.Console"

# Push it - real good!
scp ./deploy_dist/RFScout.Console $PI_USER@$PI_HOST:$TARGET_DIR/ && echo " [SCP DONE] "

echo " -----------------------------------------------------"
echo " ----------------- Cleaning up local -----------------"
echo " -----------------------------------------------------"
rm -rf ./deploy_dist && echo " [CLEANED] "

echo " -----------------------------------------------------"
echo " ----------------- Deployment complete ---------------"
echo " -----------------------------------------------------"
ssh student@cit250