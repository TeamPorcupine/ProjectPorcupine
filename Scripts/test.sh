#! /bin/sh

endTestsFold=0 #stores whether the travis_fold:end:tests has been echoed yet

echo "Attempting Unit Tests"
echo 'travis_fold:start:compile'
/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -runEditorTests -nographics -EditorTestResultFile $(pwd)/EditorTestResults.xml -projectPath $(pwd) -logFile unity.log 
cat $(pwd)/unity.log
echo 'travis_fold:end:compile'

echo 'Show Results from Tests'
echo 'travis_fold:start:tests'
if [ ! -f $(pwd)/EditorTestResults.xml ]; then
    echo "Results file not found!"
    echo "travis_fold:end:tests"
    $endTestsFold = 1

    # at this point we know that the build has failed due to compilation errors
    # lets try to parse them out of unity.log and display them
    if [ -f $(pwd)/unity.log ]; then
        out=$(grep "CompilerOutput" unity.log)
        if [ "$out" != "" ]; then

            result=$(cat unity.log | sed -n '/CompilerOutput:/,/EndCompilerOutput/p')
            echo 'Build Failed! \nThe compiler generated the following messages:'
            echo $result
        fi
    fi
    exit 1
fi
cat $(pwd)/EditorTestResults.xml
if [ "$endTestsFold" == 0 ]; then
    echo 'travis_fold:end:tests'
fi

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