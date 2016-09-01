#! /bin/sh

# Example build script for Unity3D project. See the entire example: https://github.com/JonathanPorta/ci-build

# Change this the name of your project. This will be the name of the final executables as well.

echo "Attempting Unit Tests"
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -runEditorTests \
  -editorTestsResultFile $(pwd)/unity.log \
  -projectPath $(pwd) \
  -quit


echo 'Results from Tests'
cat $(pwd)/unity.log
#can't do windows and linux builds because unity by default installs only with build module
#for the platform your on. 