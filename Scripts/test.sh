#! /bin/sh

echo "Attempting Unit Tests"
echo 'travis_fold:start:compile'
/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -runEditorTests -nographics -EditorTestResultFile $(pwd)/EditorTestResults.xml -projectPath $(pwd) -logFile unity.log 
cat $(pwd)/unity.log
echo 'travis_fold:end:compile'

echo 'Show Results from Tests'
echo 'travis_fold:start:tests'
if [ ! -f $(pwd)/EditorTestResults.xml ]; then
    echo "Results file not found!"
	exit 1
fi
cat $(pwd)/EditorTestResults.xml
echo 'travis_fold:end:tests'
#can't do windows and linux builds because unity by default installs only with build module
#for the platform your on. 
