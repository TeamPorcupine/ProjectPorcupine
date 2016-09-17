#! /bin/sh

localUnity="C:\\Program Files\\Unity\\Editor\\Unity.exe"

echo "Attempting Unit Tests"
"$localUnity" -batchmode -runEditorTests -nographics -EditorTestResultFile $(pwd)/EditorTestResults.xml -projectPath $(pwd) -logFile unity.log 
#cat $(pwd)/unity.log

if [ ! -f $(pwd)/EditorTestResults.xml ]; then
    echo "Results file not found!"
    echo "Make sure that there are no Unity processes already open and try again."

    # at this point we know that the build has failed due to compilation errors
    # lets try to parse them out of unity.log and display them
    if [ -f $(pwd)/unity.log ]; then
        out=$(grep "CompilerOutput" unity.log)
        if [ "$out" != "" ]; then

            echo '\nBuild Failed! \nThe compiler generated the following messages:'
            echo | awk '/CompilerOutput:/,/EndCompilerOutput/' < unity.log #show lines in between compiler output "tags" including tags 

        fi
	rm $(pwd)/unity.log
    fi
    exit 1
fi
#cat $(pwd)/EditorTestResults.xml

rm $(pwd)/unity.log
#TERRIBLE error check logic - Please fix ASAP
errorCount=$(grep "failures" EditorTestResults.xml | awk -F"\"" '{print $8}') #find line with 'failures' and returns characters between quotation mark 8 and 9

if [ "$errorCount" != "0" ]; then
    echo $errorCount ' unit tests failed!'
     
     #show the exact unit test failure
    echo '\nThe following unit tests failed:'
    echo | grep 'success="False"' EditorTestResults.xml | grep 'test-case'
   
    rm $(pwd)/EditorTestResults.xml
    exit 1
fi

errorCount=$(grep "failures" EditorTestResults.xml | awk -F"\"" '{print $6}') #now for errors

if [ "$errorCount" != "0" ]; then
    echo $errorCount ' unit tests threw errors!'
    rm $(pwd)/EditorTestResults.xml
    exit 1
fi

errorCount=$(grep "failures" EditorTestResults.xml | awk -F"\"" '{print $12}') #inconlusive tests

if [ "$errorCount" != "0" ]; then
    echo $errorCount ' unit tests were inconlusive!'
    rm $(pwd)/EditorTestResults.xml
    exit 1
fi


errorCount=$(grep "failures" EditorTestResults.xml | awk -F"\"" '{print $18}') #finally for invalid tests

if [ "$errorCount" != "0" ]; then
    echo $errorCount ' unit tests were invalid!'
    rm $(pwd)/EditorTestResults.xml
    exit 1
fi

rm $(pwd)/EditorTestResults.xml
#end of unit test checks. at this point the test have suceeded or exited with an error code.
