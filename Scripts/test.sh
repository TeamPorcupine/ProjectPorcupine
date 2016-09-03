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

#TERRIBLE error check logic - Please fix ASAP
errorCount=$(grep "failures" EditorTestResults.xml | awk -F"\"" '{print $8}') #find line with 'failures' and returns characters between quotation mark 8 and 9

if [ "$errorCount" != "0" ]; then
	echo $errorCount ' unit tests failed!'
	exit 1
fi

errorCount=$(grep "failures" EditorTestResults.xml | awk -F"\"" '{print $6}') #now for errors

if [ "$errorCount" != "0" ]; then
	echo $errorCount ' unit tests threw errors!'
	exit 1
fi

errorCount=$(grep "failures" EditorTestResults.xml | awk -F"\"" '{print $12}') #inconlusive tests

if [ "$errorCount" != "0" ]; then
	echo $errorCount ' unit tests were inconlusive!'
	exit 1
fi


errorCount=$(grep "failures" EditorTestResults.xml | awk -F"\"" '{print $18}') #finally for invalid tests

if [ "$errorCount" != "0" ]; then
	echo $errorCount ' unit tests were invalid!'
	exit 1
fi
#end of unit test checks. at this point the test have suceeded or exited with an error code.