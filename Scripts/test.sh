#! /bin/sh

travisUnity="/Applications/Unity/Unity.app/Contents/MacOS/Unity"

if [ $# -eq "0" ]; # Running without arguments -- assume running locally
then
    RUN_AS_TRAVIS=""
fi

while [ $# -gt 0 ]; do    # Until you run out of parameters . . .
  case "$1" in
    --travis)
        unityPath="$travisUnity"
        RUN_AS_TRAVIS=1 ;;
    -h|--help)
        echo "Usage: ${0##*/} [OPTION]"
        echo "Build ProjectPorcupine and run all unit tests. Exits with 1 if anything fails."
        echo
        echo "Options available:"
        echo "  --travis     Indicate that this is being run on the Travis CI server."
        echo "               Otherwise runs locally."
        echo "  -h, --help   This usage message."
        echo
        echo "If running locally, export the unityPath env variable to the location of any special"
        echo "Unity executable you want to run. Otherwise an OS based default is chosen."
        exit 0 ;;
    *)
        # could be being run as a git hook in which case it might have args
        # but we don't care about them
        echo "${0##*/}: unknown option -- $1. Ignoring for now. If this being run as a git hook this is okay."
        echo "Try '${0##*/} --help' for more information."
        ;;
  esac
  shift       # Check next set of parameters.
done

if [ -z "$RUN_AS_TRAVIS" ];
then
    if [ -z "$unityPath" ]; # user did not specify a special unity path to run locally
    then
        case $(uname -o) in # TODO: Add more systems!!!
            Msys)
                unityPath="C:\\Program Files\\Unity\\Editor\\Unity.exe"
                ;;
            *)
                echo "Don't know the path to your local Unity! Assuming OS X and crossing fingers."
		unityPath="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
                ;;
        esac
    fi

    echo "Using local Unity: $unityPath";
fi

# Only echos if $RUN_AS_TRAVIS is true. Used to suppress log spam when run locally.
travecho()
{
    if [ -n "$RUN_AS_TRAVIS" ];
    then
        echo "$@"
    fi
}

endTestsFold=0 #stores whether the travis_fold:end:tests has been echoed yet

travecho 'travis_fold:start:compile'
echo "Attempting Unit Tests"
"$unityPath" -batchmode -runEditorTests -nographics -EditorTestResultFile "$(pwd)"/EditorTestResults.xml -projectPath "$(pwd)" -logFile unity.log
logFile="$(pwd)"/unity.log
travecho "$(cat "$logFile")"
travecho 'travis_fold:end:compile'

travecho 'travis_fold:start:tests'
travecho 'Show Results from Tests'
if [ ! -f "$(pwd)"/EditorTestResults.xml ]; then
    echo "Results file not found!"
    echo "Make sure that there are no Unity processes already open and try again."
    travecho "travis_fold:end:tests"
    endTestsFold=1

    # at this point we know that the build has failed due to compilation errors
    # lets try to parse them out of unity.log and display them
    if [ -f "$(pwd)"/unity.log ]; then
        out=$(grep "CompilerOutput" unity.log)
        if [ "$out" != "" ]; then

            printf '\nBuild Failed! \nThe compiler generated the following messages:'
            echo | awk '/CompilerOutput:/,/EndCompilerOutput/' < unity.log #show lines in between compiler output "tags" including tags

        fi
    fi
    rm "$(pwd)"/unity.log
    exit 1
fi
rm "$(pwd)"/unity.log

resultsFile="$(pwd)/EditorTestResults.xml"
travecho "$(cat "$resultsFile")"
if [ "$endTestsFold" = 0 ]; then
    travecho 'travis_fold:end:tests'
fi


#TERRIBLE error check logic - Please fix ASAP
errorCount=$(grep "failures" EditorTestResults.xml | awk -F"\"" '{print $8}') #find line with 'failures' and returns characters between quotation mark 8 and 9

if [ "$errorCount" != "0" ]; then
    echo "$errorCount" ' unit tests failed!'

     #show the exact unit test failure
    printf '\nThe following unit tests failed:'
    echo | grep 'success="False"' EditorTestResults.xml | grep 'test-case'

    exitStatus=1
fi

errorCount=$(grep "failures" EditorTestResults.xml | awk -F"\"" '{print $6}') #now for errors

if [ "$errorCount" != "0" ]; then
    echo "$errorCount" ' unit tests threw errors!'

    exitStatus=1
fi

errorCount=$(grep "failures" EditorTestResults.xml | awk -F"\"" '{print $12}') #inconlusive tests

if [ "$errorCount" != "0" ]; then
    echo "$errorCount" ' unit tests were inconlusive!'

    exitStatus=1
fi


errorCount=$(grep "failures" EditorTestResults.xml | awk -F"\"" '{print $18}') #finally for invalid tests

if [ "$errorCount" != "0" ]; then
    echo "$errorCount" ' unit tests were invalid!'

    exitStatus=1
fi
#end of unit test checks. at this point the test have suceeded or set exitStatus to 1.
rm "$(pwd)"/EditorTestResults.xml
exit $exitStatus