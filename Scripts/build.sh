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
rm $(pwd)/unity.log

echo "Attempting to build $project for Windows 32-bit"
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -nographics \
  -silent-crashes \
  -logFile $(pwd)/unity.log \
  -projectPath $(pwd) \
  -buildWindowsPlayer "$(pwd)/Build/windows/32-bit/$project.exe" \
  -quit

echo 'Logs from build'
cat $(pwd)/unity.log
rm $(pwd)/unity.log


echo "Attempting to build $project for Windows 64-bit"
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -nographics \
  -silent-crashes \
  -logFile $(pwd)/unity.log \
  -projectPath $(pwd) \
  -buildWindows64Player "$(pwd)/Build/windows/64-bit/$project.exe" \
  -quit

echo 'Logs from build'
cat $(pwd)/unity.log


#removed windows and linux builds. Perhaps they can be fixed
