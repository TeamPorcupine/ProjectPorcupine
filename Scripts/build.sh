#! /bin/sh

# Example build script for Unity3D project. See the entire example: https://github.com/JonathanPorta/ci-build

# Change this the name of your project. This will be the name of the final executables as well.
project="project-porcupine"

echo "Attempting to build $project for OS X"
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -nographics \
  -silent-crashes \
  -logFile $(pwd)/unity.log \
  -projectPath $(pwd) \
  -buildOSXUniversalPlayer "$(pwd)/Build/osx/$project.app" \
  -quit

echo 'Logs from build'
cat $(pwd)/unity.log

echo 'Unit Tests'
/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -runEditorTests -quit -projectPath $(pwd)

#can't do windows and linux builds because unity by default installs only with build module
#for the platform your on. 